using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services.MCP
{
    /// <summary>
    /// Automated MCP Server Manager - Auto-starts and monitors MCP servers
    /// </summary>
    public interface IMcpServerManager
    {
        Task<bool> StartServerAsync(string serverName, string scriptPath, int port, int maxRetries = 3);
        Task<bool> StopServerAsync(string serverName);
        Task<bool> IsServerRunningAsync(int port);
        Task RestartServerAsync(string serverName, string scriptPath, int port);
    }

    public class McpServerManager : IMcpServerManager, IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<McpServerManager> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;
        private readonly Dictionary<string, Process> _runningServers = new();
        private readonly Dictionary<string, int> _retryCounts = new();
        private Timer? _healthCheckTimer;
        private bool _isEnabled;

        public McpServerManager(
            IConfiguration configuration,
            ILogger<McpServerManager> logger,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
            _httpClient = httpClientFactory.CreateClient("McpServerManager");
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            _isEnabled = _configuration.GetValue<bool>("MCP:AutoStart:Enabled", true);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_isEnabled)
            {
                _logger.LogInformation("MCP Server Auto-Start is disabled");
                return;
            }

            _logger.LogInformation("🚀 Starting MCP Server Manager...");

            // Start health check timer (every 30 seconds)
            _healthCheckTimer = new Timer(async _ => await CheckAndRestartServersAsync(), null, 
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));

            // Auto-start configured servers
            await AutoStartServersAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _healthCheckTimer?.Dispose();
            
            _logger.LogInformation("Stopping MCP Server Manager...");
            
            foreach (var server in _runningServers.Values.ToList())
            {
                try
                {
                    if (!server.HasExited)
                    {
                        server.Kill();
                        server.WaitForExit(5000);
                    }
                    server.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error stopping server process");
                }
            }
            
            _runningServers.Clear();
            return Task.CompletedTask;
        }

        private async Task AutoStartServersAsync()
        {
            var servers = new[]
            {
                new { Name = "SeoAutomation", Script = "seo-automation/main.py", Port = 5001 },
                // Add more servers here when implemented
                // new { Name = "Performance", Script = "performance/main.py", Port = 5002 },
                // new { Name = "UserPreferences", Script = "user-preferences/main.py", Port = 5003 }
            };

            foreach (var server in servers)
            {
                var enabled = _configuration.GetValue<bool>($"MCP:Servers:{server.Name}:Enabled", true);
                if (!enabled)
                {
                    _logger.LogInformation("Server {ServerName} is disabled, skipping", server.Name);
                    continue;
                }

                // Try multiple possible paths (check both with and without discussionspot9 folder)
                var possiblePaths = new List<string>
                {
                    Path.Combine(_environment.ContentRootPath, "mcp-servers", server.Script),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", server.Script),
                    Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", server.Script)
                };

                // Also try parent directory (in case we're in bin/Debug/net9.0)
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                if (baseDir.Contains("bin") || baseDir.Contains("Debug") || baseDir.Contains("Release"))
                {
                    var projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;
                    if (!string.IsNullOrEmpty(projectRoot))
                    {
                        possiblePaths.Add(Path.Combine(projectRoot, "mcp-servers", server.Script));
                    }
                }

                // Try ContentRootPath parent (if ContentRootPath is discussionspot9 folder)
                var contentRootParent = Directory.GetParent(_environment.ContentRootPath)?.FullName;
                if (!string.IsNullOrEmpty(contentRootParent))
                {
                    possiblePaths.Add(Path.Combine(contentRootParent, "mcp-servers", server.Script));
                    possiblePaths.Add(Path.Combine(contentRootParent, "discussionspot9", "mcp-servers", server.Script));
                }

                string? scriptPath = null;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        scriptPath = path;
                        _logger.LogInformation("Found server script at: {Path}", path);
                        break;
                    }
                }

                if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
                {
                    _logger.LogError("Server script not found. Searched paths: {Paths}", 
                        string.Join(", ", possiblePaths));
                    continue;
                }

                _logger.LogInformation("Auto-starting {ServerName} on port {Port}...", server.Name, server.Port);
                await StartServerAsync(server.Name, scriptPath, server.Port);
                
                // Wait a bit before starting next server
                await Task.Delay(2000);
            }
        }

        public async Task<bool> StartServerAsync(string serverName, string scriptPath, int port, int maxRetries = 3)
        {
            // Check if already running
            if (await IsServerRunningAsync(port))
            {
                _logger.LogInformation("Server {ServerName} is already running on port {Port}", serverName, port);
                return true;
            }

            // Check retry count
            if (_retryCounts.ContainsKey(serverName) && _retryCounts[serverName] >= maxRetries)
            {
                _logger.LogError("Server {ServerName} has exceeded max retries ({MaxRetries}), giving up", serverName, maxRetries);
                return false;
            }

            try
            {
                _logger.LogInformation("Starting {ServerName}... (Attempt {Attempt})", 
                    serverName, 
                    _retryCounts.GetValueOrDefault(serverName, 0) + 1);

                // Check if Python is available
                var pythonExe = _configuration["Python:ExecutablePath"] ?? "python";
                var pythonCheck = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                try
                {
                    using var checkProcess = Process.Start(pythonCheck);
                    if (checkProcess == null)
                    {
                        throw new Exception("Python not found. Please install Python and add it to PATH.");
                    }
                    checkProcess.WaitForExit(2000);
                    var pythonVersion = await checkProcess.StandardOutput.ReadToEndAsync();
                    _logger.LogInformation("Python found: {Version}", pythonVersion.Trim());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Python check failed. Make sure Python is installed and in PATH.");
                    throw;
                }

                var scriptDir = Path.GetDirectoryName(scriptPath);
                var scriptFile = Path.GetFileName(scriptPath);

                _logger.LogInformation("Starting server from: {ScriptDir}, File: {ScriptFile}", scriptDir, scriptFile);

                // Check if requirements are installed
                var requirementsPath = Path.Combine(scriptDir ?? "", "requirements.txt");
                if (File.Exists(requirementsPath))
                {
                    _logger.LogInformation("Requirements file found, checking dependencies...");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{scriptFile}\"",
                    WorkingDirectory = scriptDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        _logger.LogDebug("[{ServerName}] {Output}", serverName, e.Data);
                };

                var errorOutput = new List<string>();
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        errorOutput.Add(e.Data);
                        _logger.LogWarning("[{ServerName}] {Error}", serverName, e.Data);
                    }
                };

                var standardOutput = new List<string>();
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        standardOutput.Add(e.Data);
                        _logger.LogInformation("[{ServerName}] {Output}", serverName, e.Data);
                    }
                };

                if (!process.Start())
                {
                    throw new Exception("Failed to start process");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait longer for server to start and check multiple times
                var maxWaitTime = 10000; // 10 seconds
                var checkInterval = 1000; // Check every second
                var waited = 0;
                var isRunning = false;

                while (waited < maxWaitTime)
                {
                    await Task.Delay(checkInterval);
                    waited += checkInterval;

                    isRunning = await IsServerRunningAsync(port);
                    if (isRunning)
                    {
                        break;
                    }

                    // Check if process has exited (crashed)
                    if (process.HasExited)
                    {
                        var exitCode = process.ExitCode;
                        var allErrors = string.Join("\n", errorOutput);
                        var allOutput = string.Join("\n", standardOutput);
                        throw new Exception($"Server process exited with code {exitCode}. Errors: {allErrors}. Output: {allOutput}");
                    }
                }

                // Final check
                isRunning = await IsServerRunningAsync(port);

                if (isRunning)
                {
                    _runningServers[serverName] = process;
                    _retryCounts[serverName] = 0; // Reset retry count on success
                    _logger.LogInformation("✅ {ServerName} started successfully on port {Port}", serverName, port);
                    return true;
                }
                else
                {
                    var allErrors = string.Join("\n", errorOutput);
                    var allOutput = string.Join("\n", standardOutput);
                    process.Kill();
                    process.Dispose();
                    throw new Exception($"Server started but health check failed after {waited}ms. Errors: {allErrors}. Output: {allOutput}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to start {ServerName}, will retry", serverName);
                
                _retryCounts[serverName] = _retryCounts.GetValueOrDefault(serverName, 0) + 1;
                
                // Retry after delay
                var retryDelay = _configuration.GetValue<int>("MCP:AutoStart:RetryDelaySeconds", 10);
                await Task.Delay(retryDelay * 1000);
                
                return await StartServerAsync(serverName, scriptPath, port, maxRetries);
            }
        }

        public async Task<bool> StopServerAsync(string serverName)
        {
            if (!_runningServers.ContainsKey(serverName))
            {
                return false;
            }

            try
            {
                var process = _runningServers[serverName];
                if (!process.HasExited)
                {
                    process.Kill();
                    await Task.Run(() => process.WaitForExit(5000));
                }
                process.Dispose();
                _runningServers.Remove(serverName);
                _logger.LogInformation("Stopped {ServerName}", serverName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping {ServerName}", serverName);
                return false;
            }
        }

        public async Task<bool> IsServerRunningAsync(int port)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:{port}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task RestartServerAsync(string serverName, string scriptPath, int port)
        {
            _logger.LogInformation("Restarting {ServerName}...", serverName);
            await StopServerAsync(serverName);
            await Task.Delay(2000);
            await StartServerAsync(serverName, scriptPath, port);
        }

        private async Task CheckAndRestartServersAsync()
        {
            var servers = new[]
            {
                new { Name = "SeoAutomation", Port = 5001, Script = "seo-automation/main.py" }
            };

            foreach (var server in servers)
            {
                var enabled = _configuration.GetValue<bool>($"MCP:Servers:{server.Name}:Enabled", true);
                if (!enabled) continue;

                var isRunning = await IsServerRunningAsync(server.Port);
                
                if (!isRunning && _runningServers.ContainsKey(server.Name))
                {
                    _logger.LogWarning("Server {ServerName} appears to have crashed, restarting...", server.Name);
                    
                    var scriptPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "mcp-servers",
                        server.Script);

                    if (File.Exists(scriptPath))
                    {
                        await RestartServerAsync(server.Name, scriptPath, server.Port);
                    }
                }
                else if (!isRunning && !_runningServers.ContainsKey(server.Name))
                {
                    // Server not running and not in our list, try to start it
                    // Try multiple possible paths
                    var possiblePaths = new[]
                    {
                        Path.Combine(_environment.ContentRootPath, "mcp-servers", server.Script),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", server.Script),
                        Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", server.Script)
                    };

                    string? scriptPath = null;
                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            scriptPath = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
                    {
                        _logger.LogInformation("Server {ServerName} is not running, attempting to start...", server.Name);
                        await StartServerAsync(server.Name, scriptPath, server.Port);
                    }
                    else
                    {
                        _logger.LogWarning("Server {ServerName} script not found, cannot restart", server.Name);
                    }
                }
            }
        }
    }
}

