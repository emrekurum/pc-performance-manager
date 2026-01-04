using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;

namespace PcPerformanceManager.ViewModels;

public partial class RamViewModel : ObservableObject
{
    private readonly IMemoryService _memoryService;
    private DispatcherTimer? _refreshTimer;

    [ObservableProperty]
    private MemoryInfo memoryInfo = new();

    [ObservableProperty]
    private ObservableCollection<ProcessMemoryInfo> processes = new();

    [ObservableProperty]
    private ProcessMemoryInfo? selectedProcess;

    [ObservableProperty]
    private bool isAutoRefreshEnabled;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isClearingMemory;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private int refreshIntervalSeconds = 5;

    public RamViewModel()
    {
        _memoryService = new MemoryService();
        _ = RefreshDataAsync(); // Fire and forget
    }

    partial void OnIsAutoRefreshEnabledChanged(bool value)
    {
        if (value)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }
    }

    partial void OnRefreshIntervalSecondsChanged(int value)
    {
        if (IsAutoRefreshEnabled)
        {
            StopAutoRefresh();
            StartAutoRefresh();
        }
    }

    private void StartAutoRefresh()
    {
        _refreshTimer?.Stop();
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(RefreshIntervalSeconds)
        };
        _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();
        _refreshTimer.Start();
    }

    private void StopAutoRefresh()
    {
        _refreshTimer?.Stop();
        _refreshTimer = null;
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
                MemoryInfo = _memoryService.GetMemoryInfo();
                var processList = _memoryService.GetProcessMemoryUsage();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Processes.Clear();
                    foreach (var process in processList)
                    {
                        Processes.Add(process);
                    }
                });
            });

            StatusMessage = $"Updated - {Processes.Count} processes";
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
    private async Task ClearAllMemoryAsync()
    {
        var result = MessageBox.Show(
            "This will clear the working set of all processes. Continue?",
            "Clear All RAM",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsClearingMemory = true;
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
                StatusMessage = "Failed to clear RAM";
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
            IsClearingMemory = false;
        }
    }

    [RelayCommand]
    private async Task ClearSelectedProcessMemoryAsync()
    {
        if (SelectedProcess == null)
        {
            MessageBox.Show("Please select a process first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Clear memory for process '{SelectedProcess.ProcessName}' (PID: {SelectedProcess.ProcessId})?",
            "Clear Process Memory",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var success = await Task.Run(() => _memoryService.ClearProcessMemory(SelectedProcess.ProcessId));

            if (success)
            {
                StatusMessage = $"Memory cleared for {SelectedProcess.ProcessName}";
                await RefreshDataAsync();
            }
            else
            {
                StatusMessage = "Failed to clear process memory";
                MessageBox.Show("Failed to clear process memory. Make sure you have administrator privileges.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SortByMemoryUsage()
    {
        var sorted = Processes.OrderByDescending(p => p.WorkingSetBytes).ToList();
        Processes.Clear();
        foreach (var process in sorted)
        {
            Processes.Add(process);
        }
        StatusMessage = "Sorted by memory usage";
    }

    [RelayCommand]
    private void SortByName()
    {
        var sorted = Processes.OrderBy(p => p.ProcessName).ToList();
        Processes.Clear();
        foreach (var process in sorted)
        {
            Processes.Add(process);
        }
        StatusMessage = "Sorted by name";
    }
}

