using System.Collections.Generic;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface IMemoryService
{
    /// <summary>
    /// Anlık RAM kullanım bilgilerini döndürür
    /// </summary>
    MemoryInfo GetMemoryInfo();

    /// <summary>
    /// Çalışan tüm süreçlerin RAM kullanım bilgilerini döndürür
    /// </summary>
    List<ProcessMemoryInfo> GetProcessMemoryUsage();

    /// <summary>
    /// RAM temizleme işlemini başlatır (Working Set temizleme)
    /// </summary>
    Task<bool> ClearMemoryAsync();

    /// <summary>
    /// Belirli bir sürecin RAM kullanımını temizler
    /// </summary>
    bool ClearProcessMemory(int processId);

    /// <summary>
    /// Sistemdeki gereksiz/bloatware process'leri analiz eder
    /// </summary>
    List<CleanableProcess> AnalyzeUnnecessaryProcesses();

    /// <summary>
    /// Seçilen process'leri sonlandırır (kapatır)
    /// </summary>
    Task<(int terminated, int failed, double freedMB)> TerminateProcessesAsync(IEnumerable<CleanableProcess> processes);

    /// <summary>
    /// Tüm güvenli process'leri otomatik temizler
    /// </summary>
    Task<(int terminated, double freedMB)> AutoCleanSafeProcessesAsync();
}

