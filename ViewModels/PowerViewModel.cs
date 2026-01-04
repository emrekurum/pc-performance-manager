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

public partial class PowerViewModel : ObservableObject
{
    private readonly IPowerService _powerService;

    [ObservableProperty]
    private ObservableCollection<PowerPlan> powerPlans = new();

    [ObservableProperty]
    private PowerPlan? selectedPowerPlan;

    [ObservableProperty]
    private PowerPlan? activePowerPlan;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public PowerViewModel()
    {
        _powerService = new PowerService();
        _ = LoadPowerPlansAsync(); // Fire and forget
    }

    [RelayCommand]
    private async Task LoadPowerPlansAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Loading power plans...";

        try
        {
            await Task.Run(() =>
            {
                var plans = _powerService.GetPowerPlans();
                var active = _powerService.GetActivePowerPlan();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PowerPlans.Clear();
                    foreach (var plan in plans)
                    {
                        PowerPlans.Add(plan);
                    }

                    ActivePowerPlan = active;
                    SelectedPowerPlan = active;
                });
            });

            StatusMessage = $"Loaded {PowerPlans.Count} power plans";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred while loading power plans: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsRefreshing = false;
        }
    }


    [RelayCommand]
    private async Task SetActivePowerPlanAsync()
    {
        if (SelectedPowerPlan == null)
        {
            MessageBox.Show("Please select a power plan first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedPowerPlan.Guid == ActivePowerPlan?.Guid)
        {
            MessageBox.Show("This power plan is already active.", "Already Active", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Do you want to set '{SelectedPowerPlan.Name}' as the active power plan?",
            "Change Power Plan",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsRefreshing = true;
        StatusMessage = "Changing power plan...";

        try
        {
            var success = await _powerService.SetActivePowerPlanAsync(SelectedPowerPlan.Guid);

            if (success)
            {
                ActivePowerPlan = SelectedPowerPlan;
                StatusMessage = $"Active power plan changed to '{SelectedPowerPlan.Name}'";

                // Update IsActive flags
                foreach (var plan in PowerPlans)
                {
                    plan.IsActive = plan.Guid == SelectedPowerPlan.Guid;
                }

                MessageBox.Show($"Power plan changed to '{SelectedPowerPlan.Name}' successfully.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Failed to change power plan";
                MessageBox.Show("Failed to change power plan. Please ensure the application is running with administrator privileges.",
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

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPowerPlansAsync();
    }
}

