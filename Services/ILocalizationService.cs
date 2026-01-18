namespace PcPerformanceManager.Services;

public interface ILocalizationService
{
    /// <summary>
    /// Mevcut dili döndürür
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Dili değiştirir
    /// </summary>
    void ChangeLanguage(string languageCode);

    /// <summary>
    /// Belirtilen anahtar için lokalize edilmiş metni döndürür
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// Belirtilen anahtar için lokalize edilmiş metni döndürür (varsayılan değer ile)
    /// </summary>
    string GetString(string key, string defaultValue);
}
