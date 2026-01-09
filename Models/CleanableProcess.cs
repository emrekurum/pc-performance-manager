using System.ComponentModel;

namespace PcPerformanceManager.Models;

public class CleanableProcess : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isRunning;

    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double MemoryMB { get; set; }
    public ProcessRiskLevel RiskLevel { get; set; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
    
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum ProcessRiskLevel
{
    Safe,       // Tamamen güvenli - kapatılabilir
    Low,        // Düşük risk - genellikle güvenli
    Medium,     // Orta risk - dikkatli olunmalı
    High        // Yüksek risk - sistem etkilenebilir
}



