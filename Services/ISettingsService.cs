using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface ISettingsService
{
    /// <summary>
    /// Ayarları yükler (JSON dosyasından)
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// Ayarları kaydeder (JSON dosyasına)
    /// </summary>
    Task<bool> SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Varsayılan ayarları döndürür
    /// </summary>
    AppSettings GetDefaultSettings();

    /// <summary>
    /// Ayarlar dosyasının yolunu döndürür
    /// </summary>
    string GetSettingsFilePath();
}
