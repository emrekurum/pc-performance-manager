using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PcPerformanceManager.Models;
using PcPerformanceManager.Views;

namespace PcPerformanceManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<NavigationItem> navigationItems;

    [ObservableProperty]
    private NavigationItem? selectedNavigationItem;

    [ObservableProperty]
    private System.Windows.Controls.UserControl? currentContent;

    public MainViewModel()
    {
        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new NavigationItem { Title = "Dashboard", Icon = "📊" },
            new NavigationItem { Title = "RAM", Icon = "💾" },
            new NavigationItem { Title = "Disk Analizi", Icon = "💿" },
            new NavigationItem { Title = "Başlangıç", Icon = "🚀" },
            new NavigationItem { Title = "Servisler", Icon = "⚙️" },
            new NavigationItem { Title = "Güç", Icon = "⚡" },
            new NavigationItem { Title = "Temizlik", Icon = "🧹" },
            new NavigationItem { Title = "Ayarlar", Icon = "🔧" }
        };

        SelectedNavigationItem = NavigationItems[0];
        CurrentContent = CreateViewWithViewModel("Dashboard");
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value == null) return;

        CurrentContent = value.Title switch
        {
            "Dashboard" => CreateViewWithViewModel("Dashboard"),
            "RAM" => CreateViewWithViewModel("RAM"),
            "Disk Analizi" => CreateViewWithViewModel("Disk Analizi"),
            "Başlangıç" => CreateViewWithViewModel("Başlangıç"),
            "Servisler" => CreateViewWithViewModel("Servisler"),
            "Güç" => CreateViewWithViewModel("Güç"),
            "Temizlik" => CreateViewWithViewModel("Temizlik"),
            "Ayarlar" => CreateViewWithViewModel("Ayarlar"),
            _ => CreateViewWithViewModel("Dashboard")
        };
    }

    private System.Windows.Controls.UserControl CreateViewWithViewModel(string viewName)
    {
        try
        {
            return viewName switch
            {
                "Dashboard" => new DashboardView { DataContext = new DashboardViewModel() },
                "RAM" => new RamView { DataContext = new RamViewModel() },
                "Disk Analizi" => new DiskAnalyzerView { DataContext = new DiskAnalyzerViewModel() },
                "Başlangıç" => new StartupView { DataContext = new StartupViewModel() },
                "Servisler" => new ServiceView { DataContext = new ServiceViewModel() },
                "Güç" => new PowerView { DataContext = new PowerViewModel() },
                "Temizlik" => new CleanupView { DataContext = new CleanupViewModel() },
                "Ayarlar" => new SettingsView { DataContext = new SettingsViewModel() },
                _ => new DashboardView { DataContext = new DashboardViewModel() }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating view {viewName}: {ex.Message}");
            System.Windows.MessageBox.Show($"Error loading view: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return new DashboardView { DataContext = new DashboardViewModel() };
        }
    }

    public void UpdateDashboardVisibility(AppSettings settings)
    {
        // Dashboard görünümünü güncelle
        if (CurrentContent is DashboardView dashboardView && 
            dashboardView.DataContext is DashboardViewModel dashboardViewModel)
        {
            dashboardViewModel.UpdateVisibility(settings);
        }
    }
}

