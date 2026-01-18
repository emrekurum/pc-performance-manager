using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;
using PcPerformanceManager.Views;

namespace PcPerformanceManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<NavigationItem> navigationItems = new();

    [ObservableProperty]
    private NavigationItem? selectedNavigationItem;

    [ObservableProperty]
    private System.Windows.Controls.UserControl? currentContent;

    public MainViewModel()
    {
        InitializeNavigationItems();
        
        SelectedNavigationItem = NavigationItems[0];
        CurrentContent = CreateViewWithViewModel("Dashboard");
        
        // Listen for language changes
        if (Application.Current is App app)
        {
            // Update navigation items when language changes
            App.LocalizationService.ChangeLanguage(App.LocalizationService.CurrentLanguage);
        }
    }
    
    private void InitializeNavigationItems()
    {
        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new NavigationItem { Key = "Dashboard", Title = GetLocalizedString("NavDashboard", "Dashboard"), Icon = "📊" },
            new NavigationItem { Key = "RAM", Title = GetLocalizedString("NavRam", "RAM"), Icon = "💾" },
            new NavigationItem { Key = "DiskAnalizi", Title = GetLocalizedString("NavDiskAnalyzer", "Disk Analyzer"), Icon = "💿" },
            new NavigationItem { Key = "Başlangıç", Title = GetLocalizedString("NavStartup", "Startup"), Icon = "🚀" },
            new NavigationItem { Key = "Servisler", Title = GetLocalizedString("NavServices", "Services"), Icon = "⚙️" },
            new NavigationItem { Key = "Güç", Title = GetLocalizedString("NavPower", "Power"), Icon = "⚡" },
            new NavigationItem { Key = "Temizlik", Title = GetLocalizedString("NavCleanup", "Cleanup"), Icon = "🧹" },
            new NavigationItem { Key = "Ayarlar", Title = GetLocalizedString("NavSettings", "Settings"), Icon = "🔧" }
        };
    }
    
    public void RefreshNavigationItems()
    {
        foreach (var item in NavigationItems)
        {
            item.Title = GetLocalizedString($"Nav{item.Key}", item.Title);
        }
    }
    
    private string GetLocalizedString(string key, string defaultValue)
    {
        if (Application.Current?.Resources.Contains(key) == true)
        {
            return Application.Current.Resources[key]?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value == null) return;

        CurrentContent = value.Key switch
        {
            "Dashboard" => CreateViewWithViewModel("Dashboard"),
            "RAM" => CreateViewWithViewModel("RAM"),
            "DiskAnalizi" => CreateViewWithViewModel("Disk Analizi"),
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

