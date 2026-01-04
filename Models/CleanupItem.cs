using System.ComponentModel;

namespace PcPerformanceManager.Models;

public class CleanupItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
    
    public CleanupCategory Category { get; set; }

    // Helper property
    public double SizeGB => SizeBytes / (1024.0 * 1024.0 * 1024.0);
    public double SizeMB => SizeBytes / (1024.0 * 1024.0);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum CleanupCategory
{
    TemporaryFiles,
    CacheFiles,
    LogFiles,
    RecycleBin,
    WindowsUpdate,
    BrowserCache,
    Other
}

