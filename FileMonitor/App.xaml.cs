using DataAccessLayer;
using System.Configuration;
using System.IO;
using System.Windows;

namespace FileMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Override <see cref="App.OnStartup(StartupEventArgs)"/> to ensure the database file is created. Do so using <c>DbContext.EnsureCreated()</c>.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!File.Exists(ConfigurationManager.AppSettings["DatabasePath"]))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["DatabaseDir"]);
                using FileMonitorDbContext _db = new FileMonitorDbContext(
                    ConfigurationManager.ConnectionStrings[nameof(FileMonitorDbContext)].ConnectionString);
                _db.Database.EnsureCreated();
            }
            base.OnStartup(e);
        }
    }
}
