using System.Collections.Generic;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface ICleanupService
{
    /// <summary>
    /// Temizlenebilecek dosya ve klasörleri analiz eder
    /// </summary>
    Task<List<CleanupItem>> AnalyzeCleanupItemsAsync();

    /// <summary>
    /// Seçili temizleme öğelerini temizler
    /// </summary>
    Task<CleanupResult> CleanupAsync(List<CleanupItem> itemsToClean);

    /// <summary>
    /// Belirli bir kategoriye ait temizleme öğelerini analiz eder
    /// </summary>
    Task<List<CleanupItem>> AnalyzeCategoryAsync(CleanupCategory category);
}

public class CleanupResult
{
    public bool Success { get; set; }
    public long BytesCleaned { get; set; }
    public int ItemsCleaned { get; set; }
    public string Message { get; set; } = string.Empty;

    public double GBCleaned => BytesCleaned / (1024.0 * 1024.0 * 1024.0);
}





