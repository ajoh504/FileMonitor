using System.Diagnostics;
using System.ServiceProcess;
using System.Diagnostics;
using System.Timers;
using System.Runtime.InteropServices;

namespace Services
{
    partial class StartupService : ServiceBase
    {
        private EventLog _eventLog;
        private int _eventId = 1;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(nint handle, ref ServiceStatus serviceStatus);

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            var serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            _eventLog.WriteEntry("Beginning FileMonitor StartupService: Start Pending");

            // Set up a timer that triggers every minute.
            var timer = new System.Timers.Timer
            {
                Interval = 60000 // 60 seconds
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            _eventLog.WriteEntry("FileMonitor StartupService: Service Running");
        }

        protected override void OnStop()
        {
            _eventLog.WriteEntry("Stopping FileMonitor StartupService");
        }

        public void OnTimer(object sender, ElapsedEventArgs e)
        {
            _eventLog.WriteEntry("FileMonitor StartupService: status normal", EventLogEntryType.Information, _eventId++);
        }

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        public StartupService()
        {
            InitializeComponent();

            _eventLog = new EventLog();

            if (!EventLog.SourceExists("StartupServiceSource"))
            {
                EventLog.CreateEventSource("StartupServiceSource", "StartupServiceLog");
            }

            _eventLog.Source = "StartupServiceSource";
            _eventLog.Log = "StartupServiceLog";
        }
    }
}
