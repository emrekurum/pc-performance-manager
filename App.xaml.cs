using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PcPerformanceManager.Services;

namespace PcPerformanceManager;

public partial class App : System.Windows.Application
{
    public static ILocalizationService LocalizationService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize localization service
        LocalizationService = new LocalizationService();
        
        // Load language from settings
        _ = LoadLanguageFromSettingsAsync();

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

    private async Task LoadLanguageFromSettingsAsync()
    {
        try
        {
            var settingsService = new SettingsService();
            var settings = await settingsService.LoadSettingsAsync();
            LocalizationService.ChangeLanguage(settings.Language);
        }
        catch (Exception ex)
        {
            LogMessage($"Language load error: {ex.Message}");
            // Varsayılan olarak Türkçe kullan
            LocalizationService.ChangeLanguage("tr-TR");
        }
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
