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

public partial class ServiceViewModel : ObservableObject
{
    private readonly IServiceService _serviceService;

    [ObservableProperty]
    private ObservableCollection<WindowsService> services = new();

    [ObservableProperty]
    private WindowsService? selectedService;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private string statusMessage = "Hazır";

    [ObservableProperty]
    private int runningCount;

    [ObservableProperty]
    private int stoppedCount;

    [ObservableProperty]
    private int automaticCount;

    [ObservableProperty]
    private int manualCount;

    [ObservableProperty]
    private int disabledCount;

    [ObservableProperty]
    private int criticalCount;

    public ServiceViewModel()
    {
        _serviceService = new ServiceService();
        _ = LoadServicesAsync();
    }

    [RelayCommand]
    private async Task LoadServicesAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Windows servisleri yükleniyor...";

        try
        {
            var servicesList = await _serviceService.GetServicesAsync();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                Services.Clear();
                foreach (var service in servicesList)
                {
                    Services.Add(service);
                }
                
                UpdateStatistics();
            });

            StatusMessage = $"{servicesList.Count} servis bulundu";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Windows servisleri yüklenirken hata oluştu:\n{ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task StartServiceAsync(WindowsService? service)
    {
        if (service == null || IsProcessing) return;

        IsProcessing = true;
        StatusMessage = $"{service.DisplayName} başlatılıyor...";

        try
        {
            if (await _serviceService.StartServiceAsync(service))
            {
                // Servisi yeniden yükle
                await RefreshServiceStatusAsync(service);
                UpdateStatistics();
                StatusMessage = $"{service.DisplayName} başlatıldı";
            }
            else
            {
                StatusMessage = $"{service.DisplayName} başlatılamadı";
                MessageBox.Show(
                    $"{service.DisplayName} başlatılamadı.\n" +
                    "Yönetici izinlerini kontrol edin ve tekrar deneyin.",
                    "İşlem Başarısız",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Servis başlatılırken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task StopServiceAsync(WindowsService? service)
    {
        if (service == null || IsProcessing) return;

        // Kritik servis kontrolü
        if (service.IsCritical)
        {
            MessageBox.Show(
                $"{service.DisplayName} kritik bir sistem servisidir ve durdurulmamalıdır.\n" +
                "Bu servisi durdurmak sistem kararsızlığına neden olabilir.",
                "Kritik Servis Uyarısı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"{service.DisplayName} servisi durdurulacak. Devam etmek istiyor musunuz?",
            "Onay",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsProcessing = true;
        StatusMessage = $"{service.DisplayName} durduruluyor...";

        try
        {
            if (await _serviceService.StopServiceAsync(service))
            {
                await RefreshServiceStatusAsync(service);
                UpdateStatistics();
                StatusMessage = $"{service.DisplayName} durduruldu";
            }
            else
            {
                StatusMessage = $"{service.DisplayName} durdurulamadı";
                MessageBox.Show(
                    $"{service.DisplayName} durdurulamadı.\n" +
                    "Yönetici izinlerini kontrol edin ve tekrar deneyin.",
                    "İşlem Başarısız",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Servis durdurulurken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ChangeStartupTypeToAutomaticAsync(WindowsService? service)
    {
        if (service != null)
            await ChangeStartupTypeAsync(service, ServiceStartType.Automatic);
    }

    [RelayCommand]
    private async Task ChangeStartupTypeToAutomaticDelayedAsync(WindowsService? service)
    {
        if (service != null)
            await ChangeStartupTypeAsync(service, ServiceStartType.AutomaticDelayed);
    }

    [RelayCommand]
    private async Task ChangeStartupTypeToManualAsync(WindowsService? service)
    {
        if (service != null)
            await ChangeStartupTypeAsync(service, ServiceStartType.Manual);
    }

    [RelayCommand]
    private async Task ChangeStartupTypeToDisabledAsync(WindowsService? service)
    {
        if (service != null)
            await ChangeStartupTypeAsync(service, ServiceStartType.Disabled);
    }

    private async Task ChangeStartupTypeAsync(WindowsService service, ServiceStartType newStartType)
    {
        if (service == null || IsProcessing) return;

        // Kritik servis kontrolü - Disabled yapılamaz
        if (service.IsCritical && newStartType == ServiceStartType.Disabled)
        {
            MessageBox.Show(
                $"{service.DisplayName} kritik bir sistem servisidir ve devre dışı bırakılamaz.\n" +
                "Bu işlem sistem kararsızlığına neden olabilir.",
                "Kritik Servis Uyarısı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var startTypeText = newStartType switch
        {
            ServiceStartType.Automatic => "Otomatik",
            ServiceStartType.AutomaticDelayed => "Otomatik (Gecikmeli)",
            ServiceStartType.Manual => "Manuel",
            ServiceStartType.Disabled => "Devre Dışı",
            _ => "Bilinmiyor"
        };

        var result = MessageBox.Show(
            $"{service.DisplayName} servisinin başlangıç tipi '{startTypeText}' olarak değiştirilecek. Devam etmek istiyor musunuz?",
            "Onay",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsProcessing = true;
        StatusMessage = $"{service.DisplayName} başlangıç tipi değiştiriliyor...";

        try
        {
            if (await _serviceService.ChangeStartupTypeAsync(service, newStartType))
            {
                service.StartType = newStartType;
                UpdateStatistics();
                StatusMessage = $"{service.DisplayName} başlangıç tipi '{startTypeText}' olarak ayarlandı";
            }
            else
            {
                StatusMessage = $"{service.DisplayName} başlangıç tipi değiştirilemedi";
                MessageBox.Show(
                    $"{service.DisplayName} başlangıç tipi değiştirilemedi.\n" +
                    "Yönetici izinlerini kontrol edin ve tekrar deneyin.",
                    "İşlem Başarısız",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Başlangıç tipi değiştirilirken hata oluştu:\n{ex.Message}",
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
        foreach (var service in Services)
        {
            service.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var service in Services)
        {
            service.IsSelected = false;
        }
    }

    [RelayCommand]
    private void SelectRunning()
    {
        foreach (var service in Services.Where(s => s.Status == ServiceStatus.Running))
        {
            service.IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectStopped()
    {
        foreach (var service in Services.Where(s => s.Status == ServiceStatus.Stopped))
        {
            service.IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectSafeToStop()
    {
        foreach (var service in Services.Where(s => s.IsSafeToStop && s.Status == ServiceStatus.Running))
        {
            service.IsSelected = true;
        }
    }

    private async Task RefreshServiceStatusAsync(WindowsService service)
    {
        var allServices = await _serviceService.GetServicesAsync();
        var updatedService = allServices.FirstOrDefault(s => s.Name == service.Name);
        if (updatedService != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                service.Status = updatedService.Status;
                service.StartType = updatedService.StartType;
            });
        }
    }

    private void UpdateStatistics()
    {
        RunningCount = Services.Count(s => s.Status == ServiceStatus.Running);
        StoppedCount = Services.Count(s => s.Status == ServiceStatus.Stopped);
        AutomaticCount = Services.Count(s => s.StartType == ServiceStartType.Automatic || s.StartType == ServiceStartType.AutomaticDelayed);
        ManualCount = Services.Count(s => s.StartType == ServiceStartType.Manual);
        DisabledCount = Services.Count(s => s.StartType == ServiceStartType.Disabled);
        CriticalCount = Services.Count(s => s.IsCritical);
    }
}
