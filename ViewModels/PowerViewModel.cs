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
    private async Task SetActivePowerPlanAsync(PowerPlan? plan)
    {
        // Eğer parametre olarak plan geldiyse onu kullan, yoksa SelectedPowerPlan
        var targetPlan = plan ?? SelectedPowerPlan;
        
        if (targetPlan == null)
        {
            MessageBox.Show("Lütfen önce bir güç planı seçin.", "Seçim Yok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (targetPlan.Guid == ActivePowerPlan?.Guid)
        {
            MessageBox.Show("Bu güç planı zaten aktif.", "Zaten Aktif", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"'{targetPlan.Name}' güç planını aktif yapmak istiyor musunuz?",
            "Güç Planı Değiştir",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsRefreshing = true;
        StatusMessage = "Güç planı değiştiriliyor...";

        try
        {
            var success = await _powerService.SetActivePowerPlanAsync(targetPlan.Guid);

            if (success)
            {
                // UI'ı hemen güncelle - mevcut koleksiyondaki IsActive flaglerini değiştir
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var p in PowerPlans)
                    {
                        p.IsActive = p.Guid == targetPlan.Guid;
                    }
                    ActivePowerPlan = targetPlan;
                    SelectedPowerPlan = targetPlan;
                });

                StatusMessage = $"Güç planı '{targetPlan.Name}' olarak değiştirildi";
                MessageBox.Show($"Güç planı '{targetPlan.Name}' olarak başarıyla değiştirildi!",
                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Güç planı değiştirilemedi";
                MessageBox.Show("Güç planı değiştirilemedi. Lütfen uygulamanın yönetici olarak çalıştığından emin olun.",
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPowerPlansAsync();
    }
}

