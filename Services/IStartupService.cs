using System.Collections.Generic;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface IStartupService
{
    /// <summary>
    /// Tüm startup programlarını analiz eder ve listeler
    /// </summary>
    Task<List<StartupProgram>> GetStartupProgramsAsync();
    
    /// <summary>
    /// Belirli bir startup programını etkinleştirir
    /// </summary>
    Task<bool> EnableStartupAsync(StartupProgram program);
    
    /// <summary>
    /// Belirli bir startup programını devre dışı bırakır
    /// </summary>
    Task<bool> DisableStartupAsync(StartupProgram program);
    
    /// <summary>
    /// Startup programının etkisini analiz eder (başlangıç hızına göre)
    /// </summary>
    StartupImpact AnalyzeStartupImpact(StartupProgram program);
    
    /// <summary>
    /// Tüm startup programlarının toplam başlangıç süresini tahmin eder
    /// </summary>
    Task<double> EstimateTotalStartupTimeAsync(List<StartupProgram> programs);
}

