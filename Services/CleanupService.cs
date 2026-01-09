using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class CleanupService : ICleanupService
{
    public async Task<List<CleanupItem>> AnalyzeCleanupItemsAsync()
    {
        return await Task.Run(() =>
        {
            var items = new List<CleanupItem>();

            try
            {
                // Geçici dosyalar
                items.AddRange(AnalyzeTemporaryFiles());
                
                // Windows geçici klasörü
                items.AddRange(AnalyzeWindowsTempFolder());
                
                // Kullanıcı geçici klasörü
                items.AddRange(AnalyzeUserTempFolder());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Temizleme analizi hatası: {ex.Message}");
            }

            return items;
        });
    }

    public async Task<CleanupResult> CleanupAsync(List<CleanupItem> itemsToClean)
    {
        return await Task.Run(() =>
        {
            var result = new CleanupResult();
            long totalBytesCleaned = 0;
            int itemsCleaned = 0;

            try
            {
                var selectedItems = itemsToClean.Where(i => i.IsSelected).ToList();

                foreach (var item in selectedItems)
                {
                    try
                    {
                        if (Directory.Exists(item.Path))
                        {
                            var directoryInfo = new DirectoryInfo(item.Path);
                            var size = GetDirectorySize(directoryInfo);
                            
                            Directory.Delete(item.Path, true);
                            totalBytesCleaned += size;
                            itemsCleaned++;
                        }
                        else if (File.Exists(item.Path))
                        {
                            var fileInfo = new FileInfo(item.Path);
                            totalBytesCleaned += fileInfo.Length;
                            File.Delete(item.Path);
                            itemsCleaned++;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Temizleme hatası ({item.Path}): {ex.Message}");
                    }
                }

                result.Success = true;
                result.BytesCleaned = totalBytesCleaned;
                result.ItemsCleaned = itemsCleaned;
                result.Message = $"{itemsCleaned} öğe temizlendi. {result.GBCleaned:F2} GB alan kazanıldı.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Temizleme sırasında hata oluştu: {ex.Message}";
            }

            return result;
        });
    }

    public async Task<List<CleanupItem>> AnalyzeCategoryAsync(CleanupCategory category)
    {
        return await Task.Run(() =>
        {
            return category switch
            {
                CleanupCategory.TemporaryFiles => AnalyzeTemporaryFiles(),
                CleanupCategory.CacheFiles => AnalyzeCacheFiles(),
                _ => new List<CleanupItem>()
            };
        });
    }

    private List<CleanupItem> AnalyzeTemporaryFiles()
    {
        var items = new List<CleanupItem>();
        var tempPath = Path.GetTempPath();

        try
        {
            if (Directory.Exists(tempPath))
            {
                var dirInfo = new DirectoryInfo(tempPath);
                var size = GetDirectorySize(dirInfo);

                items.Add(new CleanupItem
                {
                    Name = "Geçici Dosyalar",
                    Description = "Windows geçici dosya klasörü",
                    Path = tempPath,
                    SizeBytes = size,
                    Category = CleanupCategory.TemporaryFiles,
                    IsSelected = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geçici dosya analizi hatası: {ex.Message}");
        }

        return items;
    }

    private List<CleanupItem> AnalyzeWindowsTempFolder()
    {
        var items = new List<CleanupItem>();
        var windowsTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

        try
        {
            if (Directory.Exists(windowsTempPath))
            {
                var dirInfo = new DirectoryInfo(windowsTempPath);
                var size = GetDirectorySize(dirInfo);

                items.Add(new CleanupItem
                {
                    Name = "Windows Geçici Klasörü",
                    Description = "Windows sistem geçici dosyaları",
                    Path = windowsTempPath,
                    SizeBytes = size,
                    Category = CleanupCategory.TemporaryFiles,
                    IsSelected = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Windows temp analizi hatası: {ex.Message}");
        }

        return items;
    }

    private List<CleanupItem> AnalyzeUserTempFolder()
    {
        var items = new List<CleanupItem>();
        var userTempPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp";

        try
        {
            if (Directory.Exists(userTempPath))
            {
                var dirInfo = new DirectoryInfo(userTempPath);
                var size = GetDirectorySize(dirInfo);

                items.Add(new CleanupItem
                {
                    Name = "Kullanıcı Geçici Klasörü",
                    Description = "Kullanıcı profil geçici dosyaları",
                    Path = userTempPath,
                    SizeBytes = size,
                    Category = CleanupCategory.TemporaryFiles,
                    IsSelected = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Kullanıcı temp analizi hatası: {ex.Message}");
        }

        return items;
    }

    private List<CleanupItem> AnalyzeCacheFiles()
    {
        var items = new List<CleanupItem>();
        // Cache dosyaları için ek analizler eklenebilir
        return items;
    }

    private long GetDirectorySize(DirectoryInfo directoryInfo)
    {
        long size = 0;

        try
        {
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                try
                {
                    size += file.Length;
                }
                catch
                {
                    // Dosya erişilemiyorsa atla
                }
            }
        }
        catch
        {
            // Klasör erişilemiyorsa atla
        }

        return size;
    }
}






