using System;
using System.IO;
using System.Windows;

namespace PcPerformanceManager;

public partial class App : System.Windows.Application
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
            System.Windows.MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (sender, args) =>
        {
            var error = $"Dispatcher Exception: {args.Exception}";
            LogMessage(error);
            System.Windows.MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        // Manually create and show MainWindow
        try
        {
            LogMessage("Creating MainWindow...");
            var mainWindow = new MainWindow();
            LogMessage("MainWindow created successfully.");
            this.MainWindow = mainWindow;
            LogMessage("Setting MainWindow property...");
            mainWindow.Show();
            LogMessage("MainWindow.Show() called.");
            mainWindow.Activate();
            LogMessage("MainWindow.Activate() called.");
            LogMessage("MainWindow created and shown successfully.");
        }
        catch (Exception ex)
        {
            var error = $"Failed to create MainWindow: {ex.Message}\n\n{ex.StackTrace}";
            LogMessage(error);
            try
            {
                System.Windows.MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // MessageBox da başarısız olabilir
            }
        }

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
