using System;
using System.Globalization;
using System.Windows;

namespace PcPerformanceManager.Services;

public class LocalizationService : ILocalizationService
{
    private ResourceDictionary? _currentResourceDictionary;
    private string _currentLanguage = "tr-TR";

    public string CurrentLanguage => _currentLanguage;

    public LocalizationService()
    {
        // Başlangıçta varsayılan dili yükle
        ChangeLanguage("tr-TR");
    }

    public void ChangeLanguage(string languageCode)
    {
        _currentLanguage = languageCode;

        // Mevcut resource dictionary'yi kaldır
        if (_currentResourceDictionary != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(_currentResourceDictionary);
        }

        // Yeni resource dictionary'yi yükle
        var resourceUri = languageCode switch
        {
            "en-US" => new Uri("/PcPerformanceManager;component/Resources/Localization.en-US.xaml", UriKind.Relative),
            "tr-TR" => new Uri("/PcPerformanceManager;component/Resources/Localization.tr-TR.xaml", UriKind.Relative),
            _ => new Uri("/PcPerformanceManager;component/Resources/Localization.tr-TR.xaml", UriKind.Relative)
        };

        try
        {
            _currentResourceDictionary = new ResourceDictionary { Source = resourceUri };
            Application.Current.Resources.MergedDictionaries.Add(_currentResourceDictionary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Resource dictionary yüklenemedi: {ex.Message}");
            // Hata durumunda varsayılan Türkçe'yi yükle
            if (languageCode != "tr-TR")
            {
                resourceUri = new Uri("/PcPerformanceManager;component/Resources/Localization.tr-TR.xaml", UriKind.Relative);
                _currentResourceDictionary = new ResourceDictionary { Source = resourceUri };
                Application.Current.Resources.MergedDictionaries.Add(_currentResourceDictionary);
            }
        }
    }

    public string GetString(string key)
    {
        return GetString(key, key);
    }

    public string GetString(string key, string defaultValue)
    {
        if (Application.Current.Resources.Contains(key))
        {
            var value = Application.Current.Resources[key];
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }
}
