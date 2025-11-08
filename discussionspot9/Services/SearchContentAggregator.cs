using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using discussionspot9.Models.GoogleSearch;
using discussionspot9.Models.Seo;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace discussionspot9.Services
{
    public class SearchContentAggregator
    {
        private readonly GoogleSearchService _googleSearchService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoogleSearchConfig _config;
        private readonly ILogger<SearchContentAggregator> _logger;
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "this","that","these","those","what","which","who","when","where","why","how","with","from","have","been",
            "will","your","their","about","into","through","during","before","after","above","below","between","under",
            "again","further","then","once","here","there","all","both","each","few","more","most","other","some","such",
            "only","own","same","than","too","very","just","don't","can't","should","now","also","like","them","they",
            "over","while","well","make","much","many","even","back","know","time","year","years","take","used"
        };

        public SearchContentAggregator(
            GoogleSearchService googleSearchService,
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleSearchConfig> config,
            ILogger<SearchContentAggregator> logger)
        {
            _googleSearchService = googleSearchService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<SearchContentAggregationResult?> AggregateAsync(
            string title,
            string? content,
            IEnumerable<string>? seedKeywords = null,
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var competitorConfig = _config.CompetitorContent ?? new CompetitorContentConfig();

                var primaryKeyword = DeterminePrimaryKeyword(title, content ?? string.Empty, seedKeywords);
                if (string.IsNullOrWhiteSpace(primaryKeyword))
                {
                    primaryKeyword = title;
                }

                var resultLimit = limit ?? competitorConfig.MaxResults;
                var searchResponse = await _googleSearchService.SearchAsync(primaryKeyword, resultLimit, includeRelatedKeywords: true);
                if (searchResponse == null)
                {
                    return null;
                }

                var analysisContext = new GoogleSearchAnalysisContext
                {
                    SearchTerm = searchResponse.SearchTerm,
                    RelatedKeywords = searchResponse.RelatedKeywords?.Keywords
                        .Select(k => k.Keyword)
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(20)
                        .ToList() ?? new List<string>(),
                    RetrievedAtUtc = DateTime.UtcNow
                };

                var competitors = new List<CompetitorContentInsight>();
                var competitorCandidates = searchResponse.Results
                    .Where(r => !string.IsNullOrWhiteSpace(r.Url))
                    .Take(resultLimit)
                    .ToList();

                if (competitorCandidates.Count > 0)
                {
                    if (competitorConfig.Enabled)
                    {
                        var tasks = competitorCandidates.Select(r =>
                            FetchCompetitorContentAsync(r, competitorConfig, cancellationToken)).ToArray();
                        var results = await Task.WhenAll(tasks);
                        competitors.AddRange(results.Where(r => r != null)!.Select(r => r!));
                    }

                    // Add placeholders for any competitor without fetched content
                    foreach (var candidate in competitorCandidates)
                    {
                        if (competitors.Any(c => string.Equals(c.Url, candidate.Url, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        competitors.Add(new CompetitorContentInsight
                        {
                            Position = candidate.Position,
                            Title = candidate.Title,
                            Url = candidate.Url,
                            Domain = ExtractDomain(candidate.Url),
                            Description = candidate.Description,
                            ContentSnippet = candidate.Description ?? string.Empty,
                            WordCount = 0,
                            EstimatedDomainAuthority = EstimateAuthority(candidate.Url, candidate.Position, 0).domain,
                            EstimatedUrlAuthority = EstimateAuthority(candidate.Url, candidate.Position, 0).url,
                            KeyPhrases = new List<string>()
                        });
                    }
                }

                analysisContext.Competitors = competitors
                    .OrderBy(c => c.Position)
                    .ThenByDescending(c => c.EstimatedDomainAuthority)
                    .ToList();

                var topicInsights = await _googleSearchService.GetTopicInsightsAsync(primaryKeyword);

                return new SearchContentAggregationResult
                {
                    PrimaryKeyword = primaryKeyword,
                    SearchResponse = searchResponse,
                    TopicInsights = topicInsights,
                    AnalysisContext = analysisContext
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to aggregate search content context for title: {Title}", title);
                return null;
            }
        }

        private async Task<CompetitorContentInsight?> FetchCompetitorContentAsync(
            SearchResult result,
            CompetitorContentConfig competitorConfig,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!Uri.TryCreate(result.Url, UriKind.Absolute, out var uri))
                {
                    return null;
                }

                var client = _httpClientFactory.CreateClient("CompetitorContent");
                client.Timeout = TimeSpan.FromSeconds(Math.Max(competitorConfig.RequestTimeoutSeconds, 5));
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(Windows NT 10.0; Win64; x64)", ""));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch competitor content {Status} for {Url}", response.StatusCode, uri);
                    return null;
                }

                var html = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(html))
                {
                    return null;
                }

                var effectiveMaxLength = competitorConfig.IncludeFullContent
                    ? Math.Max(competitorConfig.MaxContentLength, 6000)
                    : competitorConfig.MaxContentLength;

                var mainContent = ExtractReadableContent(html, effectiveMaxLength);
                if (string.IsNullOrWhiteSpace(mainContent))
                {
                    mainContent = result.Description ?? string.Empty;
                }

                var wordCount = CountWords(mainContent);
                var snippetLength = Math.Min(effectiveMaxLength, 1500);
                var contentSnippet = CreateSnippet(mainContent, snippetLength);
                var keyPhrases = ExtractKeyPhrases(mainContent);
                var (domainAuthority, urlAuthority) = EstimateAuthority(uri.Host, result.Position, wordCount);

                return new CompetitorContentInsight
                {
                    Position = result.Position,
                    Title = result.Title,
                    Url = uri.ToString(),
                    Domain = ExtractDomain(uri.Host),
                    Description = result.Description ?? string.Empty,
                    ContentSnippet = contentSnippet,
                    WordCount = wordCount,
                    EstimatedDomainAuthority = domainAuthority,
                    EstimatedUrlAuthority = urlAuthority,
                    KeyPhrases = keyPhrases
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching competitor content for {Url}", result.Url);
                return null;
            }
        }

        private static string ExtractReadableContent(string html, int maxLength)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var nodesToRemove = document.DocumentNode
                .SelectNodes("//script|//style|//noscript|//header|//footer|//svg|//form");
            if (nodesToRemove != null)
            {
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }

            var articleNode = document.DocumentNode.SelectSingleNode("//article") ??
                              document.DocumentNode.SelectSingleNode("//main");
            var paragraphNodes = articleNode?.SelectNodes(".//p") ??
                                 document.DocumentNode.SelectNodes("//p");

            var paragraphs = paragraphNodes?
                .Select(p => HtmlEntity.DeEntitize(p.InnerText).Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList() ?? new List<string>();

            if (paragraphs.Count == 0)
            {
                var text = HtmlEntity.DeEntitize(document.DocumentNode.InnerText)
                    .Replace("\r", " ")
                    .Replace("\n", " ");
                text = Regex.Replace(text, @"\s+", " ").Trim();
                return text.Length > maxLength ? text[..maxLength] : text;
            }

            var combined = string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
            if (combined.Length > maxLength)
            {
                combined = combined[..maxLength];
            }

            return combined;
        }

        private static string CreateSnippet(string content, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var normalized = Regex.Replace(content, @"\s+", " ").Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized[..maxLength] + "...";
        }

        private static int CountWords(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return 0;
            }

            return Regex.Matches(content, @"\b[\w']+\b").Count;
        }

        private static string ExtractDomain(string urlOrHost)
        {
            try
            {
                var host = urlOrHost;
                if (Uri.TryCreate(urlOrHost, UriKind.Absolute, out var uri))
                {
                    host = uri.Host;
                }
                return host.Replace("www.", "", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return urlOrHost;
            }
        }

        private static (double domain, double url) EstimateAuthority(string host, int position, int wordCount)
        {
            var baseScore = 60.0;
            baseScore += Math.Min(wordCount / 400.0, 12.0);
            baseScore -= Math.Max(position - 1, 0) * 5.0;

            if (host.EndsWith(".gov", StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith(".edu", StringComparison.OrdinalIgnoreCase))
            {
                baseScore += 10;
            }

            var domainAuthority = Math.Clamp(baseScore, 25, 95);
            var urlAuthority = Math.Clamp(domainAuthority - 4 + (host.Length < 20 ? 2 : 0), 20, 92);
            return (domainAuthority, urlAuthority);
        }

        private static string DeterminePrimaryKeyword(
            string title,
            string content,
            IEnumerable<string>? seedKeywords)
        {
            if (seedKeywords != null)
            {
                var candidate = seedKeywords.FirstOrDefault(k => !string.IsNullOrWhiteSpace(k));
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate;
                }
            }

            var combined = $"{title} {content}".ToLowerInvariant();
            var matches = Regex.Matches(combined, @"\b[a-z]{4,}\b");

            var frequency = matches
                .Select(m => m.Value)
                .Where(word => !StopWords.Contains(word))
                .GroupBy(word => word)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Word.Length)
                .ToList();

            return frequency.FirstOrDefault()?.Word ?? title;
        }

        private static List<string> ExtractKeyPhrases(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new List<string>();
            }

            var words = Regex.Matches(content.ToLowerInvariant(), @"\b[a-z]{4,}\b")
                .Select(m => m.Value)
                .Where(w => !StopWords.Contains(w))
                .ToList();

            var frequency = words
                .GroupBy(w => w)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Word)
                .Take(12)
                .Select(g => g.Word)
                .ToList();

            return frequency;
        }
    }
}

