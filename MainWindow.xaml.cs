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
        // Tema değiştirme kontrolü
        if (settings.Theme == "Light")
        {
            // Light tema henüz desteklenmiyor - kullanıcıya bilgi ver
            System.Windows.MessageBox.Show(
                "Light tema özelliği henüz kullanılamıyor. Sadece Dark tema desteklenmektedir.",
                "Tema Değişikliği",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        
        // Dil değiştirme kontrolü
        if (settings.Language != "tr-TR")
        {
            // Localization sistemi henüz kurulmadı - kullanıcıya bilgi ver
            System.Windows.MessageBox.Show(
                $"Dil değişikliği özelliği henüz kullanılamıyor. Şu anda sadece Türkçe desteklenmektedir.\n\n" +
                $"Seçilen dil: {settings.Language}\n" +
                $"Dil değişikliği özelliği gelecek güncellemede eklenecektir.",
                "Dil Değişikliği",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        
        // Not: Minimize to tray özelliği şimdilik devre dışı (UseWindowsForms gerektiriyor)
    }
}
