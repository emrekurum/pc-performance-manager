namespace PcPerformanceManager.Models;

public class ProcessMemoryInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public long WorkingSetBytes { get; set; }
    public double WorkingSetMB => WorkingSetBytes / (1024.0 * 1024.0);
}


