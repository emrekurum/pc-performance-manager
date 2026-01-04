using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

    public DashboardViewModel()
    {
        _memoryService = new MemoryService();
        _powerService = new PowerService();
        _cleanupService = new CleanupService();

        LoadSystemInfo();
        _ = RefreshDataAsync(); // Fire and forget
    }

    private void LoadSystemInfo()
    {
        SystemInfo = SystemInfoHelper.GetSystemInfo();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Refreshing...";

        try
        {
            await Task.Run(() =>
            {
                // Load system info
                SystemInfo = SystemInfoHelper.GetSystemInfo();

                // Load memory info
                MemoryInfo = _memoryService.GetMemoryInfo();

                // Load CPU usage
                CpuUsage = SystemInfoHelper.GetCpuUsagePercentage();

                // Load active power plan
                ActivePowerPlan = _powerService.GetActivePowerPlan();
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

    [RelayCommand]
    private async Task QuickRamCleanupAsync()
    {
        if (IsRefreshing) return;

        var result = MessageBox.Show(
            "Do you want to clear RAM memory? This will free up memory but may slow down running applications temporarily.",
            "Clear RAM",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsRefreshing = true;
            StatusMessage = "Clearing RAM...";

            try
            {
                var success = await _memoryService.ClearMemoryAsync();

                if (success)
                {
                    StatusMessage = "RAM cleared successfully";
                    await RefreshDataAsync();
                    MessageBox.Show("RAM has been cleared successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Failed to clear RAM. Make sure you're running as administrator.";
                    MessageBox.Show("Failed to clear RAM. Please ensure the application is running with administrator privileges.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        // Navigation will be handled by MainViewModel
        StatusMessage = "Navigate to RAM view";
    }

    [RelayCommand]
    private void OpenPowerView()
    {
        StatusMessage = "Navigate to Power view";
    }

    [RelayCommand]
    private void OpenCleanupView()
    {
        StatusMessage = "Navigate to Cleanup view";
    }
}

