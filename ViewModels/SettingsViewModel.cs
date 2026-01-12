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
        _ = LoadSettingsAsync(); // Fire and forget
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            Settings = await _settingsService.LoadSettingsAsync();
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
            var success = await _settingsService.SaveSettingsAsync(Settings);

            if (success)
            {
                HasUnsavedChanges = false;
                StatusMessage = "Ayarlar başarıyla kaydedildi";
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
