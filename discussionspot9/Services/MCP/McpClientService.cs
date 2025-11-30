using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace discussionspot9.Services.MCP
{
    /// <summary>
    /// Client service for communicating with MCP servers
    /// </summary>
    public interface IMcpClientService
    {
        Task<T?> CallSeoServerAsync<T>(string method, object? parameters = null) where T : class;
        Task<T?> CallPerformanceServerAsync<T>(string method, object? parameters = null) where T : class;
        Task<T?> CallPreferencesServerAsync<T>(string method, object? parameters = null) where T : class;
    }

    public class McpClientService : IMcpClientService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<McpClientService> _logger;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public McpClientService(
            IConfiguration configuration,
            ILogger<McpClientService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("McpClient");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T?> CallSeoServerAsync<T>(string method, object? parameters = null) where T : class
        {
            var endpoint = _configuration["MCP:Servers:SeoAutomation:Endpoint"];
            if (string.IsNullOrWhiteSpace(endpoint) || 
                !_configuration.GetValue<bool>("MCP:Servers:SeoAutomation:Enabled", false))
            {
                _logger.LogWarning("SEO Automation MCP server is disabled or not configured");
                return default(T);
            }

            return await CallMcpServerAsync<T>(endpoint, method, parameters);
        }

        public async Task<T?> CallPerformanceServerAsync<T>(string method, object? parameters = null) where T : class
        {
            var endpoint = _configuration["MCP:Servers:Performance:Endpoint"];
            if (string.IsNullOrWhiteSpace(endpoint) || 
                !_configuration.GetValue<bool>("MCP:Servers:Performance:Enabled", false))
            {
                _logger.LogWarning("Performance MCP server is disabled or not configured");
                return default(T);
            }

            return await CallMcpServerAsync<T>(endpoint, method, parameters);
        }

        public async Task<T?> CallPreferencesServerAsync<T>(string method, object? parameters = null) where T : class
        {
            var endpoint = _configuration["MCP:Servers:UserPreferences:Endpoint"];
            if (string.IsNullOrWhiteSpace(endpoint) || 
                !_configuration.GetValue<bool>("MCP:Servers:UserPreferences:Enabled", false))
            {
                _logger.LogWarning("User Preferences MCP server is disabled or not configured");
                return default(T);
            }

            return await CallMcpServerAsync<T>(endpoint, method, parameters);
        }

        private async Task<T?> CallMcpServerAsync<T>(string endpoint, string method, object? parameters) where T : class
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = method,
                    @params = parameters ?? new { },
                    id = Guid.NewGuid().ToString()
                };

                var response = await _httpClient.PostAsJsonAsync($"{endpoint}/mcp", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<McpResponse<T>>(_jsonOptions);
                return result?.Result ?? default(T);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling MCP server at {Endpoint} with method {Method}", endpoint, method);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling MCP server at {Endpoint}", endpoint);
                return default(T);
            }
        }
    }

    internal class McpResponse<T>
    {
        public string? JsonRpc { get; set; }
        public T? Result { get; set; }
        public McpError? Error { get; set; }
        public string? Id { get; set; }
    }

    internal class McpError
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}

