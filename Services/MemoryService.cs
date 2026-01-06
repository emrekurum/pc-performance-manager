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
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    // Kritik sistem process'leri - bunlara dokunma
    private static readonly HashSet<string> CriticalProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Idle", "System", "smss", "csrss", "wininit", "winlogon", "services", "lsass",
        "svchost", "spoolsv", "explorer", "dwm", "conhost", "audiodg", "dllhost",
        "taskhost", "taskhostw", "sihost", "RuntimeBroker", "SearchIndexer",
        "SearchProtocolHost", "SearchFilterHost", "WmiPrvSE", "MsMpEng",
        "SecurityHealthService", "CompatTelRunner", "WUDFHost", "WmiApSrv",
        "fontdrvhost", "lsaiso", "Memory Compression", "Registry", "Secure System",
        "ShellExperienceHost", "StartMenuExperienceHost", "TextInputHost",
        "ctfmon", "SystemSettings", "ApplicationFrameHost", "LockApp"
    };

    // Aktif kullanılan uygulamalar - bunları koru (temizleme sırasında)
    private static readonly HashSet<string> ActiveAppProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cursor", "Code", "devenv", "idea64", "rider64", "webstorm64", // IDE'ler
        "chrome", "firefox", "msedge", "opera", "brave", // Tarayıcılar
        "WINWORD", "EXCEL", "POWERPNT", "OUTLOOK", "Teams", "slack", // Office & iletişim
        "Discord", "Telegram", "WhatsApp", // Mesajlaşma
        "Spotify", "vlc", "wmplayer", // Medya
        "steam", "EpicGamesLauncher", // Oyun platformları
        "PcPerformanceManager" // Kendimiz
    };

    public MemoryInfo GetMemoryInfo()
    {
        return SystemInfoHelper.GetMemoryInfo();
    }

    private int GetForegroundProcessId()
    {
        try
        {
            IntPtr hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint processId);
            return (int)processId;
        }
        catch
        {
            return 0;
        }
    }

    public List<ProcessMemoryInfo> GetProcessMemoryUsage()
    {
        var processList = new List<ProcessMemoryInfo>();

        try
        {
            var processes = Process.GetProcesses()
                .Where(p => !CriticalProcesses.Contains(p.ProcessName) && 
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
                int foregroundPid = GetForegroundProcessId();
                int currentPid = Process.GetCurrentProcess().Id;
                
                // Temizlenecek process'leri filtrele
                var processes = Process.GetProcesses()
                    .Where(p => 
                        !CriticalProcesses.Contains(p.ProcessName) && // Kritik değil
                        !ActiveAppProcesses.Contains(p.ProcessName) && // Aktif uygulama değil
                        p.Id != currentPid && // Kendi process'imiz değil
                        p.Id != foregroundPid && // Ön planda çalışan değil
                        p.WorkingSet64 > 10 * 1024 * 1024) // En az 10 MB
                    .ToList();

                int clearedCount = 0;
                foreach (var process in processes)
                {
                    try
                    {
                        // Process handle'a erişmeye çalış
                        IntPtr handle = process.Handle;
                        EmptyWorkingSet(handle);
                        clearedCount++;
                    }
                    catch
                    {
                        // Erişim reddedildiyse atla - bu normal
                    }
                }

                // GC koleksiyonu tetikle
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Debug.WriteLine($"RAM temizleme: {clearedCount} process temizlendi");
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
            var process = Process.GetProcessById(processId);
            if (process != null && process.Id != Process.GetCurrentProcess().Id)
            {
                try
                {
                    IntPtr handle = process.Handle;
                    EmptyWorkingSet(handle);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Process handle erişim hatası: {ex.Message}");
                    return false;
                }
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


