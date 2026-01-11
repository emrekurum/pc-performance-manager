using CommunityToolkit.Mvvm.ComponentModel;

namespace PcPerformanceManager.Models;

public partial class DiskInfo : ObservableObject
{
    public string DriveLetter { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FileSystem { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public long FreeBytes { get; set; }
    public long UsedBytes => TotalBytes - FreeBytes;
    
    [ObservableProperty]
    private bool isSelected;
    
    public double TotalGB => TotalBytes / (1024.0 * 1024.0 * 1024.0);
    public double FreeGB => FreeBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedGB => UsedBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedPercentage => TotalBytes > 0 ? (UsedBytes / (double)TotalBytes) * 100 : 0;
    public double FreePercentage => TotalBytes > 0 ? (FreeBytes / (double)TotalBytes) * 100 : 0;
    
    public string DisplayName => $"{DriveLetter} ({Label})";
    public string UsedDisplay => $"{UsedGB:F1} GB / {TotalGB:F1} GB";
}
