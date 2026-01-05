using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PcPerformanceManager.Helpers;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class MemoryService : IMemoryService
{
    [DllImport("kernel32.dll")]
    private static extern int EmptyWorkingSet(IntPtr hProcess);

    public MemoryInfo GetMemoryInfo()
    {
        return SystemInfoHelper.GetMemoryInfo();
    }

    public List<ProcessMemoryInfo> GetProcessMemoryUsage()
    {
        var processList = new List<ProcessMemoryInfo>();

        // Kritik sistem process'leri - bunları gösterme
        var criticalProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Idle",
            "System",
            "smss",           // Session Manager
            "csrss",          // Client Server Runtime Process
            "wininit",        // Windows Initialization
            "winlogon",       // Windows Logon
            "services",       // Services and Controller
            "lsass",          // Local Security Authority
            "svchost",        // Service Host
            "spoolsv",        // Print Spooler
            "explorer",       // Windows Explorer
            "dwm",            // Desktop Window Manager
            "conhost",        // Console Window Host
            "audiodg",        // Windows Audio Device Graph
            "dllhost",        // COM Surrogate
            "taskhost",       // Task Host
            "taskhostw",      // Task Host Window
            "sihost",         // Shell Infrastructure Host
            "RuntimeBroker",  // Runtime Broker
            "SearchIndexer",  // Windows Search Indexer
            "SearchProtocolHost", // Search Protocol Host
            "SearchFilterHost",  // Search Filter Host
            "WmiPrvSE",       // WMI Provider Host
            "MsMpEng",        // Windows Defender
            "SecurityHealthService", // Windows Security Health Service
            "CompatTelRunner", // Compatibility Telemetry
            "WUDFHost",       // Windows Driver Foundation
            "WmiApSrv"        // WMI Performance Adapter
        };

        try
        {
            var processes = Process.GetProcesses()
                .Where(p => !criticalProcesses.Contains(p.ProcessName) && 
                           p.WorkingSet64 > 50 * 1024 * 1024) // En az 50 MB kullanan process'ler
                .OrderByDescending(p => p.WorkingSet64);

            foreach (var process in processes)
            {
                try
                {
                    processList.Add(new ProcessMemoryInfo
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        WorkingSetBytes = process.WorkingSet64
                    });
                }
                catch
                {
                    // Süreç erişilemiyorsa atla
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Process memory bilgisi alma hatası: {ex.Message}");
        }

        return processList;
    }

    public async Task<bool> ClearMemoryAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!AdminHelper.IsRunningAsAdministrator())
                {
                    return false;
                }

                // Çalışan tüm süreçlerin working set'ini temizle
                var processes = Process.GetProcesses()
                    .Where(p => p.ProcessName != "Idle" && 
                                p.ProcessName != "System" && 
                                p.Id != Process.GetCurrentProcess().Id);

                int clearedCount = 0;
                foreach (var process in processes)
                {
                    try
                    {
                        if (EmptyWorkingSet(process.Handle) != 0)
                        {
                            clearedCount++;
                        }
                    }
                    catch
                    {
                        // Süreç erişilemiyorsa atla
                    }
                }

                // GC koleksiyonu tetikle
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                return clearedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RAM temizleme hatası: {ex.Message}");
                return false;
            }
        });
    }

    public bool ClearProcessMemory(int processId)
    {
        try
        {
            if (!AdminHelper.IsRunningAsAdministrator())
            {
                return false;
            }

            var process = Process.GetProcessById(processId);
            if (process != null && process.Id != Process.GetCurrentProcess().Id)
            {
                return EmptyWorkingSet(process.Handle) != 0;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Süreç RAM temizleme hatası: {ex.Message}");
            return false;
        }
    }
}


