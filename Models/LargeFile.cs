using CommunityToolkit.Mvvm.ComponentModel;

namespace PcPerformanceManager.Models;

public partial class LargeFile : ObservableObject
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string Extension { get; set; } = string.Empty;
    
    [ObservableProperty]
    private bool isSelected;
    
    public double SizeGB => SizeBytes / (1024.0 * 1024.0 * 1024.0);
    public double SizeMB => SizeBytes / (1024.0 * 1024.0);
    
    public string SizeDisplay => SizeGB >= 1 
        ? $"{SizeGB:F2} GB" 
        : $"{SizeMB:F2} MB";
    
    public string LastModifiedDisplay => LastModified.ToString("dd.MM.yyyy HH:mm");
}
