using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;

namespace PcPerformanceManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IWindowsStartupService _windowsStartupService;

    [ObservableProperty]
    private AppSettings settings = new();

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private string statusMessage = "Hazır";

    [ObservableProperty]
    private bool hasUnsavedChanges;

    public SettingsViewModel()
    {
        _settingsService = new SettingsService();
        _windowsStartupService = new WindowsStartupService();
        _ = LoadSettingsAsync(); // Fire and forget
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            Settings = await _settingsService.LoadSettingsAsync();
            
            // Windows ile başlatma durumunu kontrol et ve senkronize et
            Settings.StartWithWindows = _windowsStartupService.IsStartupEnabled();
            
            HasUnsavedChanges = false;
            StatusMessage = "Ayarlar yüklendi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Ayarlar yüklenirken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (IsSaving) return;

        IsSaving = true;
        StatusMessage = "Ayarlar kaydediliyor...";

        try
        {
            // Windows ile başlatma ayarını uygula
            if (Settings.StartWithWindows)
            {
                if (!_windowsStartupService.EnableStartup())
                {
                    StatusMessage = "Windows ile başlatma ayarı uygulanamadı";
                }
            }
            else
            {
                if (!_windowsStartupService.DisableStartup())
                {
                    StatusMessage = "Windows ile başlatma kapatılamadı";
                }
            }

            var success = await _settingsService.SaveSettingsAsync(Settings);

            if (success)
            {
                HasUnsavedChanges = false;
                StatusMessage = "Ayarlar başarıyla kaydedildi";
                
                // Ayarları uygulamaya bildir
                ApplySettings();
                
                MessageBox.Show("Ayarlar başarıyla kaydedildi!",
                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Ayarlar kaydedilemedi";
                MessageBox.Show("Ayarlar kaydedilemedi. Lütfen tekrar deneyin.",
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Ayarlar kaydedilirken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ApplySettings()
    {
        // Ayarların uygulanması için App ve MainWindow'a sinyal gönder
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ApplySettings(Settings);
        }
        
        // Dashboard kartlarını güncelle
        if (Application.Current.MainWindow?.DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.UpdateDashboardVisibility(Settings);
        }
    }

    [RelayCommand]
    private Task ResetToDefaultsAsync()
    {
        var result = MessageBox.Show(
            "Tüm ayarlar varsayılan değerlere sıfırlanacak. Devam etmek istiyor musunuz?",
            "Ayarları Sıfırla",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return Task.CompletedTask;

        try
        {
            Settings = _settingsService.GetDefaultSettings();
            HasUnsavedChanges = true;
            StatusMessage = "Ayarlar varsayılan değerlere sıfırlandı";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Ayarlar sıfırlanırken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        return Task.CompletedTask;
    }

    // Ayarlar değiştiğinde HasUnsavedChanges flag'ini güncelle
    partial void OnSettingsChanged(AppSettings value)
    {
        if (value != null)
        {
            HasUnsavedChanges = true;
        }
    }
}
