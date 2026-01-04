namespace PcPerformanceManager.Models;

public class CleanupItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsSelected { get; set; }
    public CleanupCategory Category { get; set; }

    // Helper property
    public double SizeGB => SizeBytes / (1024.0 * 1024.0 * 1024.0);
    public double SizeMB => SizeBytes / (1024.0 * 1024.0);
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

