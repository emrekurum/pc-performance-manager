using System;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.ViewModels;

namespace PcPerformanceManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            // LoadSettings'ı async olarak çağır, window'un gösterilmesini engelleme
            _ = LoadSettingsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Window initialization error: {ex.Message}\n\n{ex.StackTrace}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private async System.Threading.Tasks.Task LoadSettingsAsync()
    {
        try
        {
            var settingsService = new Services.SettingsService();
            var settings = await settingsService.LoadSettingsAsync();
            ApplySettings(settings);
        }
        catch (Exception ex)
        {
            // Settings yüklenemezse varsayılan değerler kullanılır
            System.Diagnostics.Debug.WriteLine($"Settings load error: {ex.Message}");
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        // Tema değiştirme (şimdilik sadece dark tema destekleniyor)
        // Gelecekte light tema desteği eklenebilir
        // Not: Minimize to tray özelliği şimdilik devre dışı (UseWindowsForms gerektiriyor)
    }
}
