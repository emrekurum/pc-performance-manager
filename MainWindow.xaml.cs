using System;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;
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
        // Tema değiştirme kontrolü
        if (settings.Theme == "Light")
        {
            // Light tema henüz desteklenmiyor - kullanıcıya bilgi ver
            var themeMsg = App.LocalizationService.GetString("ThemeNotSupported", 
                "Light theme feature is not yet available. Only Dark theme is supported.");
            var themeTitle = App.LocalizationService.GetString("ThemeChange", "Theme Change");
            System.Windows.MessageBox.Show(
                themeMsg,
                themeTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        
        // Dil değiştirme - artık çalışıyor!
        if (App.LocalizationService.CurrentLanguage != settings.Language)
        {
            App.LocalizationService.ChangeLanguage(settings.Language);
            
            // Navigation items'ı güncelle
            if (DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.RefreshNavigationItems();
            }
            
            // Window title'ı güncelle
            Title = App.LocalizationService.GetString("AppTitle", "PC Performance Manager");
        }
        
        // Not: Minimize to tray özelliği şimdilik devre dışı (UseWindowsForms gerektiriyor)
    }
}
