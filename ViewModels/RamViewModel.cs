using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PcPerformanceManager.Models;
using PcPerformanceManager.Services;

namespace PcPerformanceManager.ViewModels;

public partial class RamViewModel : ObservableObject
{
    private readonly IMemoryService _memoryService;
    private DispatcherTimer? _refreshTimer;

    [ObservableProperty]
    private MemoryInfo memoryInfo = new();

    [ObservableProperty]
    private ObservableCollection<ProcessMemoryInfo> processes = new();

    [ObservableProperty]
    private ObservableCollection<CleanableProcess> bloatwareProcesses = new();

    [ObservableProperty]
    private ProcessMemoryInfo? selectedProcess;

    [ObservableProperty]
    private bool isAutoRefreshEnabled;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isClearingMemory;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private int refreshIntervalSeconds = 5;

    [ObservableProperty]
    private int bloatwareCount;

    [ObservableProperty]
    private double bloatwareTotalMB;

    // Formatted display strings
    public string TotalMemoryDisplay => $"{MemoryInfo.TotalGB:F2} GB";
    public string UsedMemoryDisplay => $"{MemoryInfo.UsedGB:F2} GB";
    public string FreeMemoryDisplay => $"{MemoryInfo.FreeGB:F2} GB";

    public RamViewModel()
    {
        _memoryService = new MemoryService();
        LoadMemoryInfo();
        _ = RefreshDataAsync(); // Fire and forget
        InitializeAutoRefresh();
    }

    private void InitializeAutoRefresh()
    {
        try
        {
            var settingsService = new SettingsService();
            var settings = settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
            
            if (settings.AutoRefreshEnabled)
            {
                RefreshIntervalSeconds = settings.RefreshIntervalSeconds;
                IsAutoRefreshEnabled = true;
            }
        }
        catch
        {
            // Settings yüklenemezse auto refresh devre dışı
        }
    }

    private void LoadMemoryInfo()
    {
        MemoryInfo = _memoryService.GetMemoryInfo();
        OnPropertyChanged(nameof(TotalMemoryDisplay));
        OnPropertyChanged(nameof(UsedMemoryDisplay));
        OnPropertyChanged(nameof(FreeMemoryDisplay));
    }

    partial void OnIsAutoRefreshEnabledChanged(bool value)
    {
        if (value)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }
    }

    partial void OnRefreshIntervalSecondsChanged(int value)
    {
        if (IsAutoRefreshEnabled)
        {
            StopAutoRefresh();
            StartAutoRefresh();
        }
    }

    partial void OnMemoryInfoChanged(MemoryInfo value)
    {
        OnPropertyChanged(nameof(TotalMemoryDisplay));
        OnPropertyChanged(nameof(UsedMemoryDisplay));
        OnPropertyChanged(nameof(FreeMemoryDisplay));
    }

    private void StartAutoRefresh()
    {
        _refreshTimer?.Stop();
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(RefreshIntervalSeconds)
        };
        _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();
        _refreshTimer.Start();
    }

    private void StopAutoRefresh()
    {
        if (_refreshTimer != null)
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= async (s, e) => await RefreshDataAsync();
            _refreshTimer = null;
        }
    }

    // Dispose pattern for timer cleanup
    public void Dispose()
    {
        StopAutoRefresh();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Refreshing...";

        try
        {
            MemoryInfo memoryInfoData = new();
            List<ProcessMemoryInfo> processList = new();

            await Task.Run(() =>
            {
                memoryInfoData = _memoryService.GetMemoryInfo();
                processList = _memoryService.GetProcessMemoryUsage();
            });

            // Update UI thread - must be on UI thread for property changes
            Application.Current.Dispatcher.Invoke(() =>
            {
                MemoryInfo = memoryInfoData;
            });
            
            Processes.Clear();
            foreach (var process in processList)
            {
                Processes.Add(process);
            }

            StatusMessage = $"Updated - {Processes.Count} processes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task ClearAllMemoryAsync()
    {
        var result = MessageBox.Show(
            "Bu işlem arka planda çalışan gereksiz uygulamaların RAM kullanımını temizleyecek.\n\n" +
            "• Aktif kullandığınız uygulamalar (tarayıcı, IDE, Office vb.) korunacak\n" +
            "• Sistem uygulamaları korunacak\n\n" +
            "Devam etmek istiyor musunuz?",
            "RAM Temizle",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsClearingMemory = true;
        StatusMessage = "RAM temizleniyor...";

        try
        {
            var success = await _memoryService.ClearMemoryAsync();

            if (success)
            {
                StatusMessage = "RAM başarıyla temizlendi";
                await RefreshDataAsync();
                MessageBox.Show("RAM başarıyla temizlendi!\n\nAktif uygulamalarınız korundu.", 
                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Temizlenecek process bulunamadı";
                MessageBox.Show("Temizlenecek uygun process bulunamadı veya tüm process'ler zaten optimize edilmiş.",
                    "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsClearingMemory = false;
        }
    }

    [RelayCommand]
    private async Task ClearSelectedProcessMemoryAsync()
    {
        if (SelectedProcess == null)
        {
            MessageBox.Show("Please select a process first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Clear memory for process '{SelectedProcess.ProcessName}' (PID: {SelectedProcess.ProcessId})?",
            "Clear Process Memory",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var success = await Task.Run(() => _memoryService.ClearProcessMemory(SelectedProcess.ProcessId));

            if (success)
            {
                StatusMessage = $"Memory cleared for {SelectedProcess.ProcessName}";
                await RefreshDataAsync();
            }
            else
            {
                StatusMessage = "Failed to clear process memory";
                MessageBox.Show("Failed to clear process memory. Make sure you have administrator privileges.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SortByMemoryUsage()
    {
        var sorted = Processes.OrderByDescending(p => p.WorkingSetBytes).ToList();
        Processes.Clear();
        foreach (var process in sorted)
        {
            Processes.Add(process);
        }
        StatusMessage = "Sorted by memory usage";
    }

    [RelayCommand]
    private void SortByName()
    {
        var sorted = Processes.OrderBy(p => p.ProcessName).ToList();
        Processes.Clear();
        foreach (var process in sorted)
        {
            Processes.Add(process);
        }
        StatusMessage = "Sorted by name";
    }

    /// <summary>
    /// Gereksiz/bloatware process'leri analiz eder
    /// </summary>
    [RelayCommand]
    private async Task AnalyzeBloatwareAsync()
    {
        if (IsAnalyzing) return;

        IsAnalyzing = true;
        StatusMessage = "Gereksiz uygulamalar analiz ediliyor...";

        try
        {
            List<CleanableProcess> bloatware = new();

            await Task.Run(() =>
            {
                bloatware = _memoryService.AnalyzeUnnecessaryProcesses();
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                BloatwareProcesses.Clear();
                foreach (var proc in bloatware)
                {
                    BloatwareProcesses.Add(proc);
                }

                BloatwareCount = bloatware.Count;
                BloatwareTotalMB = bloatware.Sum(p => p.MemoryMB);
            });

            if (bloatware.Count > 0)
            {
                StatusMessage = $"{bloatware.Count} gereksiz uygulama bulundu ({BloatwareTotalMB:F1} MB)";
            }
            else
            {
                StatusMessage = "Gereksiz uygulama bulunamadı - sisteminiz temiz!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Analiz hatası: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    /// <summary>
    /// Seçili bloatware process'leri temizler (kapatır)
    /// </summary>
    [RelayCommand]
    private async Task CleanBloatwareAsync()
    {
        var selectedProcesses = BloatwareProcesses.Where(p => p.IsSelected && p.IsRunning).ToList();
        
        if (!selectedProcesses.Any())
        {
            MessageBox.Show("Lütfen kapatılacak uygulamaları seçin.", "Seçim Yok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Seçili {selectedProcesses.Count} uygulama kapatılacak.\n" +
            $"Toplam {selectedProcesses.Sum(p => p.MemoryMB):F1} MB RAM serbest bırakılacak.\n\n" +
            "Devam etmek istiyor musunuz?",
            "Uygulamaları Kapat",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsClearingMemory = true;
        StatusMessage = "Uygulamalar kapatılıyor...";

        try
        {
            var (terminated, failed, freedMB) = await _memoryService.TerminateProcessesAsync(selectedProcesses);

            // UI güncelle
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Kapatılan process'leri listeden kaldır
                var closedProcesses = BloatwareProcesses.Where(p => !p.IsRunning).ToList();
                foreach (var proc in closedProcesses)
                {
                    BloatwareProcesses.Remove(proc);
                }

                BloatwareCount = BloatwareProcesses.Count;
                BloatwareTotalMB = BloatwareProcesses.Sum(p => p.MemoryMB);
            });

            await RefreshDataAsync();

            if (terminated > 0)
            {
                StatusMessage = $"{terminated} uygulama kapatıldı, {freedMB:F1} MB RAM serbest bırakıldı";
                MessageBox.Show(
                    $"✅ {terminated} uygulama başarıyla kapatıldı\n" +
                    $"💾 {freedMB:F1} MB RAM serbest bırakıldı" +
                    (failed > 0 ? $"\n⚠️ {failed} uygulama kapatılamadı" : ""),
                    "Temizlik Tamamlandı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Hiçbir uygulama kapatılamadı";
                MessageBox.Show("Seçili uygulamalar kapatılamadı. Yönetici yetkisi gerekebilir.",
                    "Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsClearingMemory = false;
        }
    }

    /// <summary>
    /// Tüm güvenli bloatware'i otomatik temizler
    /// </summary>
    [RelayCommand]
    private async Task AutoCleanBloatwareAsync()
    {
        var result = MessageBox.Show(
            "Bu işlem sisteminizdeki TÜM güvenli bloatware uygulamalarını otomatik olarak kapatacak.\n\n" +
            "• Microsoft Bloatware (OneDrive, Cortana, Xbox vb.)\n" +
            "• Telemetri servisleri\n" +
            "• Üçüncü parti güncelleyiciler\n" +
            "• Üretici bloatware'leri\n\n" +
            "Aktif kullandığınız uygulamalar korunacaktır.\n\n" +
            "Devam etmek istiyor musunuz?",
            "Otomatik Temizlik",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsClearingMemory = true;
        StatusMessage = "Otomatik temizlik yapılıyor...";

        try
        {
            var (terminated, freedMB) = await _memoryService.AutoCleanSafeProcessesAsync();

            await RefreshDataAsync();
            await AnalyzeBloatwareAsync();

            if (terminated > 0)
            {
                StatusMessage = $"Otomatik temizlik: {terminated} uygulama kapatıldı, {freedMB:F1} MB kazanıldı";
                MessageBox.Show(
                    $"✅ Otomatik temizlik tamamlandı!\n\n" +
                    $"🗑️ {terminated} gereksiz uygulama kapatıldı\n" +
                    $"💾 {freedMB:F1} MB RAM serbest bırakıldı",
                    "Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Kapatılacak gereksiz uygulama bulunamadı";
                MessageBox.Show("Sisteminizde kapatılacak gereksiz uygulama bulunamadı.\nSisteminiz zaten optimize edilmiş!",
                    "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
            MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsClearingMemory = false;
        }
    }

    [RelayCommand]
    private void SelectAllBloatware()
    {
        foreach (var proc in BloatwareProcesses)
        {
            proc.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAllBloatware()
    {
        foreach (var proc in BloatwareProcesses)
        {
            proc.IsSelected = false;
        }
    }

    [RelayCommand]
    private void SelectSafeBloatware()
    {
        foreach (var proc in BloatwareProcesses)
        {
            proc.IsSelected = proc.RiskLevel == ProcessRiskLevel.Safe;
        }
        StatusMessage = $"{BloatwareProcesses.Count(p => p.IsSelected)} güvenli uygulama seçildi";
    }
}
