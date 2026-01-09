namespace PcPerformanceManager.Models;

public class SystemInfo
{
    public string CpuName { get; set; } = string.Empty;
    public int CpuCores { get; set; }
    public int CpuLogicalProcessors { get; set; }
    public ulong TotalRamBytes { get; set; }
    public ulong TotalDiskBytes { get; set; }
    public ulong FreeDiskBytes { get; set; }

    // Helper properties
    public double TotalRamGB => TotalRamBytes / (1024.0 * 1024.0 * 1024.0);
    public double TotalDiskGB => TotalDiskBytes / (1024.0 * 1024.0 * 1024.0);
    public double FreeDiskGB => FreeDiskBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedDiskGB => TotalDiskGB - FreeDiskGB;
    public double UsedDiskPercentage => TotalDiskBytes > 0 ? (UsedDiskGB / TotalDiskGB) * 100 : 0;
}






