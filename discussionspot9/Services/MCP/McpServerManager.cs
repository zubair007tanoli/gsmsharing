using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
        Task<bool> StartServerAsync(string serverName, string scriptPath, int preferredPort, int maxRetries = 3);
        Task<bool> StopServerAsync(string serverName);
        Task<bool> IsServerRunningAsync(int port);
        Task RestartServerAsync(string serverName, string scriptPath, int preferredPort);
        int? GetServerPort(string serverName);
        Dictionary<string, int> GetAllServerPorts();
    }

    public class McpServerManager : IMcpServerManager, IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<McpServerManager> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;
        private readonly IPortFinder _portFinder;
        private readonly Dictionary<string, Process> _runningServers = new();
        private readonly Dictionary<string, int> _serverPorts = new(); // Store actual ports used
        private readonly Dictionary<string, int> _retryCounts = new();
        private Timer? _healthCheckTimer;
        private bool _isEnabled;

        public McpServerManager(
            IConfiguration configuration,
            ILogger<McpServerManager> logger,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment,
            IPortFinder portFinder)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
            _portFinder = portFinder;
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
                // Try test_server.py first (no dependencies), then main_simple.py, then main.py
                new { 
                    Name = "SeoAutomation", 
                    Scripts = new[] { "seo-automation/test_server.py", "seo-automation/main_simple.py", "seo-automation/main.py" }, 
                    PreferredPort = _configuration.GetValue<int>("MCP:Servers:SeoAutomation:Port", 5001)
                },
                // Add more servers here when implemented
                // new { Name = "Performance", Script = "performance/main.py", PreferredPort = 5002 },
                // new { Name = "UserPreferences", Script = "user-preferences/main.py", PreferredPort = 5003 }
            };

            foreach (var server in servers)
            {
                var enabled = _configuration.GetValue<bool>($"MCP:Servers:{server.Name}:Enabled", true);
                if (!enabled)
                {
                    _logger.LogInformation("Server {ServerName} is disabled, skipping", server.Name);
                    continue;
                }

                // Try each script in order (test_server.py first, then main_simple.py, then main.py)
                string? scriptPath = null;
                foreach (var script in server.Scripts)
                {
                    // Try multiple possible paths (check both with and without discussionspot9 folder, including Ubuntu paths)
                    var possiblePaths = new List<string>
                    {
                        Path.Combine(_environment.ContentRootPath, "mcp-servers", script),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", script),
                        Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", script),
                        // Ubuntu deployment paths
                        "/var/www/discussionspot/mcp-servers/" + script,
                        "/var/www/discussionspot9/mcp-servers/" + script
                    };

                    // Also try parent directory (in case we're in bin/Debug/net9.0)
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (baseDir.Contains("bin") || baseDir.Contains("Debug") || baseDir.Contains("Release"))
                    {
                        var projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;
                        if (!string.IsNullOrEmpty(projectRoot))
                        {
                            possiblePaths.Add(Path.Combine(projectRoot, "mcp-servers", script));
                        }
                    }

                    // Try ContentRootPath parent (if ContentRootPath is discussionspot9 folder)
                    var contentRootParent = Directory.GetParent(_environment.ContentRootPath)?.FullName;
                    if (!string.IsNullOrEmpty(contentRootParent))
                    {
                        possiblePaths.Add(Path.Combine(contentRootParent, "mcp-servers", script));
                        possiblePaths.Add(Path.Combine(contentRootParent, "discussionspot9", "mcp-servers", script));
                    }

                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            scriptPath = path;
                            _logger.LogInformation("Found server script at: {Path}", path);
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(scriptPath))
                        break;
                }

                if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
                {
                    _logger.LogError("Server script not found. Tried scripts: {Scripts}", 
                        string.Join(", ", server.Scripts));
                    continue;
                }

                _logger.LogInformation("Auto-starting {ServerName} on preferred port {Port} using {Script}...", 
                    server.Name, server.PreferredPort, Path.GetFileName(scriptPath));
                await StartServerAsync(server.Name, scriptPath, server.PreferredPort);
                
                // Wait a bit before starting next server
                await Task.Delay(2000);
            }
        }

        public async Task<bool> StartServerAsync(string serverName, string scriptPath, int preferredPort, int maxRetries = 3)
        {
            // Check if server is already running (check stored port)
            if (_serverPorts.ContainsKey(serverName))
            {
                int existingPort = _serverPorts[serverName];
                if (await IsServerRunningAsync(existingPort))
                {
                    _logger.LogInformation("Server {ServerName} is already running on port {Port}", serverName, existingPort);
                    return true;
                }
                else
                {
                    // Port stored but server not running, clear it
                    _serverPorts.Remove(serverName);
                    _runningServers.Remove(serverName);
                }
            }

            // Find available port (preferred port or alternative)
            int port;
            try
            {
                port = _portFinder.FindAvailablePort(preferredPort, maxAttempts: 10);
                _logger.LogInformation("Using port {Port} for {ServerName} (preferred was {PreferredPort})", 
                    port, serverName, preferredPort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find available port for {ServerName}", serverName);
                return false;
            }

            // Store the port we're using
            _serverPorts[serverName] = port;

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

                // Check if Python is available (try python3 first on Linux, then python)
                var pythonExe = _configuration["Python:ExecutablePath"];
                if (string.IsNullOrEmpty(pythonExe))
                {
                    // Auto-detect: try python3 first (Linux/Ubuntu), then python (Windows)
                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    pythonExe = isWindows ? "python" : "python3";
                }

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
                        // Try alternative: if python3 failed, try python (or vice versa)
                        var altPython = pythonExe == "python3" ? "python" : "python3";
                        pythonCheck.FileName = altPython;
                        using var altCheckProcess = Process.Start(pythonCheck);
                        if (altCheckProcess == null)
                        {
                            throw new Exception($"Python not found. Tried: {pythonExe} and {altPython}. Please install Python and add it to PATH.");
                        }
                        pythonExe = altPython;
                        altCheckProcess.WaitForExit(2000);
                        var pythonVersion = await altCheckProcess.StandardOutput.ReadToEndAsync();
                        _logger.LogInformation("Python found: {Version} (using {Exe})", pythonVersion.Trim(), pythonExe);
                    }
                    else
                    {
                        checkProcess.WaitForExit(2000);
                        var pythonVersion = await checkProcess.StandardOutput.ReadToEndAsync();
                        _logger.LogInformation("Python found: {Version} (using {Exe})", pythonVersion.Trim(), pythonExe);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Python check failed. Make sure Python is installed and in PATH.");
                    throw;
                }

                var scriptDir = Path.GetDirectoryName(scriptPath);
                var scriptFile = Path.GetFileName(scriptPath);

                _logger.LogInformation("Starting server from: {ScriptDir}, File: {ScriptFile}, Port: {Port}", scriptDir, scriptFile, port);

                // Check if requirements are installed
                var requirementsPath = Path.Combine(scriptDir ?? "", "requirements.txt");
                if (File.Exists(requirementsPath))
                {
                    _logger.LogInformation("Requirements file found, checking dependencies...");
                }

                // For test_server.py, pass port as argument if it's not the default
                var scriptArgs = $"\"{scriptPath}\"";
                if (scriptPath.EndsWith("test_server.py") && port != 5001)
                {
                    scriptArgs = $"\"{scriptPath}\" {port}";
                    _logger.LogInformation("Passing port {Port} as argument to test_server.py", port);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = scriptArgs,
                    WorkingDirectory = scriptDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                // Set environment variable for unbuffered output (helps with logging)
                startInfo.Environment["PYTHONUNBUFFERED"] = "1";

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

                // Final check - wait a bit longer for server to fully start
                await Task.Delay(1000); // Give server extra time to initialize
                isRunning = await IsServerRunningAsync(port);

                if (isRunning)
                {
                    _runningServers[serverName] = process;
                    _serverPorts[serverName] = port; // Store the port
                    _retryCounts[serverName] = 0; // Reset retry count on success
                    _logger.LogInformation("✅ {ServerName} started successfully on port {Port}", serverName, port);
                    return true;
                }
                else
                {
                    var allErrors = string.Join("\n", errorOutput);
                    var allOutput = string.Join("\n", standardOutput);
                    
                    // Don't kill process immediately - it might still be starting
                    // Wait a bit more and check again
                    await Task.Delay(2000);
                    isRunning = await IsServerRunningAsync(port);
                    
                    if (isRunning)
                    {
                        _runningServers[serverName] = process;
                        _serverPorts[serverName] = port;
                        _retryCounts[serverName] = 0;
                        _logger.LogInformation("✅ {ServerName} started successfully on port {Port} (after extended wait)", serverName, port);
                        return true;
                    }
                    
                    // Check one more time after a longer delay
                    await Task.Delay(3000);
                    isRunning = await IsServerRunningAsync(port);
                    
                    if (isRunning)
                    {
                        _runningServers[serverName] = process;
                        _serverPorts[serverName] = port;
                        _retryCounts[serverName] = 0;
                        _logger.LogInformation("✅ {ServerName} started successfully on port {Port} (after final wait)", serverName, port);
                        return true;
                    }
                    
                    // Process is still running but health check fails - might be a different issue
                    _logger.LogWarning("Server process is running but health check failed. Process may be starting slowly or there's a configuration issue.");
                    _logger.LogWarning("Errors: {Errors}, Output: {Output}", allErrors, allOutput);
                    
                    // Don't kill the process - it might still be starting
                    // Store it and let health check monitor it
                    _runningServers[serverName] = process;
                    _serverPorts[serverName] = port;
                    
                    // Return false but keep process running - health check will retry
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start {ServerName}: {Error}", serverName, ex.Message);
                
                // Clear port if we stored it
                if (_serverPorts.ContainsKey(serverName))
                {
                    _serverPorts.Remove(serverName);
                }
                
                _retryCounts[serverName] = _retryCounts.GetValueOrDefault(serverName, 0) + 1;
                
                // Only retry if we haven't exceeded max retries
                if (_retryCounts[serverName] < maxRetries)
                {
                    _logger.LogInformation("Retrying {ServerName} in {Delay} seconds (attempt {Attempt}/{MaxRetries})", 
                        serverName, _configuration.GetValue<int>("MCP:AutoStart:RetryDelaySeconds", 10), 
                        _retryCounts[serverName], maxRetries);
                    
                    // Retry after delay
                    var retryDelay = _configuration.GetValue<int>("MCP:AutoStart:RetryDelaySeconds", 10);
                    await Task.Delay(retryDelay * 1000);
                    
                    return await StartServerAsync(serverName, scriptPath, preferredPort, maxRetries);
                }
                else
                {
                    _logger.LogError("Server {ServerName} has exceeded max retries ({MaxRetries}), giving up", serverName, maxRetries);
                    return false;
                }
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
                _serverPorts.Remove(serverName); // Clear port mapping
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

        public async Task RestartServerAsync(string serverName, string scriptPath, int preferredPort)
        {
            _logger.LogInformation("Restarting {ServerName}...", serverName);
            await StopServerAsync(serverName);
            await Task.Delay(2000);
            await StartServerAsync(serverName, scriptPath, preferredPort);
        }

        public int? GetServerPort(string serverName)
        {
            return _serverPorts.ContainsKey(serverName) ? _serverPorts[serverName] : null;
        }

        public Dictionary<string, int> GetAllServerPorts()
        {
            return new Dictionary<string, int>(_serverPorts);
        }

        private async Task CheckAndRestartServersAsync()
        {
            var servers = new[]
            {
                new { 
                    Name = "SeoAutomation", 
                    PreferredPort = _configuration.GetValue<int>("MCP:Servers:SeoAutomation:Port", 5001), 
                    Scripts = new[] { "seo-automation/test_server.py", "seo-automation/main_simple.py", "seo-automation/main.py" } 
                }
            };

            foreach (var server in servers)
            {
                var enabled = _configuration.GetValue<bool>($"MCP:Servers:{server.Name}:Enabled", true);
                if (!enabled) continue;

                // Check if server is running (use stored port if available, otherwise preferred port)
                int portToCheck = _serverPorts.ContainsKey(server.Name) 
                    ? _serverPorts[server.Name] 
                    : server.PreferredPort;
                var isRunning = await IsServerRunningAsync(portToCheck);
                
                if (!isRunning && _runningServers.ContainsKey(server.Name))
                {
                    _logger.LogWarning("Server {ServerName} appears to have crashed, restarting...", server.Name);
                    
                    // Try to find script path
                    string? scriptPath = null;
                    foreach (var script in server.Scripts)
                    {
                        var possiblePaths = new[]
                        {
                            Path.Combine(_environment.ContentRootPath, "mcp-servers", script),
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", script),
                            Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", script)
                        };

                        foreach (var path in possiblePaths)
                        {
                            if (File.Exists(path))
                            {
                                scriptPath = path;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(scriptPath))
                            break;
                    }

                    if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
                    {
                        await RestartServerAsync(server.Name, scriptPath, server.PreferredPort);
                    }
                }
                else if (!isRunning && !_runningServers.ContainsKey(server.Name))
                {
                    // Server not running and not in our list, try to start it
                    // Try multiple possible paths for each script
                    string? scriptPath = null;
                    foreach (var script in server.Scripts)
                    {
                        var possiblePaths = new[]
                        {
                            Path.Combine(_environment.ContentRootPath, "mcp-servers", script),
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", script),
                            Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", script)
                        };

                        foreach (var path in possiblePaths)
                        {
                            if (File.Exists(path))
                            {
                                scriptPath = path;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(scriptPath))
                            break;
                    }

                    if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
                    {
                        _logger.LogInformation("Server {ServerName} is not running, attempting to start...", server.Name);
                        await StartServerAsync(server.Name, scriptPath, server.PreferredPort);
                    }
                    else
                    {
                        _logger.LogWarning("Server {ServerName} script not found, cannot restart. Tried: {Scripts}", 
                            server.Name, string.Join(", ", server.Scripts));
                    }
                }
            }
        }
    }
}

