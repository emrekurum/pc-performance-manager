using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class DiskAnalyzerService : IDiskAnalyzerService
{
    public async Task<List<DiskInfo>> GetDisksAsync()
    {
        return await Task.Run(() =>
        {
            var disks = new List<DiskInfo>();
            
            try
            {
                var drives = DriveInfo.GetDrives();
                
                foreach (var drive in drives)
                {
                    try
                    {
                        if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                        {
                            var diskInfo = new DiskInfo
                            {
                                DriveLetter = drive.Name.Substring(0, 2),
                                Label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Yerel Disk" : drive.VolumeLabel,
                                FileSystem = drive.DriveFormat,
                                TotalBytes = drive.TotalSize,
                                FreeBytes = drive.AvailableFreeSpace
                            };
                            
                            disks.Add(diskInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Disk {drive.Name} bilgisi alınırken hata: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Diskler alınırken hata: {ex.Message}");
            }
            
            return disks;
        });
    }

    public async Task<List<FolderSize>> AnalyzeFolderSizesAsync(string driveLetter, int maxDepth = 3)
    {
        return await Task.Run(() =>
        {
            var folderSizes = new List<FolderSize>();
            var rootPath = driveLetter;
            
            if (!Directory.Exists(rootPath))
                return folderSizes;
            
            try
            {
                AnalyzeFolderRecursive(rootPath, folderSizes, 0, maxDepth);
                return folderSizes.OrderByDescending(f => f.SizeBytes).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Klasör boyutları analiz edilirken hata: {ex.Message}");
                return folderSizes;
            }
        });
    }

    public async Task<long> CalculateFolderSizeAsync(string folderPath)
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(folderPath))
                return 0;
            
            long totalSize = 0;
            
            try
            {
                var directoryInfo = new DirectoryInfo(folderPath);
                var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                
                foreach (var file in files)
                {
                    try
                    {
                        totalSize += file.Length;
                    }
                    catch
                    {
                        // Skip inaccessible files
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Klasör boyutu hesaplanırken hata: {ex.Message}");
            }
            
            return totalSize;
        });
    }

    public async Task<List<LargeFile>> FindLargeFilesAsync(string driveLetter, long minSizeMB = 100)
    {
        return await Task.Run(() =>
        {
            var largeFiles = new List<LargeFile>();
            var minSizeBytes = minSizeMB * 1024 * 1024;
            var rootPath = driveLetter.TrimEnd('\\');
            
            if (!Directory.Exists(rootPath))
            {
                Debug.WriteLine($"Klasör bulunamadı: {rootPath}");
                return largeFiles;
            }
            
            try
            {
                Debug.WriteLine($"Büyük dosyalar aranıyor: {rootPath}, Min boyut: {minSizeMB} MB ({minSizeBytes} bytes)");
                FindLargeFilesRecursive(rootPath, largeFiles, minSizeBytes);
                Debug.WriteLine($"{largeFiles.Count} büyük dosya bulundu");
                return largeFiles.OrderByDescending(f => f.SizeBytes).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Büyük dosyalar bulunurken hata: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return largeFiles;
            }
        });
    }

    public async Task<List<LargeFile>> FindLargeFilesInFolderAsync(string folderPath, long minSizeMB = 100)
    {
        return await Task.Run(() =>
        {
            var largeFiles = new List<LargeFile>();
            var minSizeBytes = minSizeMB * 1024 * 1024;
            
            if (!Directory.Exists(folderPath))
                return largeFiles;
            
            try
            {
                FindLargeFilesRecursive(folderPath, largeFiles, minSizeBytes);
                return largeFiles.OrderByDescending(f => f.SizeBytes).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Klasörde büyük dosyalar bulunurken hata: {ex.Message}");
                return largeFiles;
            }
        });
    }

    private void AnalyzeFolderRecursive(string path, List<FolderSize> folderSizes, int currentDepth, int maxDepth)
    {
        if (currentDepth >= maxDepth)
            return;
        
        try
        {
            var directoryInfo = new DirectoryInfo(path);
            var folders = directoryInfo.GetDirectories();
            
            foreach (var folder in folders)
            {
                try
                {
                    if (folder.Attributes.HasFlag(FileAttributes.System) || 
                        folder.Attributes.HasFlag(FileAttributes.Hidden))
                        continue;
                    
                    long folderSize = 0;
                    int fileCount = 0;
                    
                    try
                    {
                        var files = folder.GetFiles("*", SearchOption.TopDirectoryOnly);
                        fileCount = files.Length;
                        folderSize = files.Sum(f => f.Length);
                    }
                    catch
                    {
                        // Skip inaccessible files
                    }
                    
                    var folderSizeInfo = new FolderSize
                    {
                        Path = folder.FullName,
                        Name = folder.Name,
                        SizeBytes = folderSize,
                        FileCount = fileCount,
                        FolderCount = folder.GetDirectories().Length
                    };
                    
                    folderSizes.Add(folderSizeInfo);
                    
                    // Recursively analyze subfolders
                    AnalyzeFolderRecursive(folder.FullName, folderSizes, currentDepth + 1, maxDepth);
                }
                catch
                {
                    // Skip inaccessible folders
                }
            }
        }
        catch
        {
            // Skip inaccessible paths
        }
    }

    private void FindLargeFilesRecursive(string path, List<LargeFile> largeFiles, long minSizeBytes)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles();
            
            foreach (var file in files)
            {
                try
                {
                    if (file.Length >= minSizeBytes)
                    {
                        var largeFile = new LargeFile
                        {
                            Path = file.FullName,
                            Name = file.Name,
                            Directory = file.DirectoryName ?? "",
                            SizeBytes = file.Length,
                            LastModified = file.LastWriteTime,
                            Extension = file.Extension
                        };
                        
                        largeFiles.Add(largeFile);
                    }
                }
                catch
                {
                    // Skip inaccessible files
                }
            }
            
            // Recursively search subdirectories
            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                try
                {
                    if (!directory.Attributes.HasFlag(FileAttributes.System) &&
                        !directory.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        FindLargeFilesRecursive(directory.FullName, largeFiles, minSizeBytes);
                    }
                }
                catch
                {
                    // Skip inaccessible directories
                }
            }
        }
        catch
        {
            // Skip inaccessible paths
        }
    }
}
