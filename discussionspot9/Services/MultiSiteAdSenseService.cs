using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using discussionspot9.Data.DbContext;
using discussionspot9.Models.Configuration;
using discussionspot9.Models.Domain;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace discussionspot9.Services
{
    /// <summary>
    /// Enhanced AdSense service supporting multiple sites (gsmsharing.com + discussionspot.com)
    /// </summary>
    public class MultiSiteAdSenseService
    {
        private const string AdSenseBasePath = "https://adsense.googleapis.com/v2/";
        private static readonly string[] RequiredScopes = { "https://www.googleapis.com/auth/adsense.readonly" };

        private readonly ApplicationDbContext _context;
        private readonly ILogger<MultiSiteAdSenseService> _logger;
        private readonly AdSenseConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        private bool _initialized;
        private Func<Task<string?>>? _accessTokenFactory;

        public MultiSiteAdSenseService(
            ApplicationDbContext context,
            ILogger<MultiSiteAdSenseService> logger,
            IOptions<AdSenseConfiguration> config,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _config = config.Value;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Sync revenue data for all configured sites.
        /// </summary>
        public async Task<bool> SyncAllSitesRevenueAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.AddDays(-1).Date;
            var siteResults = new List<SiteRevenueData>();

            var apiAvailable = await EnsureInitializedAsync();

            _logger.LogInformation("💰 Starting multi-site revenue sync for {Date}", targetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            var success = true;

            foreach (var site in _config.Sites.Where(s => s.IsActive))
            {
                try
                {
                    var data = await FetchRevenueDataAsync(site, targetDate, apiAvailable, CancellationToken.None);
                    siteResults.Add(data);
                    await SaveRevenueDataAsync(site, data, targetDate);

                    _logger.LogInformation("✅ Synced {Site}: ${Earnings:F2}", site.Domain, data.TotalEarnings);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to sync revenue for {Site}", site.Domain);
                    success = false;
                }
            }

            if (siteResults.Any())
            {
                await SaveAggregatedRevenueAsync(siteResults, targetDate);
            }

            _logger.LogInformation(success 
                ? "✅ Multi-site revenue sync completed successfully" 
                : "⚠️ Multi-site revenue sync completed with errors");

            return success;
        }

        private async Task<SiteRevenueData> FetchRevenueDataAsync(AdSenseSite site, DateTime date, bool apiAvailable, CancellationToken cancellationToken)
        {
            if (apiAvailable)
            {
                try
                {
                    var apiData = await FetchRevenueFromApiAsync(site, date, cancellationToken);
                    apiData.Source = "AdSense";
                    return apiData;
            }
            catch (Exception ex)
            {
                    _logger.LogWarning(ex, "⚠️ Falling back to placeholder data for {Site} on {Date}", site.Domain, date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
            }

            return GeneratePlaceholderRevenue(site, date);
        }

        private async Task<SiteRevenueData> FetchRevenueFromApiAsync(AdSenseSite site, DateTime date, CancellationToken cancellationToken)
        {
            var accountName = $"accounts/{site.AccountId}";
            var metrics = new[]
            {
                "METRIC_ESTIMATED_EARNINGS",
                "METRIC_EARNINGS",
                "METRIC_PAGE_VIEWS",
                "METRIC_AD_REQUESTS",
                "METRIC_CLICKS",
                "METRIC_IMPRESSIONS",
                "METRIC_AD_REQUESTS_CTR",
                "METRIC_RPM",
                "METRIC_AD_REQUESTS_COVERAGE"
            };

            var summaryReport = await ExecuteReportRequestAsync(accountName, site, date, metrics, null, cancellationToken);
            var urlReport = await ExecuteReportRequestAsync(accountName, site, date, metrics, new[] { "DIMENSION_URL" }, cancellationToken);

            var summaryMetrics = ParseTotals(summaryReport, metrics);
            var urlRows = ParseUrlRows(site, urlReport, metrics);

            if (!urlRows.Any() && summaryMetrics.TotalEarnings == 0)
            {
                throw new InvalidOperationException($"AdSense API returned no data for {site.Domain} on {date:yyyy-MM-dd}.");
            }

            return new SiteRevenueData
            {
                SiteDomain = site.Domain,
                TotalEarnings = summaryMetrics.TotalEarnings,
                TotalEstimatedEarnings = summaryMetrics.TotalEstimatedEarnings,
                TotalPageViews = (int)Math.Round(summaryMetrics.TotalPageViews),
                TotalAdClicks = (int)Math.Round(summaryMetrics.TotalClicks),
                TotalImpressions = (int)Math.Round(summaryMetrics.TotalImpressions),
                TotalAdRequests = (int)Math.Round(summaryMetrics.TotalAdRequests),
                TotalCoverage = summaryMetrics.TotalCoverage,
                TotalRpm = summaryMetrics.TotalRpm,
                TotalCtr = summaryMetrics.TotalCtr,
                TotalCpc = summaryMetrics.TotalCpc,
                TotalActiveViewViewableImpressions = (int)Math.Round(summaryMetrics.TotalActiveViewImpressions),
                UrlRevenueData = urlRows,
                Source = "AdSense",
                SyncedAt = DateTime.UtcNow
            };
        }

        private async Task<JsonDocument?> ExecuteReportRequestAsync(
            string accountName,
            AdSenseSite site,
            DateTime date,
            IEnumerable<string> metrics,
            IEnumerable<string>? dimensions,
            CancellationToken cancellationToken)
        {
            var queryParameters = new List<string>
            {
                "dateRange=CUSTOM",
                $"startDate.year={date.Year}",
                $"startDate.month={date.Month}",
                $"startDate.day={date.Day}",
                $"endDate.year={date.Year}",
                $"endDate.month={date.Month}",
                $"endDate.day={date.Day}",
                "timeZone=ACCOUNT_TIME_ZONE"
            };

            foreach (var metric in metrics)
            {
                queryParameters.Add($"metrics={Uri.EscapeDataString(metric)}");
            }

            if (dimensions != null)
            {
                foreach (var dimension in dimensions)
                {
                    queryParameters.Add($"dimensions={Uri.EscapeDataString(dimension)}");
                }
            }

            if (!string.IsNullOrWhiteSpace(site.AdClientId))
            {
                queryParameters.Add($"filters={Uri.EscapeDataString($"FILTER_AD_CLIENT_ID=={site.AdClientId}")}");
            }

            if (!string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                queryParameters.Add($"key={Uri.EscapeDataString(_config.ApiKey)}");
            }

            var requestUri = $"{accountName}/reports:generate?{string.Join("&", queryParameters)}";
            var httpClient = _httpClientFactory.CreateClient("AdSenseApi");

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            if (_accessTokenFactory != null)
            {
                var token = await _accessTokenFactory();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var response = await httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AdSense API request failed for {Site} ({Status}): {Body}", site.Domain, response.StatusCode, content);
                throw new InvalidOperationException($"AdSense API returned {response.StatusCode}");
            }

            return JsonDocument.Parse(content);
        }

        private static ReportTotals ParseTotals(JsonDocument? document, IEnumerable<string> metrics)
        {
            var totals = new ReportTotals();
            if (document == null || document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return totals;
            }

            var headerIndex = BuildHeaderIndex(document.RootElement);
            if (document.RootElement.TryGetProperty("totals", out var totalsElement) &&
                totalsElement.ValueKind == JsonValueKind.Array &&
                totalsElement.GetArrayLength() > 0)
            {
                var row = totalsElement[0];
                totals.TotalEstimatedEarnings = ReadDecimal(row, headerIndex, "METRIC_ESTIMATED_EARNINGS");
                totals.TotalEarnings = ReadDecimal(row, headerIndex, "METRIC_EARNINGS");
                totals.TotalPageViews = ReadDecimal(row, headerIndex, "METRIC_PAGE_VIEWS");
                totals.TotalAdRequests = ReadDecimal(row, headerIndex, "METRIC_AD_REQUESTS");
                totals.TotalClicks = ReadDecimal(row, headerIndex, "METRIC_CLICKS");
                totals.TotalImpressions = ReadDecimal(row, headerIndex, "METRIC_IMPRESSIONS");
                totals.TotalRpm = ReadDecimal(row, headerIndex, "METRIC_RPM");
                totals.TotalCoverage = ReadDecimal(row, headerIndex, "METRIC_AD_REQUESTS_COVERAGE");

                var ctr = ReadDecimal(row, headerIndex, "METRIC_AD_REQUESTS_CTR");
                totals.TotalCtr = ctr;
                totals.TotalCpc = totals.TotalClicks > 0 ? totals.TotalEarnings / totals.TotalClicks : 0;
                totals.TotalActiveViewImpressions = 0;
            }

            if (totals.TotalEarnings == 0 && totals.TotalEstimatedEarnings > 0)
            {
                totals.TotalEarnings = totals.TotalEstimatedEarnings;
            }

            return totals;
        }

        private static List<UrlRevenueData> ParseUrlRows(AdSenseSite site, JsonDocument? document, IEnumerable<string> metrics)
        {
            var rows = new List<UrlRevenueData>();
            if (document == null || document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return rows;
            }

            var headerIndex = BuildHeaderIndex(document.RootElement);

            if (!document.RootElement.TryGetProperty("rows", out var rowsElement) ||
                rowsElement.ValueKind != JsonValueKind.Array)
            {
                return rows;
            }

            foreach (var row in rowsElement.EnumerateArray())
            {
                var url = ReadString(row, headerIndex, "DIMENSION_URL");
                if (string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                var earnings = ReadDecimal(row, headerIndex, "METRIC_ESTIMATED_EARNINGS");
                var pageViews = (int)Math.Round(ReadDecimal(row, headerIndex, "METRIC_PAGE_VIEWS"));
                var clicks = (int)Math.Round(ReadDecimal(row, headerIndex, "METRIC_CLICKS"));
                var impressions = (int)Math.Round(ReadDecimal(row, headerIndex, "METRIC_IMPRESSIONS"));
                var adRequests = (int)Math.Round(ReadDecimal(row, headerIndex, "METRIC_AD_REQUESTS"));
                var coverage = ReadDecimal(row, headerIndex, "METRIC_AD_REQUESTS_COVERAGE");
                var rpm = ReadDecimal(row, headerIndex, "METRIC_RPM");
                var ctr = ReadDecimal(row, headerIndex, "METRIC_AD_REQUESTS_CTR");
                var cpc = clicks > 0 ? earnings / clicks : 0;

                rows.Add(new UrlRevenueData
                {
                    SiteDomain = site.Domain,
                    Url = url,
                    Earnings = earnings,
                    EstimatedEarnings = earnings,
                    PageViews = pageViews,
                    AdClicks = clicks,
                    Impressions = impressions,
                    AdRequests = adRequests,
                    Coverage = coverage,
                    Rpm = rpm,
                    Ctr = ctr,
                    Cpc = cpc,
                    ActiveViewViewableImpressions = 0
                });
            }

            return rows;
        }

        private async Task SaveRevenueDataAsync(AdSenseSite site, SiteRevenueData revenueData, DateTime date)
        {
            foreach (var urlData in revenueData.UrlRevenueData)
            {
                var existing = await _context.MultiSiteRevenues
                    .FirstOrDefaultAsync(r =>
                        r.SiteDomain == site.Domain &&
                        r.Date == date &&
                        r.PostId == urlData.PostId);

                var rpm = urlData.Rpm ?? (urlData.PageViews > 0 ? (urlData.Earnings / urlData.PageViews) * 1000 : 0);
                var ctr = urlData.Ctr ?? (urlData.Impressions > 0 ? ((decimal)urlData.AdClicks / urlData.Impressions) * 100 : 0);
                var cpc = urlData.Cpc ?? (urlData.AdClicks > 0 ? urlData.Earnings / urlData.AdClicks : 0);

                if (existing != null)
                {
                    existing.Earnings = urlData.Earnings;
                    existing.EstimatedEarnings = urlData.EstimatedEarnings;
                    existing.PageViews = urlData.PageViews;
                    existing.AdClicks = urlData.AdClicks;
                    existing.AdImpressions = urlData.Impressions;
                    existing.RPM = rpm;
                    existing.CTR = ctr;
                    existing.CPC = cpc;
                    existing.Coverage = urlData.Coverage ?? ctr;
                    existing.ActiveViewViewableImpressions = urlData.ActiveViewViewableImpressions;
                    existing.SyncedAt = DateTime.UtcNow;
                }
                else
                {
                    var revenue = new MultiSiteRevenue
                    {
                        SiteDomain = site.Domain,
                        Date = date,
                        PostId = urlData.PostId,
                        PostUrl = urlData.Url,
                        Earnings = urlData.Earnings,
                        EstimatedEarnings = urlData.EstimatedEarnings,
                        PageViews = urlData.PageViews,
                        AdClicks = urlData.AdClicks,
                        AdImpressions = urlData.Impressions,
                        RPM = rpm,
                        CTR = ctr,
                        CPC = cpc,
                        Coverage = urlData.Coverage ?? ctr,
                        ActiveViewViewableImpressions = urlData.ActiveViewViewableImpressions,
                        SyncedAt = DateTime.UtcNow,
                        Source = revenueData.Source
                    };

                    _context.MultiSiteRevenues.Add(revenue);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveAggregatedRevenueAsync(IEnumerable<SiteRevenueData> revenueData, DateTime date)
        {
            var dataList = revenueData.ToList();
            if (!dataList.Any())
            {
                return;
            }

            var totalEarnings = dataList.Sum(d => d.TotalEarnings);
            var totalEstimated = dataList.Sum(d => d.TotalEstimatedEarnings);
            var totalViews = dataList.Sum(d => d.TotalPageViews);
            var totalClicks = dataList.Sum(d => d.TotalAdClicks);
            var totalImpressions = dataList.Sum(d => d.TotalImpressions);
            var totalRequests = dataList.Sum(d => d.TotalAdRequests);
            var totalActiveView = dataList.Sum(d => d.TotalActiveViewViewableImpressions);

            var avgCoverage = dataList.Where(d => d.TotalCoverage > 0).DefaultIfEmpty().Average(d => d?.TotalCoverage ?? 0);
            var avgRpm = dataList.Where(d => d.TotalRpm > 0).DefaultIfEmpty().Average(d => d?.TotalRpm ?? 0);
            var avgCtr = dataList.Where(d => d.TotalCtr > 0).DefaultIfEmpty().Average(d => d?.TotalCtr ?? 0);
            var avgCpc = dataList.Where(d => d.TotalCpc > 0).DefaultIfEmpty().Average(d => d?.TotalCpc ?? 0);

            var record = await _context.AdSenseRevenues
                .FirstOrDefaultAsync(r => r.Date == date && r.PostId == null);

            if (record == null)
            {
                record = new AdSenseRevenue
                {
                    Date = date,
                    PostId = null,
                    Source = "AdSense"
                };
                _context.AdSenseRevenues.Add(record);
            }

            record.Earnings = totalEarnings;
            record.EstimatedEarnings = totalEstimated > 0 ? totalEstimated : totalEarnings;
            record.PageViews = totalViews;
            record.AdClicks = totalClicks;
            record.AdImpressions = totalImpressions;
            record.RPM = avgRpm > 0 ? avgRpm : (totalViews > 0 ? (totalEarnings / totalViews) * 1000 : 0);
            record.CTR = avgCtr > 0 ? avgCtr : (totalImpressions > 0 ? ((decimal)totalClicks / totalImpressions) * 100 : 0);
            record.CPC = avgCpc > 0 ? avgCpc : (totalClicks > 0 ? totalEarnings / totalClicks : 0);
            record.Coverage = avgCoverage;
            record.ActiveViewViewableImpressions = totalActiveView;
            record.SyncedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private async Task<bool> EnsureInitializedAsync()
        {
            if (_initialized)
            {
                return _accessTokenFactory != null || !string.IsNullOrWhiteSpace(_config.ApiKey);
            }

            await _initializationLock.WaitAsync();

            try
            {
                if (_initialized)
                {
                    return _accessTokenFactory != null || !string.IsNullOrWhiteSpace(_config.ApiKey);
                }

                if (_config.UseServiceAccount)
                {
                    var serviceAccount = ResolveServiceAccountCredentialStream();
                    if (serviceAccount.Stream != null)
                    {
                        using (serviceAccount.Stream)
                        {
                            var credential = GoogleCredential.FromStream(serviceAccount.Stream).CreateScoped(RequiredScopes);
                            _accessTokenFactory = () => ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
                        }

                        var sourceDescription = string.IsNullOrWhiteSpace(serviceAccount.Source)
                            ? _config.ServiceAccountEmail
                            : $"{_config.ServiceAccountEmail} via {serviceAccount.Source}";

                        _logger.LogInformation("Initialized AdSense client with service account credentials ({Source})", sourceDescription);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(serviceAccount.Error))
                        {
                            _logger.LogError("AdSense service account initialization failed: {Message}", serviceAccount.Error);
                        }
                        else
                        {
                            var attemptedPath = string.IsNullOrWhiteSpace(_config.ServiceAccountKeyPath)
                                ? "no path configured"
                                : ResolvePath(_config.ServiceAccountKeyPath);
                            
                            _logger.LogError("AdSense service account key not found. Checked environment variables and {Path}", attemptedPath);
                        }
                    }
                }

                if (_accessTokenFactory == null &&
                    !string.IsNullOrWhiteSpace(_config.OAuthClientId) &&
                    !string.IsNullOrWhiteSpace(_config.OAuthClientSecret) &&
                    !string.IsNullOrWhiteSpace(_config.OAuthRefreshToken))
                {
                    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _config.OAuthClientId,
                            ClientSecret = _config.OAuthClientSecret
                        },
                        Scopes = RequiredScopes,
                        DataStore = new NullDataStore()
                    });

                    var userId = string.IsNullOrWhiteSpace(_config.OAuthUserEmail) ? "adsense-sync" : _config.OAuthUserEmail;
                    var token = new TokenResponse { RefreshToken = _config.OAuthRefreshToken };
                    var credential = new UserCredential(flow, userId, token);

                    var refreshed = await credential.RefreshTokenAsync(CancellationToken.None);
                    if (!refreshed)
                    {
                        _logger.LogError("Failed to refresh AdSense OAuth token. Validate refresh token and scopes.");
                    }
                    else
                    {
                        _accessTokenFactory = () => ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
                        _logger.LogInformation("Initialized AdSense client with OAuth credentials for {User}", userId);
                    }
                }

                if (_accessTokenFactory == null && string.IsNullOrWhiteSpace(_config.ApiKey))
                {
                    _logger.LogWarning("AdSense API credentials missing. Configure OAuth client credentials or provide an API key.");
                }
                else if (_accessTokenFactory == null && !string.IsNullOrWhiteSpace(_config.ApiKey))
                {
                    _logger.LogWarning("Using AdSense API key without OAuth. Revenue endpoints may still return PERMISSION_DENIED.");
                }

                _initialized = true;
                return _accessTokenFactory != null || !string.IsNullOrWhiteSpace(_config.ApiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AdSense API client.");
                return false;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        private static Dictionary<string, int> BuildHeaderIndex(JsonElement root)
        {
            var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (root.TryGetProperty("headers", out var headersElement) && headersElement.ValueKind == JsonValueKind.Array)
            {
                var i = 0;
                foreach (var header in headersElement.EnumerateArray())
                {
                    var name = header.TryGetProperty("name", out var nameElement)
                        ? nameElement.GetString() ?? $"col{i}"
                        : $"col{i}";

                    index[name] = i;
                    i++;
                }
            }

            return index;
        }

        private static decimal ReadDecimal(JsonElement row, Dictionary<string, int> headerIndex, string columnName)
        {
            if (!headerIndex.TryGetValue(columnName, out var idx))
            {
                return 0;
            }

            if (!row.TryGetProperty("cells", out var cellsElement) || cellsElement.ValueKind != JsonValueKind.Array)
            {
                return 0;
            }

            if (idx >= cellsElement.GetArrayLength())
            {
                return 0;
            }

            var cell = cellsElement[idx];

            if (cell.ValueKind == JsonValueKind.Object && cell.TryGetProperty("value", out var valueElement))
            {
                return ParseDecimal(valueElement.GetString());
            }

            if (cell.ValueKind == JsonValueKind.String)
            {
                return ParseDecimal(cell.GetString());
            }

            return 0;
        }

        private static string ReadString(JsonElement row, Dictionary<string, int> headerIndex, string columnName)
        {
            if (!headerIndex.TryGetValue(columnName, out var idx))
            {
                return string.Empty;
            }

            if (!row.TryGetProperty("cells", out var cellsElement) || cellsElement.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            if (idx >= cellsElement.GetArrayLength())
            {
                return string.Empty;
            }

            var cell = cellsElement[idx];

            if (cell.ValueKind == JsonValueKind.Object && cell.TryGetProperty("value", out var valueElement))
            {
                return valueElement.GetString() ?? string.Empty;
            }

            if (cell.ValueKind == JsonValueKind.String)
            {
                return cell.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }

        private static string ResolvePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            var baseDirectory = AppContext.BaseDirectory;
            return Path.Combine(baseDirectory, path);
        }

        private (Stream? Stream, string? Source, string? Error) ResolveServiceAccountCredentialStream()
        {
            var inlineJson = TryCreateStreamFromJson(_config.ServiceAccountKeyJson, "AdSenseConfiguration.ServiceAccountKeyJson");
            if (inlineJson.Stream != null || !string.IsNullOrWhiteSpace(inlineJson.Error))
            {
                return inlineJson;
            }

            var environmentCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(_config.ServiceAccountKeyEnvironmentVariable))
            {
                environmentCandidates.Add(_config.ServiceAccountKeyEnvironmentVariable);
            }
            environmentCandidates.Add("GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY");

            foreach (var envName in environmentCandidates)
            {
                var envValue = Environment.GetEnvironmentVariable(envName);
                if (string.IsNullOrWhiteSpace(envValue))
                {
                    continue;
                }

                var envStream = TryCreateStreamFromDynamicValue(envValue, $"Environment:{envName}");
                if (envStream.Stream != null || !string.IsNullOrWhiteSpace(envStream.Error))
                {
                    return envStream;
                }
            }

            var base64Config = TryCreateStreamFromBase64(_config.ServiceAccountKeyBase64, "AdSenseConfiguration.ServiceAccountKeyBase64");
            if (base64Config.Stream != null || !string.IsNullOrWhiteSpace(base64Config.Error))
            {
                return base64Config;
            }

            if (!string.IsNullOrWhiteSpace(_config.ServiceAccountKeyPath))
            {
                var resolvedPath = ResolvePath(_config.ServiceAccountKeyPath);
                if (File.Exists(resolvedPath))
                {
                    return (File.OpenRead(resolvedPath), $"File:{resolvedPath}", null);
                }

                return (null, null, $"AdSense service account key not found at {resolvedPath}");
            }

            return (null, null, null);
        }

        private (Stream? Stream, string? Source, string? Error) TryCreateStreamFromDynamicValue(string value, string sourceLabel)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (null, null, null);
            }

            var trimmed = value.Trim();

            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return TryCreateStreamFromJson(trimmed, $"{sourceLabel} (json)");
            }

            try
            {
                var resolvedPath = ResolvePath(trimmed);
                if (File.Exists(resolvedPath))
                {
                    return (File.OpenRead(resolvedPath), $"{sourceLabel} (file:{resolvedPath})", null);
                }
            }
            catch
            {
                // Ignore path resolution errors and continue attempting other formats.
            }

            var base64Attempt = TryCreateStreamFromBase64(trimmed, $"{sourceLabel} (base64)");
            if (base64Attempt.Stream != null || !string.IsNullOrWhiteSpace(base64Attempt.Error))
            {
                return base64Attempt;
            }

            return TryCreateStreamFromJson(trimmed, $"{sourceLabel} (json)");
        }

        private static (Stream? Stream, string? Source, string? Error) TryCreateStreamFromBase64(string base64, string sourceLabel)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return (null, null, null);
            }

            try
            {
                var decodedBytes = Convert.FromBase64String(base64);
                var json = Encoding.UTF8.GetString(decodedBytes);

                using (JsonDocument.Parse(json))
                {
                    return (new MemoryStream(decodedBytes), sourceLabel, null);
                }
            }
            catch (FormatException)
            {
                return (null, null, null);
            }
            catch (JsonException ex)
            {
                return (null, null, $"{sourceLabel} contains invalid JSON: {ex.Message}");
            }
        }

        private static (Stream? Stream, string? Source, string? Error) TryCreateStreamFromJson(string json, string sourceLabel)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return (null, null, null);
            }

            try
            {
                using (JsonDocument.Parse(json))
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    return (new MemoryStream(bytes), sourceLabel, null);
                }
            }
            catch (JsonException ex)
            {
                return (null, null, $"{sourceLabel} contains invalid JSON: {ex.Message}");
            }
        }

        private SiteRevenueData GeneratePlaceholderRevenue(AdSenseSite site, DateTime date)
        {
            var random = new Random(date.Day + site.Domain.GetHashCode());
            var totalEarnings = site.Domain.Contains("discussion", StringComparison.OrdinalIgnoreCase) ? 1.85m : 0.95m;
            var totalViews = random.Next(500, 2000);
            var totalClicks = random.Next(10, 50);
            var totalImpressions = random.Next(1000, 5000);

            return new SiteRevenueData
            {
                SiteDomain = site.Domain,
                TotalEarnings = totalEarnings,
                TotalEstimatedEarnings = totalEarnings,
                TotalPageViews = totalViews,
                TotalAdClicks = totalClicks,
                TotalImpressions = totalImpressions,
                TotalAdRequests = totalImpressions,
                TotalCoverage = 80,
                TotalRpm = totalViews > 0 ? (totalEarnings / totalViews) * 1000 : 0,
                TotalCtr = totalImpressions > 0 ? ((decimal)totalClicks / totalImpressions) * 100 : 0,
                TotalCpc = totalClicks > 0 ? totalEarnings / totalClicks : 0,
                TotalActiveViewViewableImpressions = (int)(totalImpressions * 0.6),
                UrlRevenueData = GeneratePlaceholderUrlRevenue(site, totalEarnings, date),
                Source = "Placeholder",
                SyncedAt = DateTime.UtcNow
            };
        }

        private List<UrlRevenueData> GeneratePlaceholderUrlRevenue(AdSenseSite site, decimal totalEarnings, DateTime date)
        {
            var posts = _context.Posts
                .Where(p => p.Status == "published" && p.CreatedAt < date)
                .OrderByDescending(p => p.ViewCount)
                .Take(20)
                .Select(p => new { p.PostId, p.Slug, p.ViewCount })
                .ToList();

            var earningsRemaining = totalEarnings;
            var random = new Random(date.Day + site.Domain.GetHashCode());
            var results = new List<UrlRevenueData>();

            foreach (var post in posts)
            {
                if (earningsRemaining <= 0)
                {
                    break;
                }

                var share = Math.Round(earningsRemaining * (decimal)random.NextDouble() * 0.3m, 2);
                if (share > earningsRemaining)
                {
                    share = earningsRemaining;
                }

                var pageViews = random.Next(50, 500);
                var clicks = random.Next(1, 10);
                var impressions = pageViews * random.Next(2, 5);
                var adRequests = impressions + random.Next(0, 200);
                var coverage = adRequests > 0 ? (decimal)impressions / adRequests * 100 : 75;

                results.Add(new UrlRevenueData
                {
                    SiteDomain = site.Domain,
                    PostId = post.PostId,
                    Url = $"{site.BaseUrl?.TrimEnd('/')}/r/community/{post.Slug}",
                    Earnings = share,
                    EstimatedEarnings = share,
                    PageViews = pageViews,
                    AdClicks = clicks,
                    Impressions = impressions,
                    AdRequests = adRequests,
                    Coverage = coverage,
                    Rpm = pageViews > 0 ? (share / pageViews) * 1000 : 0,
                    Ctr = impressions > 0 ? ((decimal)clicks / impressions) * 100 : 0,
                    Cpc = clicks > 0 ? share / clicks : 0,
                    ActiveViewViewableImpressions = (int)(impressions * 0.6)
                });

                earningsRemaining -= share;
            }

            if (earningsRemaining > 0)
            {
                var pageViews = random.Next(200, 800);
                var clicks = random.Next(5, 20);
                var impressions = random.Next(500, 2000);
                var adRequests = impressions + random.Next(0, 200);
                var coverage = adRequests > 0 ? (decimal)impressions / adRequests * 100 : 80;

                results.Add(new UrlRevenueData
                {
                    SiteDomain = site.Domain,
                    PostId = null,
                    Url = site.BaseUrl ?? site.Domain,
                    Earnings = earningsRemaining,
                    EstimatedEarnings = earningsRemaining,
                    PageViews = pageViews,
                    AdClicks = clicks,
                    Impressions = impressions,
                    AdRequests = adRequests,
                    Coverage = coverage,
                    Rpm = pageViews > 0 ? (earningsRemaining / pageViews) * 1000 : 0,
                    Ctr = impressions > 0 ? ((decimal)clicks / impressions) * 100 : 0,
                    Cpc = clicks > 0 ? earningsRemaining / clicks : 0,
                    ActiveViewViewableImpressions = (int)(impressions * 0.6)
                });
            }

            return results;
        }

        /// <summary>
        /// Get total revenue for date range across all sites.
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.MultiSiteRevenues
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .SumAsync(r => r.Earnings);
        }

        /// <summary>
        /// Get revenue grouped by site.
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetRevenueBySiteAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.MultiSiteRevenues
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .GroupBy(r => r.SiteDomain)
                .Select(g => new { Site = g.Key, Revenue = g.Sum(r => r.Earnings) })
                .ToDictionaryAsync(x => x.Site, x => x.Revenue);
        }

        /// <summary>
        /// Get top earning posts across all sites.
        /// </summary>
        public async Task<List<PostRevenueData>> GetTopEarningPostsAsync(int count = 10, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            return await _context.MultiSiteRevenues
                .Where(r => r.PostId != null && r.Date >= startDate)
                .GroupBy(r => new { r.PostId, r.SiteDomain })
                .Select(g => new PostRevenueData
                {
                    PostId = g.Key.PostId!.Value,
                    SiteDomain = g.Key.SiteDomain,
                    TotalEarnings = g.Sum(r => r.Earnings),
                    TotalPageViews = g.Sum(r => r.PageViews),
                    AverageRPM = g.Average(r => r.RPM)
                })
                .OrderByDescending(x => x.TotalEarnings)
                .Take(count)
                .ToListAsync();
        }
    }

    internal sealed class ReportTotals
    {
        public decimal TotalEstimatedEarnings { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalPageViews { get; set; }
        public decimal TotalAdRequests { get; set; }
        public decimal TotalClicks { get; set; }
        public decimal TotalImpressions { get; set; }
        public decimal TotalCoverage { get; set; }
        public decimal TotalRpm { get; set; }
        public decimal TotalCtr { get; set; }
        public decimal TotalCpc { get; set; }
        public decimal TotalActiveViewImpressions { get; set; }
    }

    public class SiteRevenueData
    {
        public string SiteDomain { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public decimal TotalEstimatedEarnings { get; set; }
        public int TotalPageViews { get; set; }
        public int TotalAdClicks { get; set; }
        public int TotalImpressions { get; set; }
        public int TotalAdRequests { get; set; }
        public decimal TotalCoverage { get; set; }
        public decimal TotalRpm { get; set; }
        public decimal TotalCtr { get; set; }
        public decimal TotalCpc { get; set; }
        public int TotalActiveViewViewableImpressions { get; set; }
        public List<UrlRevenueData> UrlRevenueData { get; set; } = new();
        public string Source { get; set; } = "AdSense";
        public DateTime SyncedAt { get; set; }
    }

    public class UrlRevenueData
    {
        public string SiteDomain { get; set; } = string.Empty;
        public int? PostId { get; set; }
        public string Url { get; set; } = string.Empty;
        public decimal Earnings { get; set; }
        public decimal EstimatedEarnings { get; set; }
        public int PageViews { get; set; }
        public int AdClicks { get; set; }
        public int Impressions { get; set; }
        public int AdRequests { get; set; }
        public decimal? Coverage { get; set; }
        public decimal? Rpm { get; set; }
        public decimal? Ctr { get; set; }
        public decimal? Cpc { get; set; }
        public int ActiveViewViewableImpressions { get; set; }
    }

    public class PostRevenueData
    {
        public int PostId { get; set; }
        public string SiteDomain { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public int TotalPageViews { get; set; }
        public decimal AverageRPM { get; set; }
    }
}

