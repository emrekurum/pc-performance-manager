using CommunityToolkit.Mvvm.ComponentModel;

namespace PcPerformanceManager.Models;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private bool startWithWindows;

    [ObservableProperty]
    private bool autoRefreshEnabled;

    [ObservableProperty]
    private int refreshIntervalSeconds = 5;

    [ObservableProperty]
    private bool minimizeToTray;

    [ObservableProperty]
    private bool showNotifications;

    [ObservableProperty]
    private bool autoCleanupOnStartup;

    [ObservableProperty]
    private bool checkForUpdates;

    [ObservableProperty]
    private string language = "tr-TR";

    [ObservableProperty]
    private string theme = "Dark";

    [ObservableProperty]
    private bool enableLogging;

    [ObservableProperty]
    private string logLevel = "Info";

    // RAM ayarları
    [ObservableProperty]
    private bool autoRamCleanupEnabled;

    [ObservableProperty]
    private int ramCleanupThresholdMB = 2048; // 2GB

    // Dashboard ayarları
    [ObservableProperty]
    private bool showCpuCard = true;

    [ObservableProperty]
    private bool showMemoryCard = true;

    [ObservableProperty]
    private bool showDiskCard = true;

    [ObservableProperty]
    private bool showPowerCard = true;
}
