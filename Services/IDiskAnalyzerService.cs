using System.Collections.Generic;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface IDiskAnalyzerService
{
    /// <summary>
    /// Sistemdeki tüm diskleri listeler
    /// </summary>
    Task<List<DiskInfo>> GetDisksAsync();
    
    /// <summary>
    /// Belirli bir disk sürücüsü için klasör boyutlarını analiz eder
    /// </summary>
    Task<List<FolderSize>> AnalyzeFolderSizesAsync(string driveLetter, int maxDepth = 3);
    
    /// <summary>
    /// Belirli bir klasörün boyutunu hesaplar
    /// </summary>
    Task<long> CalculateFolderSizeAsync(string folderPath);
    
    /// <summary>
    /// Belirli bir disk sürücüsünde büyük dosyaları bulur
    /// </summary>
    Task<List<LargeFile>> FindLargeFilesAsync(string driveLetter, long minSizeMB = 100);
    
    /// <summary>
    /// Belirli bir klasörde büyük dosyaları bulur
    /// </summary>
    Task<List<LargeFile>> FindLargeFilesInFolderAsync(string folderPath, long minSizeMB = 100);
}
