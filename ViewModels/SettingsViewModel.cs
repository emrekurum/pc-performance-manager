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
    private string statusMessage;

    [ObservableProperty]
    private bool hasUnsavedChanges;

    public SettingsViewModel()
    {
        _settingsService = new SettingsService();
        _windowsStartupService = new WindowsStartupService();
        StatusMessage = App.LocalizationService.GetString("Ready", "Ready");
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
            StatusMessage = App.LocalizationService.GetString("SettingsLoaded", "Settings loaded");
        }
        catch (Exception ex)
        {
            var errorMsg = App.LocalizationService.GetString("SettingsLoadError", "An error occurred while loading settings:");
            StatusMessage = $"{errorMsg} {ex.Message}";
            var errorTitle = App.LocalizationService.GetString("Error", "Error");
            MessageBox.Show($"{errorMsg}\n{ex.Message}",
                errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (IsSaving) return;

        IsSaving = true;
        StatusMessage = App.LocalizationService.GetString("SavingSettings", "Saving settings...");

        try
        {
            // Windows ile başlatma ayarını uygula
            if (Settings.StartWithWindows)
            {
                if (!_windowsStartupService.EnableStartup())
                {
                    StatusMessage = App.LocalizationService.GetString("StartupFailed", "Start with Windows setting could not be applied");
                }
            }
            else
            {
                if (!_windowsStartupService.DisableStartup())
                {
                    StatusMessage = App.LocalizationService.GetString("StartupDisableFailed", "Start with Windows could not be disabled");
                }
            }

            var success = await _settingsService.SaveSettingsAsync(Settings);

            if (success)
            {
                HasUnsavedChanges = false;
                StatusMessage = App.LocalizationService.GetString("SettingsSaved", "Settings saved successfully");
                
                // Ayarları uygulamaya bildir
                ApplySettings();
                
                var successMsg = App.LocalizationService.GetString("SettingsSaveSuccess", "Settings saved successfully!");
                var successTitle = App.LocalizationService.GetString("Success", "Success");
                MessageBox.Show(successMsg,
                    successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = App.LocalizationService.GetString("SettingsSaveFailed", "Settings could not be saved");
                var errorMsg = App.LocalizationService.GetString("SettingsSaveError", "Settings could not be saved. Please try again.");
                var errorTitle = App.LocalizationService.GetString("Error", "Error");
                MessageBox.Show(errorMsg,
                    errorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            var errorMsg = App.LocalizationService.GetString("SettingsSaveErrorTitle", "An error occurred while saving settings:");
            StatusMessage = $"{errorMsg} {ex.Message}";
            MessageBox.Show($"{errorMsg}\n{ex.Message}",
                App.LocalizationService.GetString("Error", "Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
        var confirmMsg = App.LocalizationService.GetString("ResetConfirmation", "All settings will be reset to default values. Do you want to continue?");
        var confirmTitle = App.LocalizationService.GetString("ResetTitle", "Reset Settings");
        var result = MessageBox.Show(
            confirmMsg,
            confirmTitle,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return Task.CompletedTask;

        try
        {
            Settings = _settingsService.GetDefaultSettings();
            HasUnsavedChanges = true;
            StatusMessage = App.LocalizationService.GetString("SettingsReset", "Settings reset to default values");
        }
        catch (Exception ex)
        {
            var errorMsg = App.LocalizationService.GetString("Error", "Error");
            StatusMessage = $"{errorMsg}: {ex.Message}";
            MessageBox.Show($"Settings reset error:\n{ex.Message}",
                errorMsg, MessageBoxButton.OK, MessageBoxImage.Error);
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
