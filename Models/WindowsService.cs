using CommunityToolkit.Mvvm.ComponentModel;

namespace PcPerformanceManager.Models;

public enum ServiceStatus
{
    Running,        // Çalışıyor
    Stopped,        // Durdurulmuş
    Paused,         // Duraklatılmış
    Starting,       // Başlatılıyor
    Stopping,       // Durduruluyor
    Unknown         // Bilinmiyor
}

public enum ServiceStartType
{
    Automatic,          // Otomatik
    AutomaticDelayed,   // Otomatik (Gecikmeli)
    Manual,            // Manuel
    Disabled,          // Devre Dışı
    Unknown            // Bilinmiyor
}

public partial class WindowsService : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;
    public ServiceStartType StartType { get; set; } = ServiceStartType.Unknown;
    
    [ObservableProperty]
    private bool isSelected;
    
    public bool IsCritical { get; set; }
    public bool IsSafeToStop { get; set; }
    public string Category { get; set; } = string.Empty;
    
    public string StatusDisplay => Status switch
    {
        ServiceStatus.Running => "Çalışıyor",
        ServiceStatus.Stopped => "Durdurulmuş",
        ServiceStatus.Paused => "Duraklatılmış",
        ServiceStatus.Starting => "Başlatılıyor",
        ServiceStatus.Stopping => "Durduruluyor",
        _ => "Bilinmiyor"
    };
    
    public string StartTypeDisplay => StartType switch
    {
        ServiceStartType.Automatic => "Otomatik",
        ServiceStartType.AutomaticDelayed => "Otomatik (Gecikmeli)",
        ServiceStartType.Manual => "Manuel",
        ServiceStartType.Disabled => "Devre Dışı",
        _ => "Bilinmiyor"
    };
    
    // ServiceController'dan ServiceStatus'e dönüştürme (ServiceService'de implement edilecek)
    
    // ServiceController'dan ServiceStartType'a dönüştürme (WMI ile alınacak)
    public static ServiceStartType ConvertStartType(string startType)
    {
        return startType.ToUpperInvariant() switch
        {
            "AUTO" => ServiceStartType.Automatic,
            "AUTOMATIC" => ServiceStartType.Automatic,
            "AUTODELAYED" => ServiceStartType.AutomaticDelayed,
            "AUTOMATIC(DELAYED START)" => ServiceStartType.AutomaticDelayed,
            "MANUAL" => ServiceStartType.Manual,
            "DEMAND" => ServiceStartType.Manual,
            "DISABLED" => ServiceStartType.Disabled,
            _ => ServiceStartType.Unknown
        };
    }
}
