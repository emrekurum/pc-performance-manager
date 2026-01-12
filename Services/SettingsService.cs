using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "PcPerformanceManager");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _settingsFilePath = Path.Combine(appFolder, "settings.json");
    }

    public string GetSettingsFilePath()
    {
        return _settingsFilePath;
    }

    public AppSettings GetDefaultSettings()
    {
        return new AppSettings();
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return GetDefaultSettings();
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                return GetDefaultSettings();
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return settings ?? GetDefaultSettings();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings yüklenirken hata: {ex.Message}");
            return GetDefaultSettings();
        }
    }

    public async Task<bool> SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings kaydedilirken hata: {ex.Message}");
            return false;
        }
    }
}
