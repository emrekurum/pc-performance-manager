using System;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.ViewModels;

namespace PcPerformanceManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            var settingsService = new Services.SettingsService();
            var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
            ApplySettings(settings);
        }
        catch
        {
            // Settings yüklenemezse varsayılan değerler kullanılır
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        // Tema değiştirme (şimdilik sadece dark tema destekleniyor)
        // Gelecekte light tema desteği eklenebilir
        // Not: Minimize to tray özelliği şimdilik devre dışı (UseWindowsForms gerektiriyor)
    }
}
