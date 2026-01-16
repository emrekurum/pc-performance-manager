namespace PcPerformanceManager.Services;

public interface IWindowsStartupService
{
    bool IsStartupEnabled();
    bool EnableStartup();
    bool DisableStartup();
}
