using Serilog;
using System.Windows;
using TaskManager.View;
using TaskManager.ViewModel;

namespace TaskManager
{
    public partial class App : Application
    {
        private readonly MainViewModel mainViewModel = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File("logs/application.log", rollingInterval: RollingInterval.Day).CreateLogger();

            new MainWindow() { DataContext = mainViewModel }.Show();
            Log.Information("Приложение запущено");
        }
    }
}
