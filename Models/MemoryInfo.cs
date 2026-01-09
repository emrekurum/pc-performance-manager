namespace PcPerformanceManager.Models;

public class MemoryInfo
{
    public ulong TotalBytes { get; set; }
    public ulong UsedBytes { get; set; }
    public ulong FreeBytes { get; set; }

    // Helper properties
    public double TotalGB => TotalBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedGB => UsedBytes / (1024.0 * 1024.0 * 1024.0);
    public double FreeGB => FreeBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedPercentage => TotalBytes > 0 ? (UsedBytes / (double)TotalBytes) * 100 : 0;
    public double FreePercentage => TotalBytes > 0 ? (FreeBytes / (double)TotalBytes) * 100 : 0;
}






