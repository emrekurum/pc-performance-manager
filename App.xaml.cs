using System;
using System.IO;
using System.Windows;

namespace PcPerformanceManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Log startup
        LogMessage("Application starting...");

        // Global exception handling
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var error = $"Unhandled Exception: {args.ExceptionObject}";
            LogMessage(error);
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (sender, args) =>
        {
            var error = $"Dispatcher Exception: {args.Exception}";
            LogMessage(error);
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        LogMessage("Application startup complete.");
    }

    private void LogMessage(string message)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
        }
        catch { }
    }
}
