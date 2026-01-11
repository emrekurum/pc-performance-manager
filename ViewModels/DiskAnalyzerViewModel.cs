using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;

namespace PcPerformanceManager.ViewModels;

public partial class DiskAnalyzerViewModel : ObservableObject
{
    private readonly IDiskAnalyzerService _diskAnalyzerService;

    [ObservableProperty]
    private ObservableCollection<DiskInfo> disks = new();

    [ObservableProperty]
    private DiskInfo? selectedDisk;

    [ObservableProperty]
    private ObservableCollection<FolderSize> folderSizes = new();

    [ObservableProperty]
    private ObservableCollection<LargeFile> largeFiles = new();

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private string statusMessage = "Hazır";

    [ObservableProperty]
    private string currentTab = "Disks";

    [ObservableProperty]
    private double minFileSizeMB = 100;

    [ObservableProperty]
    private double totalFolderSizeGB;

    [ObservableProperty]
    private int totalFoldersCount;

    [ObservableProperty]
    private double totalLargeFilesSizeGB;

    [ObservableProperty]
    private int totalLargeFilesCount;

    public DiskAnalyzerViewModel()
    {
        _diskAnalyzerService = new DiskAnalyzerService();
        _ = LoadDisksAsync();
    }

    [RelayCommand]
    private async Task LoadDisksAsync()
    {
        if (IsAnalyzing) return;

        IsAnalyzing = true;
        StatusMessage = "Diskler yükleniyor...";

        try
        {
            var disksList = await _diskAnalyzerService.GetDisksAsync();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                Disks.Clear();
                foreach (var disk in disksList)
                {
                    Disks.Add(disk);
                }
                
                if (Disks.Any() && SelectedDisk == null)
                {
                    var firstDisk = Disks.First();
                    firstDisk.IsSelected = true;
                    SelectedDisk = firstDisk;
                }
            });

            StatusMessage = $"{disksList.Count} disk bulundu";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Diskler yüklenirken hata oluştu:\n{ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task AnalyzeFolderSizesAsync()
    {
        if (SelectedDisk == null || IsScanning) return;

        IsScanning = true;
        StatusMessage = $"Klasör boyutları analiz ediliyor: {SelectedDisk.DriveLetter}";
        FolderSizes.Clear();

        try
        {
            var folders = await _diskAnalyzerService.AnalyzeFolderSizesAsync(SelectedDisk.DriveLetter, 3);
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                FolderSizes.Clear();
                foreach (var folder in folders.Take(100)) // İlk 100 klasörü göster
                {
                    FolderSizes.Add(folder);
                }
                
                UpdateFolderStatistics();
            });

            StatusMessage = $"{folders.Count} klasör analiz edildi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Klasör boyutları analiz edilirken hata oluştu:\n{ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task FindLargeFilesAsync()
    {
        if (SelectedDisk == null || IsScanning) return;

        IsScanning = true;
        StatusMessage = $"Büyük dosyalar aranıyor: {SelectedDisk.DriveLetter} (Min: {MinFileSizeMB} MB)";
        LargeFiles.Clear();

        try
        {
            var files = await _diskAnalyzerService.FindLargeFilesAsync(SelectedDisk.DriveLetter, (long)MinFileSizeMB);
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                LargeFiles.Clear();
                foreach (var file in files.Take(200)) // İlk 200 dosyayı göster
                {
                    LargeFiles.Add(file);
                }
                
                UpdateLargeFilesStatistics();
            });

            StatusMessage = $"{files.Count} büyük dosya bulundu";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Büyük dosyalar aranırken hata oluştu:\n{ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void SelectDiskTab()
    {
        CurrentTab = "Disks";
        StatusMessage = "Disk görünümü";
    }

    [RelayCommand]
    private void SelectFoldersTab()
    {
        CurrentTab = "Folders";
        StatusMessage = "Klasör boyutları görünümü";
    }

    [RelayCommand]
    private void SelectLargeFilesTab()
    {
        CurrentTab = "LargeFiles";
        StatusMessage = "Büyük dosyalar görünümü";
    }

    partial void OnSelectedDiskChanged(DiskInfo? value)
    {
        // Tüm disklerin IsSelected özelliğini güncelle
        foreach (var disk in Disks)
        {
            disk.IsSelected = disk == value;
        }
        
        if (value != null)
        {
            FolderSizes.Clear();
            LargeFiles.Clear();
            StatusMessage = $"{value.DriveLetter} seçildi";
        }
    }

    private void UpdateFolderStatistics()
    {
        TotalFolderSizeGB = FolderSizes.Sum(f => f.SizeGB);
        TotalFoldersCount = FolderSizes.Count;
    }

    private void UpdateLargeFilesStatistics()
    {
        TotalLargeFilesSizeGB = LargeFiles.Sum(f => f.SizeGB);
        TotalLargeFilesCount = LargeFiles.Count;
    }
}
