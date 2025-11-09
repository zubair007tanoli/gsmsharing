using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace discussionspot9.Infrastructure
{
    /// <summary>
    /// Resolves Google OAuth credentials from multiple configuration sources (files, environment variables, direct config).
    /// </summary>
    public static class GoogleCredentialResolver
    {
        public static GoogleOAuthResolutionResult ResolveOAuthCredentials(
            IConfiguration configuration,
            string contentRootPath)
        {
            var result = new GoogleOAuthResolutionResult();

            var defaultSecretsPath = Path.Combine(contentRootPath, "Secrets", "AuthKeys.json");
            var wwwrootSecretsPath = Path.Combine(contentRootPath, "wwwroot", "GoogleApiAccess", "AuthKeys.json");

            var configuredPath = configuration["Authentication:Google:CredentialsPath"];
            var envConfiguredPath = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CREDENTIALS_PATH");

            var candidatePaths = new List<(string Path, string Source)>(
                new[]
                {
                    (Path: configuredPath ?? string.Empty, Source: "Authentication:Google:CredentialsPath configuration"),
                    (Path: envConfiguredPath ?? string.Empty, Source: "GOOGLE_OAUTH_CREDENTIALS_PATH environment variable"),
                    (Path: defaultSecretsPath, Source: "Default Secrets/AuthKeys.json"),
                    (Path: wwwrootSecretsPath, Source: "wwwroot/GoogleApiAccess/AuthKeys.json fallback")
                }
                .Where(item => !string.IsNullOrWhiteSpace(item.Path))
                .Select(item => NormalizePath(item.Path, contentRootPath, item.Source))
            );

            foreach (var (path, source) in candidatePaths)
            {
                result.AttemptedPaths.Add(path);

                if (!File.Exists(path))
                {
                    continue;
                }

                if (TryParseCredentialsFromJson(File.ReadAllText(path), out var clientId, out var clientSecret))
                {
                    result.Success = true;
                    result.ClientId = clientId;
                    result.ClientSecret = clientSecret;
                    result.Source = source;
                    return result;
                }

                result.Warnings.Add($"Failed to parse Google OAuth credentials from {source} ({path}).");
            }

            var jsonCandidates = new[]
            {
                (configuration["Authentication:Google:CredentialsJson"], "Authentication:Google:CredentialsJson configuration"),
                (Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CREDENTIALS_JSON"), "GOOGLE_OAUTH_CREDENTIALS_JSON environment variable"),
                (DecodeBase64(Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CREDENTIALS_BASE64")), "GOOGLE_OAUTH_CREDENTIALS_BASE64 environment variable")
            };

            foreach (var (json, source) in jsonCandidates)
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    continue;
                }

                if (TryParseCredentialsFromJson(json, out var clientId, out var clientSecret))
                {
                    result.Success = true;
                    result.ClientId = clientId;
                    result.ClientSecret = clientSecret;
                    result.Source = source;
                    return result;
                }

                result.Warnings.Add($"Failed to parse Google OAuth credentials from {source} JSON content.");
            }

            var clientIdCandidate = configuration["Authentication:Google:ClientId"]
                ?? Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
            var clientSecretCandidate = configuration["Authentication:Google:ClientSecret"]
                ?? Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET");

            if (!string.IsNullOrWhiteSpace(clientIdCandidate) && !string.IsNullOrWhiteSpace(clientSecretCandidate))
            {
                result.Success = true;
                result.ClientId = clientIdCandidate;
                result.ClientSecret = clientSecretCandidate;
                result.Source = "Configuration/environment variables";
                return result;
            }

            result.MissingPathHint = configuredPath
                ?? envConfiguredPath
                ?? (candidatePaths.FirstOrDefault().Path ?? defaultSecretsPath);

            if (result.AttemptedPaths.Count > 0 && string.IsNullOrWhiteSpace(result.MissingPathHint))
            {
                result.MissingPathHint = result.AttemptedPaths[0];
            }

            return result;
        }

        private static (string Path, string Source) NormalizePath(string path, string contentRootPath, string source)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(contentRootPath, path));
            }

            return (path, source);
        }

        private static bool TryParseCredentialsFromJson(string json, out string? clientId, out string? clientSecret)
        {
            clientId = null;
            clientSecret = null;

            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("web", out var webElement))
                {
                    clientId = ReadString(webElement, "client_id") ?? ReadString(webElement, "clientId");
                    clientSecret = ReadString(webElement, "client_secret") ?? ReadString(webElement, "clientSecret");
                }
                else if (root.TryGetProperty("installed", out var installedElement))
                {
                    clientId = ReadString(installedElement, "client_id") ?? ReadString(installedElement, "clientId");
                    clientSecret = ReadString(installedElement, "client_secret") ?? ReadString(installedElement, "clientSecret");
                }
                else
                {
                    clientId = ReadString(root, "client_id") ?? ReadString(root, "clientId");
                    clientSecret = ReadString(root, "client_secret") ?? ReadString(root, "clientSecret");
                }

                return !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string? ReadString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
        }

        private static string? DecodeBase64(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }

    public sealed class GoogleOAuthResolutionResult
    {
        public bool Success { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Source { get; set; }
        public string? MissingPathHint { get; set; }
        public List<string> AttemptedPaths { get; } = new();
        public List<string> Warnings { get; } = new();
    }
}

