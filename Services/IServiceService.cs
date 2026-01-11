using System.Collections.Generic;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface IServiceService
{
    /// <summary>
    /// Tüm Windows servislerini listeler
    /// </summary>
    Task<List<WindowsService>> GetServicesAsync();
    
    /// <summary>
    /// Belirli bir servisi başlatır
    /// </summary>
    Task<bool> StartServiceAsync(WindowsService service);
    
    /// <summary>
    /// Belirli bir servisi durdurur
    /// </summary>
    Task<bool> StopServiceAsync(WindowsService service);
    
    /// <summary>
    /// Belirli bir servisin başlangıç tipini değiştirir
    /// </summary>
    Task<bool> ChangeStartupTypeAsync(WindowsService service, ServiceStartType startType);
    
    /// <summary>
    /// Servisin kritik olup olmadığını kontrol eder
    /// </summary>
    bool IsCriticalService(WindowsService service);
    
    /// <summary>
    /// Servisin güvenle durdurulup durdurulamayacağını kontrol eder
    /// </summary>
    bool IsSafeToStop(WindowsService service);
}
