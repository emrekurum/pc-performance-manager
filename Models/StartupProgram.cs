using CommunityToolkit.Mvvm.ComponentModel;

namespace PcPerformanceManager.Models;

public enum StartupImpact
{
    Low,      // Düşük etki - hızlı başlar
    Medium,   // Orta etki - normal başlar
    High,     // Yüksek etki - yavaş başlar
    Unknown   // Bilinmiyor
}

public enum StartupLocation
{
    Registry,           // HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
    RegistryMachine,    // HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
    StartupFolder,      // %APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
    TaskScheduler,      // Windows Task Scheduler
    Service            // Windows Service
}

public partial class StartupProgram : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StartupLocation Location { get; set; }
    public StartupImpact Impact { get; set; } = StartupImpact.Unknown;
    public string LocationPath { get; set; } = string.Empty; // Registry path veya file path
    
    [ObservableProperty]
    private bool isEnabled;
    
    [ObservableProperty]
    private bool isSelected;
    
    public string ImpactDisplay => Impact switch
    {
        StartupImpact.Low => "Düşük",
        StartupImpact.Medium => "Orta",
        StartupImpact.High => "Yüksek",
        _ => "Bilinmiyor"
    };
    
    public string LocationDisplay => Location switch
    {
        StartupLocation.Registry => "Kayıt Defteri (Kullanıcı)",
        StartupLocation.RegistryMachine => "Kayıt Defteri (Sistem)",
        StartupLocation.StartupFolder => "Başlangıç Klasörü",
        StartupLocation.TaskScheduler => "Zamanlanmış Görevler",
        StartupLocation.Service => "Windows Servisi",
        _ => "Bilinmiyor"
    };
}

