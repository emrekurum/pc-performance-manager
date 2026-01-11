using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class ServiceService : IServiceService
{
    // Kritik sistem servisleri - ASLA durdurulmamalı
    private static readonly HashSet<string> CriticalServices = new(StringComparer.OrdinalIgnoreCase)
    {
        // Windows Core Security
        "WinDefend", "SecurityHealthService", "WdNisSvc", "MpDefenderCoreService",
        "wuauserv", "UsoSvc", "MoUsoCoreWorker", "TrustedInstaller",
        
        // Windows Audio & Display
        "Audiosrv", "AudioEndpointBuilder", "UxSms", "DispBrokerDesktopSvc",
        
        // Windows Firewall & Network
        "MpsSvc", "Dhcp", "Dnscache", "Netman", "NlaSvc", "Netlogon",
        
        // Windows Essential Services
        "PlugPlay", "Power", "EventLog", "RpcSs", "Themes", "DcomLaunch",
        "Winmgmt", "CryptSvc", "KeyIso", "SamSs", "Lsa", "BITS",
        
        // Windows Search & Indexing
        "WSearch", "SearchIndexer",
        
        // Windows Time
        "W32Time",
        
        // Windows Update & Error Reporting
        "WerSvc", "Wecsvc",
        
        // Storage & Volume
        "VSS", "VDS",
        
        // Print Spooler
        "Spooler",
        
        // Desktop Window Manager
        "DPS",
        
        // User Profile Service
        "ProfSvc",
        
        // Task Scheduler
        "Schedule",
        
        // System Events
        "EventSystem"
    };

    // Güvenle durdurulabilecek servisler
    private static readonly Dictionary<string, (string Category, string Description)> SafeToStopServices = new(StringComparer.OrdinalIgnoreCase)
    {
        // Microsoft Bloatware Services
        { "OneSyncSvc", ("Microsoft Bloatware", "OneDrive senkronizasyon servisi") },
        { "OneDrive Updater Service", ("Microsoft Bloatware", "OneDrive güncelleme servisi") },
        { "WMPNetworkSvc", ("Microsoft Bloatware", "Windows Media Player Network Sharing") },
        { "XblAuthManager", ("Microsoft Bloatware", "Xbox Live Auth Manager") },
        { "XblGameSave", ("Microsoft Bloatware", "Xbox Live Game Save") },
        { "XboxGipSvc", ("Microsoft Bloatware", "Xbox Accessory Management") },
        { "XboxNetApiSvc", ("Microsoft Bloatware", "Xbox Live Networking Service") },
        { "WpcMonSvc", ("Microsoft Bloatware", "Parental Controls") },
        { "PhoneSvc", ("Microsoft Bloatware", "Phone Service") },
        { "PimIndexMaintenanceSvc", ("Microsoft Bloatware", "Contact Data Service") },
        { "UnistoreSvc", ("Microsoft Bloatware", "User Data Access Service") },
        { "UserDataSvc", ("Microsoft Bloatware", "User Data Access Service") },
        { "WpnUserService", ("Microsoft Bloatware", "Windows Push Notifications User Service") },
        
        // Telemetry Services
        { "DiagTrack", ("Telemetri", "Connected User Experiences and Telemetry") },
        { "dmwappushservice", ("Telemetri", "WAP Push Message Routing Service") },
        
        // Optional Services
        { "RemoteRegistry", ("İsteğe Bağlı", "Remote Registry - genellikle gerekli değil") },
        { "SSDPSRV", ("İsteğe Bağlı", "SSDP Discovery - ağ yoksa kapatılabilir") },
        { "upnphost", ("İsteğe Bağlı", "UPnP Device Host - ağ yoksa kapatılabilir") },
        { "WbioSrvc", ("İsteğe Bağlı", "Windows Biometric Service - biyometrik cihaz yoksa") },
        { "TabletInputService", ("İsteğe Bağlı", "Touch Keyboard and Handwriting Panel Service") },
        { "TrkWks", ("İsteğe Bağlı", "Distributed Link Tracking Client") },
        
        // Printer Services (yazıcı yoksa)
        { "Spooler", ("Yazıcı", "Print Spooler - yazıcı yoksa kapatılabilir (DİKKAT: Yazıcılar çalışmaz)") },
        
        // Fax Service
        { "Fax", ("Faks", "Fax Service - faks kullanmıyorsanız") },
        
        // Bluetooth (bluetooth yoksa)
        { "bthserv", ("Bluetooth", "Bluetooth Support Service - bluetooth yoksa") },
        
        // Windows Search (arama kullanmıyorsanız)
        { "WSearch", ("Arama", "Windows Search - arama kullanmıyorsanız (performans artışı sağlar)") }
    };

    public async Task<List<WindowsService>> GetServicesAsync()
    {
        return await Task.Run(() =>
        {
            var services = new List<WindowsService>();
            
            try
            {
                var serviceControllers = ServiceController.GetServices();
                
                // WMI ile başlangıç tiplerini al
                var startTypes = GetServiceStartTypes();
                
                foreach (var sc in serviceControllers)
                {
                    try
                    {
                        var serviceStatus = sc.Status.ToString() switch
                        {
                            "Running" => ServiceStatus.Running,
                            "Stopped" => ServiceStatus.Stopped,
                            "Paused" => ServiceStatus.Paused,
                            "StartPending" => ServiceStatus.Starting,
                            "StopPending" => ServiceStatus.Stopping,
                            _ => ServiceStatus.Unknown
                        };
                        
                        var service = new WindowsService
                        {
                            Name = sc.ServiceName,
                            DisplayName = string.IsNullOrWhiteSpace(sc.DisplayName) ? sc.ServiceName : sc.DisplayName,
                            Status = serviceStatus,
                            StartType = startTypes.ContainsKey(sc.ServiceName) 
                                ? WindowsService.ConvertStartType(startTypes[sc.ServiceName])
                                : ServiceStartType.Unknown
                        };
                        
                        // Açıklama ve kategori bilgilerini al
                        if (SafeToStopServices.ContainsKey(service.Name))
                        {
                            service.Category = SafeToStopServices[service.Name].Category;
                            service.Description = SafeToStopServices[service.Name].Description;
                            service.IsSafeToStop = true;
                        }
                        else
                        {
                            service.Category = "Sistem";
                            service.Description = string.IsNullOrWhiteSpace(sc.DisplayName) ? sc.ServiceName : sc.DisplayName;
                            service.IsSafeToStop = !IsCriticalService(service);
                        }
                        
                        service.IsCritical = IsCriticalService(service);
                        
                        services.Add(service);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Servis {sc.ServiceName} yüklenirken hata: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Servisler yüklenirken hata: {ex.Message}");
            }
            
            return services.OrderBy(s => s.DisplayName).ToList();
        });
    }

    public async Task<bool> StartServiceAsync(WindowsService service)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var sc = new ServiceController(service.Name);
                sc.Refresh();
                var statusStr = sc.Status.ToString();
                if (statusStr == "Stopped")
                {
                    sc.Start();
                    // Wait for service to start (polling)
                    for (int i = 0; i < 30; i++)
                    {
                        System.Threading.Thread.Sleep(1000);
                        sc.Refresh();
                        if (sc.Status.ToString() == "Running")
                            return true;
                    }
                    return sc.Status.ToString() == "Running";
                }
                return statusStr == "Running";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Servis {service.Name} başlatılırken hata: {ex.Message}");
                return false;
            }
        });
    }

    public async Task<bool> StopServiceAsync(WindowsService service)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var sc = new ServiceController(service.Name);
                sc.Refresh();
                var statusStr = sc.Status.ToString();
                if (statusStr == "Running")
                {
                    sc.Stop();
                    // Wait for service to stop (polling)
                    for (int i = 0; i < 30; i++)
                    {
                        System.Threading.Thread.Sleep(1000);
                        sc.Refresh();
                        if (sc.Status.ToString() == "Stopped")
                            return true;
                    }
                    return sc.Status.ToString() == "Stopped";
                }
                return statusStr == "Stopped";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Servis {service.Name} durdurulurken hata: {ex.Message}");
                return false;
            }
        });
    }

    public async Task<bool> ChangeStartupTypeAsync(WindowsService service, ServiceStartType startType)
    {
        return await Task.Run(() =>
        {
            try
            {
                var startTypeValue = startType switch
                {
                    ServiceStartType.Automatic => "automatic",
                    ServiceStartType.AutomaticDelayed => "automatic-delayed",
                    ServiceStartType.Manual => "demand",
                    ServiceStartType.Disabled => "disabled",
                    _ => "demand"
                };
                
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"config {service.Name} start= {startTypeValue}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Servis {service.Name} başlangıç tipi değiştirilirken hata: {ex.Message}");
                return false;
            }
        });
    }

    public bool IsCriticalService(WindowsService service)
    {
        return CriticalServices.Contains(service.Name);
    }

    public bool IsSafeToStop(WindowsService service)
    {
        return service.IsSafeToStop && !IsCriticalService(service);
    }

    private Dictionary<string, string> GetServiceStartTypes()
    {
        var startTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, StartMode FROM Win32_Service");
            foreach (ManagementObject service in searcher.Get())
            {
                var name = service["Name"]?.ToString();
                var startMode = service["StartMode"]?.ToString();
                if (name != null && startMode != null)
                {
                    startTypes[name] = startMode;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WMI ile başlangıç tipleri alınırken hata: {ex.Message}");
        }
        
        return startTypes;
    }
}
