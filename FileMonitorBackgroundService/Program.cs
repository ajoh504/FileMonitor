using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

namespace FileMonitorBackgroundService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "FileMonitorBackgroundService";
            });

            LoggerProviderOptions.RegisterProviderOptions<
                EventLogSettings, EventLogLoggerProvider>(builder.Services);

            builder.Services.AddSingleton<FileMonitorBackgroundService>();
            builder.Services.AddHostedService<WindowsBackgroundService>();

            var host = builder.Build();
            host.Run();
        }
    }
}