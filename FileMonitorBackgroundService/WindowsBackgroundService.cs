using FileMonitorBackgroundService.Udp;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileMonitorBackgroundService
{
    public sealed class WindowsBackgroundService(
        FileMonitorBackgroundService fileMonitorBackgroundService,
        ILogger<WindowsBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var eventLog = new System.Diagnostics.EventLog();
                eventLog.BeginInit();
                eventLog.Source = "FileMonitorBackgroundService";
                eventLog.EndInit();

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        var message = $"FileMonitorBackgroundService running at: {DateTimeOffset.Now}";
                        logger.LogInformation(message);
                        eventLog.WriteEntry(message);
                    }

                    fileMonitorBackgroundService.Run();
                    var _changedFileCount = fileMonitorBackgroundService.ChangedFileCount;

                    if (_changedFileCount > 0 && logger.IsEnabled(LogLevel.Information))
                    {
                        var host = "127.0.0.1";
                        var portNumber = 9713;
                        sendUdpMessage(new FilesChangedUdpMessage(_changedFileCount));

                        var logMessage = $"FileMonitorBackgroundService, changed files found: {_changedFileCount}, UDP packet sent to: {host}:{portNumber}, running at: {DateTimeOffset.Now}";
                        logger.LogInformation(logMessage);
                        eventLog.WriteEntry(logMessage);
                    }

                    // Force a five minute timer before the service restarts
                    await Task.Delay(300000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }
        
        private void sendUdpMessage(IUdpMessage message, string host = "127.0.0.1", int portNumber = 9713)
        {
            var ip = IPAddress.Parse(host);
            var udpClient = new UdpClient();

            udpClient.Connect(ip, portNumber);

            var messageFormatted = message
                .ChangedFileCount
                .ToString()
                .ToCharArray();

            var bytes = Encoding
                .Default
                .GetBytes(messageFormatted);

            udpClient.Send(bytes);
        }
    }
}
