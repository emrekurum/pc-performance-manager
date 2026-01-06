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

    // Kritik sistem process'leri - bunlara ASLA dokunma
    private static readonly HashSet<string> CriticalProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        // Windows Core
        "Idle", "System", "smss", "csrss", "wininit", "winlogon", "services", "lsass",
        "svchost", "spoolsv", "explorer", "dwm", "conhost", "dllhost",
        "taskhost", "taskhostw", "sihost", "RuntimeBroker",
        "fontdrvhost", "lsaiso", "Memory Compression", "Registry", "Secure System",
        "ShellExperienceHost", "StartMenuExperienceHost", "TextInputHost",
        "ctfmon", "SystemSettings", "ApplicationFrameHost", "LockApp",
        "SecurityHealthSystray", "UserOOBEBroker", "backgroundTaskHost",
        
        // Windows Services
        "WmiPrvSE", "WUDFHost", "WmiApSrv", "dasHost", "smartscreen",
        "SearchHost", "SearchIndexer", "SearchProtocolHost", "SearchFilterHost",
        
        // Security
        "MsMpEng", "SecurityHealthService", "NisSrv", "MpDefenderCoreService",
        
        // Hardware & Drivers
        "audiodg", "nvcontainer", "nvdisplay.container", "AMD External Events Utility",
        "igfxEM", "igfxHK", "igfxTray", "RtkAudUService64"
    };

    // Aktif kullanılan uygulamalar - bunları koru
    private static readonly HashSet<string> ActiveAppProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        // IDE'ler & Editörler
        "Cursor", "Code", "devenv", "idea64", "idea", "rider64", "webstorm64",
        "pycharm64", "phpstorm64", "goland64", "clion64", "datagrip64",
        "notepad++", "sublime_text", "atom", "brackets",
        
        // Tarayıcılar
        "chrome", "firefox", "msedge", "opera", "brave", "vivaldi", "safari",
        
        // Office & Productivity
        "WINWORD", "EXCEL", "POWERPNT", "OUTLOOK", "ONENOTE", "MSACCESS",
        "Teams", "slack", "Notion", "Obsidian", "Evernote",
        
        // İletişim
        "Discord", "Telegram", "WhatsApp", "Zoom", "Skype", "Signal",
        
        // Medya
        "Spotify", "vlc", "wmplayer", "iTunes", "foobar2000", "AIMP",
        "mpc-hc64", "PotPlayerMini64",
        
        // Oyun & Platformlar
        "steam", "EpicGamesLauncher", "GalaxyClient", "Origin", "Uplay",
        "Battle.net", "RiotClientServices", "LeagueClient",
        
        // Geliştirme Araçları
        "node", "python", "java", "dotnet", "git", "docker",
        "WindowsTerminal", "powershell", "cmd", "wt",
        
        // Veritabanları
        "sqlservr", "mysqld", "postgres", "mongod", "redis-server",
        
        // Bizim uygulama
        "PcPerformanceManager"
    };

    // Güvenle kapatılabilecek BLOATWARE ve gereksiz process'ler
    // Bu liste Windows 10/11'de yaygın olarak bulunan gereksiz uygulamaları içerir
    private static readonly Dictionary<string, (string DisplayName, string Category, string Description, ProcessRiskLevel Risk)> UnnecessaryProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        // === MICROSOFT BLOATWARE ===
        { "OneDrive", ("OneDrive", "Microsoft Bloatware", "Microsoft bulut depolama servisi - kullanmıyorsanız kapatılabilir", ProcessRiskLevel.Safe) },
        { "OneDriveSetup", ("OneDrive Setup", "Microsoft Bloatware", "OneDrive kurulum servisi", ProcessRiskLevel.Safe) },
        { "Microsoft.Photos", ("Fotoğraflar", "Microsoft Bloatware", "Windows Fotoğraflar uygulaması", ProcessRiskLevel.Safe) },
        { "Video.UI", ("Filmler ve TV", "Microsoft Bloatware", "Windows medya uygulaması", ProcessRiskLevel.Safe) },
        { "GameBar", ("Xbox Game Bar", "Microsoft Bloatware", "Xbox oyun çubuğu", ProcessRiskLevel.Safe) },
        { "GameBarPresenceWriter", ("Game Bar Writer", "Microsoft Bloatware", "Xbox Game Bar yardımcı servisi", ProcessRiskLevel.Safe) },
        { "XboxApp", ("Xbox App", "Microsoft Bloatware", "Xbox uygulaması", ProcessRiskLevel.Safe) },
        { "XboxGamingOverlay", ("Xbox Gaming Overlay", "Microsoft Bloatware", "Xbox oyun arayüzü", ProcessRiskLevel.Safe) },
        { "GamingServices", ("Gaming Services", "Microsoft Bloatware", "Xbox oyun servisleri", ProcessRiskLevel.Safe) },
        { "YourPhone", ("Telefonunuz", "Microsoft Bloatware", "Telefon bağlantı uygulaması", ProcessRiskLevel.Safe) },
        { "PhoneExperienceHost", ("Phone Experience", "Microsoft Bloatware", "Telefon deneyimi servisi", ProcessRiskLevel.Safe) },
        { "SkypeApp", ("Skype", "Microsoft Bloatware", "Skype uygulaması - kullanmıyorsanız", ProcessRiskLevel.Safe) },
        { "SkypeBackgroundHost", ("Skype Background", "Microsoft Bloatware", "Skype arka plan servisi", ProcessRiskLevel.Safe) },
        { "Cortana", ("Cortana", "Microsoft Bloatware", "Microsoft sesli asistan", ProcessRiskLevel.Safe) },
        { "SearchApp", ("Windows Search", "Microsoft Bloatware", "Windows arama uygulaması (arka plan)", ProcessRiskLevel.Low) },
        { "HxTsr", ("Mail/Calendar Sync", "Microsoft Bloatware", "Mail ve Takvim senkronizasyonu", ProcessRiskLevel.Safe) },
        { "HxOutlook", ("Mail App", "Microsoft Bloatware", "Windows Mail uygulaması", ProcessRiskLevel.Safe) },
        { "HxCalendarAppImm", ("Calendar App", "Microsoft Bloatware", "Windows Takvim uygulaması", ProcessRiskLevel.Safe) },
        { "People", ("Kişiler", "Microsoft Bloatware", "Windows Kişiler uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.People", ("Kişiler", "Microsoft Bloatware", "Windows Kişiler uygulaması", ProcessRiskLevel.Safe) },
        { "Calculator", ("Hesap Makinesi", "Microsoft Bloatware", "Kullanılmıyorsa kapatılabilir", ProcessRiskLevel.Safe) },
        { "WindowsCamera", ("Kamera", "Microsoft Bloatware", "Windows Kamera uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.549981C3F5F10", ("Cortana", "Microsoft Bloatware", "Cortana uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.WindowsMaps", ("Haritalar", "Microsoft Bloatware", "Windows Haritalar", ProcessRiskLevel.Safe) },
        { "Microsoft.MixedReality.Portal", ("Mixed Reality", "Microsoft Bloatware", "VR Portal uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.3DBuilder", ("3D Builder", "Microsoft Bloatware", "3D modelleme uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.Print3D", ("Print 3D", "Microsoft Bloatware", "3D baskı uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.Messaging", ("Mesajlaşma", "Microsoft Bloatware", "Windows mesajlaşma", ProcessRiskLevel.Safe) },
        { "Microsoft.OneConnect", ("OneConnect", "Microsoft Bloatware", "Mobil planlar", ProcessRiskLevel.Safe) },
        { "Microsoft.BingWeather", ("Hava Durumu", "Microsoft Bloatware", "Bing hava durumu", ProcessRiskLevel.Safe) },
        { "Microsoft.BingNews", ("Haberler", "Microsoft Bloatware", "Bing haberler", ProcessRiskLevel.Safe) },
        { "Microsoft.GetHelp", ("Yardım Al", "Microsoft Bloatware", "Microsoft yardım uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.Getstarted", ("İpuçları", "Microsoft Bloatware", "Windows ipuçları", ProcessRiskLevel.Safe) },
        { "Microsoft.WindowsFeedbackHub", ("Geri Bildirim", "Microsoft Bloatware", "Windows geri bildirim merkezi", ProcessRiskLevel.Safe) },
        { "Microsoft.ZuneMusic", ("Groove Müzik", "Microsoft Bloatware", "Groove müzik uygulaması", ProcessRiskLevel.Safe) },
        { "Microsoft.ZuneVideo", ("Filmler", "Microsoft Bloatware", "Film ve TV uygulaması", ProcessRiskLevel.Safe) },
        { "Clipchamp", ("Clipchamp", "Microsoft Bloatware", "Video düzenleme uygulaması", ProcessRiskLevel.Safe) },
        
        // === TELEMETRY & DIAGNOSTICS ===
        { "CompatTelRunner", ("Telemetry Runner", "Telemetri", "Windows uyumluluk telemetrisi - veri toplar", ProcessRiskLevel.Safe) },
        { "DiagTrack", ("Diagnostic Tracking", "Telemetri", "Windows tanılama izleme servisi", ProcessRiskLevel.Low) },
        { "DeviceCensus", ("Device Census", "Telemetri", "Cihaz bilgisi toplama servisi", ProcessRiskLevel.Safe) },
        { "wsqmcons", ("CEIP", "Telemetri", "Müşteri deneyimi iyileştirme programı", ProcessRiskLevel.Safe) },
        { "WerFault", ("Error Reporting", "Telemetri", "Windows hata raporlama", ProcessRiskLevel.Safe) },
        { "wermgr", ("Error Manager", "Telemetri", "Hata yöneticisi", ProcessRiskLevel.Safe) },
        { "TiWorker", ("Windows Update", "Sistem", "Windows Update işçisi - güncelleme yoksa", ProcessRiskLevel.Low) },
        { "MoUsoCoreWorker", ("Update Worker", "Sistem", "Windows Update çalışanı", ProcessRiskLevel.Low) },
        { "UsoClient", ("Update Client", "Sistem", "Windows Update istemcisi", ProcessRiskLevel.Low) },
        
        // === ÜÇÜNCÜ PARTI BLOATWARE ===
        // Adobe
        { "AdobeARM", ("Adobe Updater", "Adobe", "Adobe güncelleme yöneticisi", ProcessRiskLevel.Safe) },
        { "AdobeUpdateService", ("Adobe Update Service", "Adobe", "Adobe güncelleme servisi", ProcessRiskLevel.Safe) },
        { "CCXProcess", ("Creative Cloud", "Adobe", "Adobe Creative Cloud servisi", ProcessRiskLevel.Safe) },
        { "CCLibrary", ("CC Library", "Adobe", "Creative Cloud kütüphane servisi", ProcessRiskLevel.Safe) },
        { "AdobeIPCBroker", ("Adobe IPC", "Adobe", "Adobe iletişim servisi", ProcessRiskLevel.Safe) },
        { "armsvc", ("Adobe ARM Service", "Adobe", "Adobe ARM servisi", ProcessRiskLevel.Safe) },
        { "AGSService", ("Adobe Genuine", "Adobe", "Adobe orijinallik kontrolü", ProcessRiskLevel.Safe) },
        
        // Apple
        { "iTunesHelper", ("iTunes Helper", "Apple", "iTunes yardımcı servisi", ProcessRiskLevel.Safe) },
        { "AppleMobileDeviceService", ("Apple Mobile", "Apple", "Apple cihaz servisi", ProcessRiskLevel.Safe) },
        { "mDNSResponder", ("Bonjour", "Apple", "Apple Bonjour servisi", ProcessRiskLevel.Safe) },
        { "APSDaemon", ("Apple Push", "Apple", "Apple push bildirimleri", ProcessRiskLevel.Safe) },
        { "iCloudServices", ("iCloud", "Apple", "iCloud senkronizasyon servisi", ProcessRiskLevel.Safe) },
        { "iCloudDrive", ("iCloud Drive", "Apple", "iCloud Drive servisi", ProcessRiskLevel.Safe) },
        { "iCloudPhotos", ("iCloud Photos", "Apple", "iCloud Fotoğraflar servisi", ProcessRiskLevel.Safe) },
        
        // Google
        { "GoogleUpdate", ("Google Update", "Google", "Google güncelleme servisi", ProcessRiskLevel.Safe) },
        { "GoogleCrashHandler", ("Google Crash", "Google", "Google çökme raporlama", ProcessRiskLevel.Safe) },
        { "GoogleCrashHandler64", ("Google Crash 64", "Google", "Google çökme raporlama (64-bit)", ProcessRiskLevel.Safe) },
        { "GoogleDriveFS", ("Google Drive", "Google", "Google Drive dosya senkronizasyonu", ProcessRiskLevel.Safe) },
        
        // Spotify
        { "SpotifyWebHelper", ("Spotify Web", "Spotify", "Spotify web yardımcısı", ProcessRiskLevel.Safe) },
        { "SpotifyMigrator", ("Spotify Migrator", "Spotify", "Spotify veri taşıma", ProcessRiskLevel.Safe) },
        
        // Diğer yaygın bloatware
        { "jusched", ("Java Updater", "Java", "Java güncelleme kontrolü", ProcessRiskLevel.Safe) },
        { "jucheck", ("Java Check", "Java", "Java güncelleme kontrolü", ProcessRiskLevel.Safe) },
        { "DropboxUpdate", ("Dropbox Update", "Dropbox", "Dropbox güncelleme servisi", ProcessRiskLevel.Safe) },
        { "Dropbox", ("Dropbox", "Dropbox", "Dropbox senkronizasyon (kullanmıyorsanız)", ProcessRiskLevel.Safe) },
        { "CCleanerBrowser", ("CCleaner Browser", "CCleaner", "CCleaner tarayıcısı", ProcessRiskLevel.Safe) },
        { "ccleaner", ("CCleaner", "CCleaner", "CCleaner arka plan servisi", ProcessRiskLevel.Safe) },
        { "ccleaner64", ("CCleaner 64", "CCleaner", "CCleaner 64-bit", ProcessRiskLevel.Safe) },
        { "NVDisplay.Container", ("NVIDIA Container", "NVIDIA", "NVIDIA görüntü container'ı", ProcessRiskLevel.Low) },
        { "NVIDIA Web Helper", ("NVIDIA Web", "NVIDIA", "NVIDIA web yardımcısı", ProcessRiskLevel.Safe) },
        { "NvTmru", ("NVIDIA Telemetry", "NVIDIA", "NVIDIA telemetri", ProcessRiskLevel.Safe) },
        { "NvNetworkService", ("NVIDIA Network", "NVIDIA", "NVIDIA ağ servisi", ProcessRiskLevel.Safe) },
        { "NvBackend", ("NVIDIA Backend", "NVIDIA", "NVIDIA GeForce Experience backend", ProcessRiskLevel.Safe) },
        { "nvstreamsvc", ("NVIDIA Streaming", "NVIDIA", "NVIDIA streaming servisi", ProcessRiskLevel.Safe) },
        
        // Üreticiler (Dell, HP, Lenovo, ASUS vb.)
        { "DellDataVault", ("Dell Data Vault", "Dell", "Dell veri toplama servisi", ProcessRiskLevel.Safe) },
        { "DellSupportAssistAgent", ("Dell Support", "Dell", "Dell destek asistanı", ProcessRiskLevel.Safe) },
        { "SupportAssistAgent", ("Dell Support Agent", "Dell", "Dell destek ajanı", ProcessRiskLevel.Safe) },
        { "HPSupportSolutionsFrameworkService", ("HP Support", "HP", "HP destek servisi", ProcessRiskLevel.Safe) },
        { "HPJumpStartBridge", ("HP JumpStart", "HP", "HP başlangıç servisi", ProcessRiskLevel.Safe) },
        { "HPAudioSwitch", ("HP Audio", "HP", "HP ses servisi", ProcessRiskLevel.Low) },
        { "LenovoVantageService", ("Lenovo Vantage", "Lenovo", "Lenovo Vantage servisi", ProcessRiskLevel.Safe) },
        { "ImControllerService", ("Lenovo Controller", "Lenovo", "Lenovo kontrol servisi", ProcessRiskLevel.Safe) },
        { "ASUSOptimization", ("ASUS Optimization", "ASUS", "ASUS optimizasyon servisi", ProcessRiskLevel.Safe) },
        { "ArmouryCrateService", ("Armoury Crate", "ASUS", "ASUS Armoury Crate servisi", ProcessRiskLevel.Safe) },
        { "AsusCertService", ("ASUS Cert", "ASUS", "ASUS sertifika servisi", ProcessRiskLevel.Safe) },
        
        // Diğer
        { "RazerCentralService", ("Razer Central", "Razer", "Razer merkezi servis", ProcessRiskLevel.Safe) },
        { "RzSynapse3Service", ("Razer Synapse", "Razer", "Razer Synapse servisi", ProcessRiskLevel.Safe) },
        { "LogiRegistryService", ("Logitech Registry", "Logitech", "Logitech kayıt servisi", ProcessRiskLevel.Safe) },
        { "LogiOptions", ("Logitech Options", "Logitech", "Logitech Options servisi", ProcessRiskLevel.Safe) },
        { "GHub", ("Logitech G Hub", "Logitech", "Logitech G Hub", ProcessRiskLevel.Safe) },
        { "WinZipper", ("WinZipper", "Bloatware", "Potansiyel istenmeyen program", ProcessRiskLevel.Safe) },
        { "Driver Booster", ("Driver Booster", "IObit", "Sürücü güncelleme programı", ProcessRiskLevel.Safe) },
        { "Advanced SystemCare", ("SystemCare", "IObit", "Sistem bakım programı", ProcessRiskLevel.Safe) },
        { "McAfee", ("McAfee", "McAfee", "McAfee antivirüs (Windows Defender yeterli)", ProcessRiskLevel.Low) },
        { "Norton", ("Norton", "Norton", "Norton antivirüs (Windows Defender yeterli)", ProcessRiskLevel.Low) },
        { "avastui", ("Avast", "Avast", "Avast antivirüs arayüzü", ProcessRiskLevel.Low) },
        { "avgui", ("AVG", "AVG", "AVG antivirüs arayüzü", ProcessRiskLevel.Low) }
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

    /// <summary>
    /// Sistemdeki gereksiz/bloatware process'leri analiz eder
    /// </summary>
    public List<CleanableProcess> AnalyzeUnnecessaryProcesses()
    {
        var result = new List<CleanableProcess>();
        
        try
        {
            var runningProcesses = Process.GetProcesses();
            
            foreach (var process in runningProcesses)
            {
                try
                {
                    string processName = process.ProcessName;
                    
                    // Bilinen gereksiz process mi kontrol et
                    if (UnnecessaryProcesses.TryGetValue(processName, out var info))
                    {
                        result.Add(new CleanableProcess
                        {
                            ProcessId = process.Id,
                            ProcessName = processName,
                            DisplayName = info.DisplayName,
                            Category = info.Category,
                            Description = info.Description,
                            RiskLevel = info.Risk,
                            MemoryMB = process.WorkingSet64 / (1024.0 * 1024.0),
                            IsRunning = true,
                            IsSelected = info.Risk == ProcessRiskLevel.Safe // Güvenli olanları otomatik seç
                        });
                    }
                }
                catch
                {
                    // Process erişilemiyorsa atla
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Process analiz hatası: {ex.Message}");
        }

        return result.OrderByDescending(p => p.MemoryMB).ToList();
    }

    /// <summary>
    /// Seçilen process'leri sonlandırır (kapatır)
    /// </summary>
    public async Task<(int terminated, int failed, double freedMB)> TerminateProcessesAsync(IEnumerable<CleanableProcess> processes)
    {
        return await Task.Run(() =>
        {
            int terminated = 0;
            int failed = 0;
            double freedMB = 0;

            foreach (var proc in processes.Where(p => p.IsSelected && p.IsRunning))
            {
                try
                {
                    var process = Process.GetProcessById(proc.ProcessId);
                    if (process != null && !process.HasExited)
                    {
                        double memoryBefore = process.WorkingSet64 / (1024.0 * 1024.0);
                        
                        // Process'i nazikçe kapat
                        if (process.CloseMainWindow())
                        {
                            // 3 saniye bekle
                            if (!process.WaitForExit(3000))
                            {
                                // Hala kapanmadıysa zorla kapat
                                process.Kill();
                            }
                        }
                        else
                        {
                            // Ana penceresi yoksa direkt kapat
                            process.Kill();
                        }
                        
                        process.WaitForExit(2000);
                        
                        freedMB += memoryBefore;
                        terminated++;
                        proc.IsRunning = false;
                        
                        Debug.WriteLine($"Process sonlandırıldı: {proc.ProcessName} ({memoryBefore:F2} MB)");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Debug.WriteLine($"Process sonlandırma hatası ({proc.ProcessName}): {ex.Message}");
                }
            }

            return (terminated, failed, freedMB);
        });
    }

    /// <summary>
    /// Tüm güvenli process'leri otomatik temizler
    /// </summary>
    public async Task<(int terminated, double freedMB)> AutoCleanSafeProcessesAsync()
    {
        var unnecessaryProcesses = AnalyzeUnnecessaryProcesses();
        var safeProcesses = unnecessaryProcesses.Where(p => p.RiskLevel == ProcessRiskLevel.Safe).ToList();
        
        // Hepsini seçili yap
        foreach (var p in safeProcesses)
        {
            p.IsSelected = true;
        }

        var result = await TerminateProcessesAsync(safeProcesses);
        return (result.terminated, result.freedMB);
    }
}


