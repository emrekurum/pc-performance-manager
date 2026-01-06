using System;
using System.Diagnostics;
using System.Management;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Helpers;

public static class SystemInfoHelper
{
    /// <summary>
    /// Sistem bilgilerini toplar ve SystemInfo modeli olarak döndürür
    /// </summary>
    public static SystemInfo GetSystemInfo()
    {
        var systemInfo = new SystemInfo();

        try
        {
            // RAM Bilgileri
            using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["TotalPhysicalMemory"] != null)
                    {
                        systemInfo.TotalRamBytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                    }
                }
            }

            // CPU Bilgileri
            using (var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    systemInfo.CpuName = obj["Name"]?.ToString() ?? "Bilinmeyen";
                    systemInfo.CpuCores = Convert.ToInt32(obj["NumberOfCores"] ?? 0);
                    systemInfo.CpuLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                    break; // İlk CPU'yu al
                }
            }

            // Disk Bilgileri
            using (var searcher = new ManagementObjectSearcher("SELECT Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3"))
            {
                ulong totalDiskBytes = 0;
                ulong freeDiskBytes = 0;

                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["Size"] != null)
                        totalDiskBytes += Convert.ToUInt64(obj["Size"]);
                    if (obj["FreeSpace"] != null)
                        freeDiskBytes += Convert.ToUInt64(obj["FreeSpace"]);
                }

                systemInfo.TotalDiskBytes = totalDiskBytes;
                systemInfo.FreeDiskBytes = freeDiskBytes;
            }
        }
        catch (Exception ex)
        {
            // Hata durumunda loglanabilir
            System.Diagnostics.Debug.WriteLine($"SystemInfo alma hatası: {ex.Message}");
        }

        return systemInfo;
    }

    /// <summary>
    /// Anlık CPU kullanım yüzdesini döndürür
    /// </summary>
    public static double GetCpuUsagePercentage()
    {
        try
        {
            using (var performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                performanceCounter.NextValue(); // İlk değer 0 olabilir
                System.Threading.Thread.Sleep(100);
                return Math.Round(performanceCounter.NextValue(), 2);
            }
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Anlık RAM kullanım bilgilerini döndürür
    /// </summary>
    public static MemoryInfo GetMemoryInfo()
    {
        var memoryInfo = new MemoryInfo();

        try
        {
            // Total Physical Memory from Win32_ComputerSystem
            using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["TotalPhysicalMemory"] != null)
                    {
                        memoryInfo.TotalBytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                    }
                }
            }

            // Free Physical Memory from Win32_OperatingSystem (in KB)
            using (var searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    // TotalVisibleMemorySize daha doğru (KB cinsinden)
                    if (obj["TotalVisibleMemorySize"] != null && memoryInfo.TotalBytes == 0)
                    {
                        memoryInfo.TotalBytes = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
                    }
                    
                    if (obj["FreePhysicalMemory"] != null)
                    {
                        // FreePhysicalMemory KB cinsinden, byte'a çevir
                        memoryInfo.FreeBytes = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024;
                        memoryInfo.UsedBytes = memoryInfo.TotalBytes - memoryInfo.FreeBytes;
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MemoryInfo alma hatası: {ex.Message}");
        }

        return memoryInfo;
    }
}




