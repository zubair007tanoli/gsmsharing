using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using discussionspot9.Models.Configuration;
using discussionspot9.Models.GoogleSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace discussionspot9.Services.HealthChecks
{
    /// <summary>
    /// Validates that required Google integrations (OAuth, Search APIs, AdSense) have been configured.
    /// </summary>
    public class GoogleIntegrationsHealthCheck : IHealthCheck
    {
        private readonly GoogleOAuthRuntimeState _oauthState;
        private readonly IOptions<GoogleSearchConfig> _googleSearchOptions;
        private readonly IOptions<AdSenseConfiguration> _adSenseOptions;
        private readonly IConfiguration _configuration;

        public GoogleIntegrationsHealthCheck(
            GoogleOAuthRuntimeState oauthState,
            IOptions<GoogleSearchConfig> googleSearchOptions,
            IOptions<AdSenseConfiguration> adSenseOptions,
            IConfiguration configuration)
        {
            _oauthState = oauthState;
            _googleSearchOptions = googleSearchOptions;
            _adSenseOptions = adSenseOptions;
            _configuration = configuration;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var failures = new List<string>();

            if (string.IsNullOrWhiteSpace(_oauthState.ClientId) || string.IsNullOrWhiteSpace(_oauthState.ClientSecret))
            {
                var hint = string.IsNullOrWhiteSpace(_oauthState.CredentialsPathHint)
                    ? "Set GOOGLE_OAUTH_CLIENT_ID and GOOGLE_OAUTH_CLIENT_SECRET environment variables."
                    : $"Provide credentials at {_oauthState.CredentialsPathHint} or via GOOGLE_OAUTH_CLIENT_ID / GOOGLE_OAUTH_CLIENT_SECRET environment variables.";

                failures.Add($"Google OAuth credentials missing. {hint}");
            }

            var searchConfig = _googleSearchOptions.Value;

            if (searchConfig.EnableSerpApiFallback && string.IsNullOrWhiteSpace(searchConfig.SerpApiKey))
            {
                failures.Add("SerpApi fallback is enabled but no SerpApi key is configured.");
            }

            if (searchConfig.EnableRapidApi && string.IsNullOrWhiteSpace(searchConfig.ApiKey))
            {
                failures.Add("RapidAPI Google Search is enabled but no API key is configured.");
            }

            var adSenseConfig = _adSenseOptions.Value;

            if (adSenseConfig.UseServiceAccount)
            {
                var credentialsAvailable =
                    !string.IsNullOrWhiteSpace(adSenseConfig.ServiceAccountKeyJson) ||
                    !string.IsNullOrWhiteSpace(adSenseConfig.ServiceAccountKeyBase64) ||
                    !string.IsNullOrWhiteSpace(adSenseConfig.ServiceAccountKeyPath) ||
                    HasEnvironmentValue(adSenseConfig.ServiceAccountKeyEnvironmentVariable) ||
                    HasEnvironmentValue("GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY");

                if (!credentialsAvailable)
                {
                    failures.Add("AdSense service account is enabled but credentials are not configured.");
                }
            }

            return failures.Count == 0
                ? Task.FromResult(HealthCheckResult.Healthy("Google integrations are configured."))
                : Task.FromResult(HealthCheckResult.Unhealthy(string.Join("; ", failures)));
        }

        private bool HasEnvironmentValue(string? variableName)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                return false;
            }

            var value = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(_configuration[variableName]);
        }
    }
}










