using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;

namespace PcPerformanceManager.ViewModels;

public partial class StartupViewModel : ObservableObject
{
    private readonly IStartupService _startupService;

    [ObservableProperty]
    private ObservableCollection<StartupProgram> startupPrograms = new();

    [ObservableProperty]
    private StartupProgram? selectedProgram;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private string statusMessage = "Hazır";

    [ObservableProperty]
    private double estimatedStartupTime;

    [ObservableProperty]
    private int enabledCount;

    [ObservableProperty]
    private int disabledCount;

    [ObservableProperty]
    private int highImpactCount;

    [ObservableProperty]
    private int mediumImpactCount;

    [ObservableProperty]
    private int lowImpactCount;

    public StartupViewModel()
    {
        _startupService = new StartupService();
        _ = LoadStartupProgramsAsync();
    }

    [RelayCommand]
    private async Task LoadStartupProgramsAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Başlangıç programları yükleniyor...";

        try
        {
            var programs = await _startupService.GetStartupProgramsAsync();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                StartupPrograms.Clear();
                foreach (var program in programs)
                {
                    StartupPrograms.Add(program);
                }
                
                UpdateStatistics();
                UpdateEstimatedStartupTime();
            });

            StatusMessage = $"{programs.Count} başlangıç programı bulundu";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Başlangıç programları yüklenirken hata oluştu:\n{ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task ToggleStartupAsync(StartupProgram? program)
    {
        if (program == null || IsProcessing) return;

        IsProcessing = true;
        var previousState = program.IsEnabled;
        var action = program.IsEnabled ? "devre dışı bırakılıyor" : "etkinleştiriliyor";

        StatusMessage = $"{program.DisplayName} {action}...";

        try
        {
            bool success;
            if (program.IsEnabled)
            {
                success = await _startupService.DisableStartupAsync(program);
            }
            else
            {
                success = await _startupService.EnableStartupAsync(program);
            }

            if (success)
            {
                program.IsEnabled = !previousState;
                UpdateStatistics();
                UpdateEstimatedStartupTime();
                StatusMessage = $"{program.DisplayName} {(program.IsEnabled ? "etkinleştirildi" : "devre dışı bırakıldı")}";
            }
            else
            {
                StatusMessage = $"{program.DisplayName} {(program.IsEnabled ? "etkinleştirilemedi" : "devre dışı bırakılamadı")}";
                MessageBox.Show(
                    $"{program.DisplayName} {(program.IsEnabled ? "etkinleştirilemedi" : "devre dışı bırakılamadı")}.\n" +
                    "Yönetici izinlerini kontrol edin ve tekrar deneyin.",
                    "İşlem Başarısız",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"İşlem sırasında hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task DisableSelectedAsync()
    {
        var selectedPrograms = StartupPrograms.Where(p => p.IsSelected && p.IsEnabled).ToList();
        if (!selectedPrograms.Any())
        {
            MessageBox.Show("Devre dışı bırakılacak program seçilmedi.", "Uyarı", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"{selectedPrograms.Count} program devre dışı bırakılacak. Devam etmek istiyor musunuz?",
            "Onay",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsProcessing = true;
        StatusMessage = "Seçili programlar devre dışı bırakılıyor...";

        try
        {
            int successCount = 0;
            int failCount = 0;
            int criticalCount = 0;

            foreach (var program in selectedPrograms)
            {
                try
                {
                    if (await _startupService.DisableStartupAsync(program))
                    {
                        program.IsEnabled = false;
                        successCount++;
                    }
                    else
                    {
                        // Kritik uygulama kontrolü - eğer Description'da "Kritik" varsa
                        if (program.Description?.Contains("Kritik", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            criticalCount++;
                        }
                        failCount++;
                    }
                }
                catch
                {
                    failCount++;
                }
            }

            UpdateStatistics();
            UpdateEstimatedStartupTime();
            
            if (criticalCount > 0)
            {
                StatusMessage = $"{successCount} program devre dışı bırakıldı. {criticalCount} kritik uygulama korundu" + 
                               (failCount > criticalCount ? $", {failCount - criticalCount} başarısız" : "");
            }
            else
            {
                StatusMessage = $"{successCount} program devre dışı bırakıldı" + 
                               (failCount > 0 ? $", {failCount} başarısız" : "");
            }

            if (failCount > 0)
            {
                MessageBox.Show(
                    $"{successCount} program başarıyla devre dışı bırakıldı.\n{failCount} program işlenemedi.",
                    "Kısmi Başarı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Toplu işlem sırasında hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task EnableSelectedAsync()
    {
        var selectedPrograms = StartupPrograms.Where(p => p.IsSelected && !p.IsEnabled).ToList();
        if (!selectedPrograms.Any())
        {
            MessageBox.Show("Etkinleştirilecek program seçilmedi.", "Uyarı", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"{selectedPrograms.Count} program etkinleştirilecek. Devam etmek istiyor musunuz?",
            "Onay",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsProcessing = true;
        StatusMessage = "Seçili programlar etkinleştiriliyor...";

        try
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var program in selectedPrograms)
            {
                try
                {
                    if (await _startupService.EnableStartupAsync(program))
                    {
                        program.IsEnabled = true;
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }
                catch
                {
                    failCount++;
                }
            }

            UpdateStatistics();
            UpdateEstimatedStartupTime();
            StatusMessage = $"{successCount} program etkinleştirildi" + 
                           (failCount > 0 ? $", {failCount} başarısız" : "");

            if (failCount > 0)
            {
                MessageBox.Show(
                    $"{successCount} program başarıyla etkinleştirildi.\n{failCount} program işlenemedi.",
                    "Kısmi Başarı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Toplu işlem sırasında hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var program in StartupPrograms)
        {
            program.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var program in StartupPrograms)
        {
            program.IsSelected = false;
        }
    }

    [RelayCommand]
    private void SelectHighImpact()
    {
        foreach (var program in StartupPrograms.Where(p => p.Impact == StartupImpact.High))
        {
            program.IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectDisabled()
    {
        foreach (var program in StartupPrograms.Where(p => !p.IsEnabled))
        {
            program.IsSelected = true;
        }
    }

    private void UpdateStatistics()
    {
        EnabledCount = StartupPrograms.Count(p => p.IsEnabled);
        DisabledCount = StartupPrograms.Count(p => !p.IsEnabled);
        HighImpactCount = StartupPrograms.Count(p => p.IsEnabled && p.Impact == StartupImpact.High);
        MediumImpactCount = StartupPrograms.Count(p => p.IsEnabled && p.Impact == StartupImpact.Medium);
        LowImpactCount = StartupPrograms.Count(p => p.IsEnabled && p.Impact == StartupImpact.Low);
    }

    private async void UpdateEstimatedStartupTime()
    {
        EstimatedStartupTime = await _startupService.EstimateTotalStartupTimeAsync(StartupPrograms.ToList());
        OnPropertyChanged(nameof(EstimatedStartupTimeDisplay));
    }

    public string EstimatedStartupTimeDisplay => $"{EstimatedStartupTime:F1} saniye";
}
