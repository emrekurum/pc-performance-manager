using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;
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
    private UserControl? currentContent;

    public MainViewModel()
    {
        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new NavigationItem { Title = "Dashboard", Icon = "📊" },
            new NavigationItem { Title = "RAM", Icon = "💾" },
            new NavigationItem { Title = "Güç", Icon = "⚡" },
            new NavigationItem { Title = "Temizlik", Icon = "🧹" }
        };

        SelectedNavigationItem = NavigationItems[0];
        CurrentContent = new DashboardView();
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value == null) return;

        CurrentContent = value.Title switch
        {
            "Dashboard" => new DashboardView(),
            "RAM" => new RamView(),
            "Güç" => new PowerView(),
            "Temizlik" => new CleanupView(),
            _ => new DashboardView()
        };
    }
}

