using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PcPerformanceManager.Helpers;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;

namespace PcPerformanceManager.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IMemoryService _memoryService;
    private readonly IPowerService _powerService;
    private readonly ICleanupService _cleanupService;

    [ObservableProperty]
    private SystemInfo systemInfo = new();

    [ObservableProperty]
    private MemoryInfo memoryInfo = new();

    [ObservableProperty]
    private double cpuUsage;

    [ObservableProperty]
    private PowerPlan? activePowerPlan;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string statusMessage = "Ready";

    private System.Windows.Threading.DispatcherTimer? _autoRefreshTimer;

    // Formatted strings for display
    public string MemoryUsedDisplay => $"{MemoryInfo.UsedGB:F1} GB / {MemoryInfo.TotalGB:F1} GB";
    public string DiskUsedDisplay => $"{SystemInfo.UsedDiskGB:F1} GB / {SystemInfo.TotalDiskGB:F1} GB";

    public DashboardViewModel()
    {
        _memoryService = new MemoryService();
        _powerService = new PowerService();
        _cleanupService = new CleanupService();

        LoadSystemInfo();
        LoadMemoryInfo();
        _ = RefreshDataAsync(); // Fire and forget
        InitializeAutoRefresh();
    }

    private void InitializeAutoRefresh()
    {
        try
        {
            var settingsService = new SettingsService();
            var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
            
            if (settings.AutoRefreshEnabled)
            {
                _autoRefreshTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(settings.RefreshIntervalSeconds)
                };
                _autoRefreshTimer.Tick += async (s, e) => await RefreshDataAsync();
                _autoRefreshTimer.Start();
            }
        }
        catch
        {
            // Settings yüklenemezse auto refresh devre dışı
        }
    }

    private void LoadSystemInfo()
    {
        SystemInfo = SystemInfoHelper.GetSystemInfo();
    }

    private void LoadMemoryInfo()
    {
        MemoryInfo = _memoryService.GetMemoryInfo();
        OnPropertyChanged(nameof(MemoryUsedDisplay));
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Refreshing...";

        try
        {
            SystemInfo systemInfoData = new();
            MemoryInfo memoryInfoData = new();
            double cpuUsageData = 0;
            PowerPlan? activePowerPlanData = null;

            await Task.Run(() =>
            {
                // Load system info
                systemInfoData = SystemInfoHelper.GetSystemInfo();

                // Load memory info
                memoryInfoData = _memoryService.GetMemoryInfo();

                // Load CPU usage
                cpuUsageData = SystemInfoHelper.GetCpuUsagePercentage();

                // Load active power plan
                activePowerPlanData = _powerService.GetActivePowerPlan();
            });

            // Update UI thread - must be on UI thread for property changes
            Application.Current.Dispatcher.Invoke(() =>
            {
                SystemInfo = systemInfoData;
                MemoryInfo = memoryInfoData;
                CpuUsage = cpuUsageData;
                ActivePowerPlan = activePowerPlanData;

                OnPropertyChanged(nameof(MemoryUsedDisplay));
                OnPropertyChanged(nameof(DiskUsedDisplay));
            });

            StatusMessage = "Data refreshed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnMemoryInfoChanged(MemoryInfo value)
    {
        OnPropertyChanged(nameof(MemoryUsedDisplay));
    }

    partial void OnSystemInfoChanged(SystemInfo value)
    {
        OnPropertyChanged(nameof(DiskUsedDisplay));
    }

    public void UpdateVisibility(AppSettings settings)
    {
        // Dashboard kartlarının görünürlüğünü güncelle
        OnPropertyChanged(nameof(ShowCpuCard));
        OnPropertyChanged(nameof(ShowMemoryCard));
        OnPropertyChanged(nameof(ShowDiskCard));
        OnPropertyChanged(nameof(ShowPowerCard));
        
        // Auto refresh'i güncelle
        _autoRefreshTimer?.Stop();
        _autoRefreshTimer = null;
        
        if (settings.AutoRefreshEnabled)
        {
            _autoRefreshTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(settings.RefreshIntervalSeconds)
            };
            _autoRefreshTimer.Tick += async (s, e) => await RefreshDataAsync();
            _autoRefreshTimer.Start();
        }
    }

    // Visibility properties for dashboard cards
    public bool ShowCpuCard
    {
        get
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
                return settings.ShowCpuCard;
            }
            catch
            {
                return true;
            }
        }
    }

    public bool ShowMemoryCard
    {
        get
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
                return settings.ShowMemoryCard;
            }
            catch
            {
                return true;
            }
        }
    }

    public bool ShowDiskCard
    {
        get
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
                return settings.ShowDiskCard;
            }
            catch
            {
                return true;
            }
        }
    }

    public bool ShowPowerCard
    {
        get
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
                return settings.ShowPowerCard;
            }
            catch
            {
                return true;
            }
        }
    }

    [RelayCommand]
    private async Task QuickRamCleanupAsync()
    {
        if (IsRefreshing) return;

        var result = MessageBox.Show(
            "Hızlı RAM temizliği yapılacak.\n\n" +
            "• Aktif uygulamalarınız korunacak\n" +
            "• Arka plandaki gereksiz process'ler temizlenecek\n\n" +
            "Devam etmek istiyor musunuz?",
            "Hızlı RAM Temizliği",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsRefreshing = true;
            StatusMessage = "RAM temizleniyor...";

            try
            {
                var success = await _memoryService.ClearMemoryAsync();

                if (success)
                {
                    StatusMessage = "RAM başarıyla temizlendi";
                    await RefreshDataAsync();
                    MessageBox.Show("RAM başarıyla temizlendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Temizlenecek process bulunamadı";
                    MessageBox.Show("Temizlenecek uygun process bulunamadı veya tüm process'ler zaten optimize edilmiş.", 
                        "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hata: {ex.Message}";
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }

    [RelayCommand]
    private void OpenRamView()
    {
        NavigateToView("RAM");
    }

    [RelayCommand]
    private void OpenPowerView()
    {
        NavigateToView("Güç");
    }

    [RelayCommand]
    private void OpenCleanupView()
    {
        NavigateToView("Temizlik");
    }

    private void NavigateToView(string viewName)
    {
        try
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow?.DataContext is MainViewModel mainViewModel)
            {
                var targetItem = mainViewModel.NavigationItems.FirstOrDefault(n => n.Title == viewName);
                if (targetItem != null)
                {
                    mainViewModel.SelectedNavigationItem = targetItem;
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Navigation error: {ex.Message}";
        }
    }
}
