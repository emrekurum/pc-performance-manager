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

public partial class CleanupViewModel : ObservableObject
{
    private readonly ICleanupService _cleanupService;

    [ObservableProperty]
    private ObservableCollection<CleanupItem> cleanupItems = new();

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private bool isCleaning;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private long totalBytesToClean;

    [ObservableProperty]
    private int selectedItemsCount;

    [ObservableProperty]
    private string lastCleanupResult = string.Empty;

    public CleanupViewModel()
    {
        _cleanupService = new CleanupService();
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (IsAnalyzing) return;

        IsAnalyzing = true;
        StatusMessage = "Analyzing files...";

        try
        {
            var items = await _cleanupService.AnalyzeCleanupItemsAsync();

            CleanupItems.Clear();
            foreach (var item in items)
            {
                CleanupItems.Add(item);
            }

            UpdateStatistics();
            StatusMessage = $"Analysis complete - Found {CleanupItems.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred during analysis: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task CleanupAsync()
    {
        var selectedItems = CleanupItems.Where(i => i.IsSelected).ToList();

        if (selectedItems.Count == 0)
        {
            MessageBox.Show("Please select at least one item to clean.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var totalSizeGB = selectedItems.Sum(i => i.SizeGB);
        var result = MessageBox.Show(
            $"Do you want to clean {selectedItems.Count} selected item(s)?\n" +
            $"This will free approximately {totalSizeGB:F2} GB of disk space.\n\n" +
            "Warning: This action cannot be undone!",
            "Confirm Cleanup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsCleaning = true;
        StatusMessage = "Cleaning...";

        try
        {
            var cleanupResult = await _cleanupService.CleanupAsync(CleanupItems.ToList());

            if (cleanupResult.Success)
            {
                LastCleanupResult = cleanupResult.Message;
                StatusMessage = $"Cleanup complete - {cleanupResult.ItemsCleaned} items cleaned, {cleanupResult.GBCleaned:F2} GB freed";

                // Remove cleaned items from list
                var cleanedPaths = selectedItems.Select(i => i.Path).ToList();
                var itemsToRemove = CleanupItems.Where(i => cleanedPaths.Contains(i.Path)).ToList();
                
                foreach (var item in itemsToRemove)
                {
                    CleanupItems.Remove(item);
                }

                UpdateStatistics();

                MessageBox.Show(
                    $"Cleanup completed successfully!\n\n" +
                    $"{cleanupResult.ItemsCleaned} items cleaned\n" +
                    $"{cleanupResult.GBCleaned:F2} GB of disk space freed",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = $"Cleanup failed: {cleanupResult.Message}";
                MessageBox.Show($"Cleanup failed: {cleanupResult.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred during cleanup: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsCleaning = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in CleanupItems)
        {
            item.IsSelected = true;
        }
        UpdateStatistics();
        StatusMessage = "All items selected";
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in CleanupItems)
        {
            item.IsSelected = false;
        }
        UpdateStatistics();
        StatusMessage = "All items deselected";
    }

    [RelayCommand]
    private void SelectByCategory(CleanupCategory category)
    {
        foreach (var item in CleanupItems.Where(i => i.Category == category))
        {
            item.IsSelected = true;
        }
        UpdateStatistics();
        StatusMessage = $"Selected all {category} items";
    }

    private void UpdateStatistics()
    {
        var selected = CleanupItems.Where(i => i.IsSelected).ToList();
        SelectedItemsCount = selected.Count;
        TotalBytesToClean = selected.Sum(i => i.SizeBytes);
    }

    partial void OnCleanupItemsChanged(ObservableCollection<CleanupItem> value)
    {
        if (value != null)
        {
            foreach (var item in value)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CleanupItem.IsSelected))
                    {
                        UpdateStatistics();
                    }
                };
            }
        }
        UpdateStatistics();
    }
}

