using CommunityToolkit.Mvvm.ComponentModel;

namespace PcPerformanceManager.Models;

public partial class FolderSize : ObservableObject
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int FileCount { get; set; }
    public int FolderCount { get; set; }
    
    [ObservableProperty]
    private bool isExpanded;
    
    [ObservableProperty]
    private bool isSelected;
    
    public double SizeGB => SizeBytes / (1024.0 * 1024.0 * 1024.0);
    public double SizeMB => SizeBytes / (1024.0 * 1024.0);
    
    public string SizeDisplay => SizeGB >= 1 
        ? $"{SizeGB:F2} GB" 
        : $"{SizeMB:F2} MB";
    
    public string ItemCountDisplay => $"{FileCount} dosya, {FolderCount} klasör";
}
