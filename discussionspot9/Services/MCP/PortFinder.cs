using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services.MCP
{
    /// <summary>
    /// Utility class to find available ports for MCP servers
    /// </summary>
    public interface IPortFinder
    {
        int FindAvailablePort(int preferredPort, int maxAttempts = 10);
        bool IsPortAvailable(int port);
        List<int> FindAvailablePorts(int count, int startPort = 5001);
    }

    public class PortFinder : IPortFinder
    {
        private readonly ILogger<PortFinder> _logger;

        public PortFinder(ILogger<PortFinder> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Finds an available port, starting from preferred port
        /// </summary>
        public int FindAvailablePort(int preferredPort, int maxAttempts = 10)
        {
            // First, check if preferred port is available
            if (IsPortAvailable(preferredPort))
            {
                _logger.LogInformation("Preferred port {Port} is available", preferredPort);
                return preferredPort;
            }

            _logger.LogWarning("Preferred port {Port} is not available, searching for alternative", preferredPort);

            // Try ports in range [preferredPort, preferredPort + maxAttempts]
            for (int i = 1; i <= maxAttempts; i++)
            {
                int candidatePort = preferredPort + i;
                
                // Don't go above 65535 (max port number)
                if (candidatePort > 65535)
                {
                    candidatePort = preferredPort - i;
                    if (candidatePort < 1024)
                    {
                        _logger.LogError("Could not find available port after {Attempts} attempts", maxAttempts);
                        throw new Exception($"Could not find available port after {maxAttempts} attempts");
                    }
                }

                if (IsPortAvailable(candidatePort))
                {
                    _logger.LogInformation("Found available port: {Port} (preferred was {PreferredPort})", 
                        candidatePort, preferredPort);
                    return candidatePort;
                }
            }

            _logger.LogError("Could not find available port after {Attempts} attempts", maxAttempts);
            throw new Exception($"Could not find available port after {maxAttempts} attempts");
        }

        /// <summary>
        /// Checks if a port is available
        /// </summary>
        public bool IsPortAvailable(int port)
        {
            if (port < 1024 || port > 65535)
            {
                return false;
            }

            try
            {
                // Check if port is in use
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking port {Port} availability", port);
                return false;
            }
        }

        /// <summary>
        /// Finds multiple available ports
        /// </summary>
        public List<int> FindAvailablePorts(int count, int startPort = 5001)
        {
            var availablePorts = new List<int>();
            int currentPort = startPort;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    int port = FindAvailablePort(currentPort, maxAttempts: 20);
                    availablePorts.Add(port);
                    currentPort = port + 1; // Start next search from next port
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to find {Count} available ports, found {Found} so far", 
                        count, availablePorts.Count);
                    throw;
                }
            }

            return availablePorts;
        }

        /// <summary>
        /// Gets information about a port (what's using it)
        /// </summary>
        public string GetPortInfo(int port)
        {
            try
            {
                var connections = IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpConnections()
                    .Where(c => c.LocalEndPoint.Port == port)
                    .ToList();

                if (connections.Any())
                {
                    var connection = connections.First();
                    return $"Port {port} is in use by PID {connection.State}";
                }

                var listeners = IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Where(l => l.Port == port)
                    .ToList();

                if (listeners.Any())
                {
                    return $"Port {port} is listening";
                }

                return $"Port {port} appears to be available";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting port info for {Port}", port);
                return $"Unable to determine port {port} status";
            }
        }
    }
}

