using Microsoft.Win32;

namespace PcPerformanceManager.Services;

public class WindowsStartupService : IWindowsStartupService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "PcPerformanceManager";

    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            var value = key?.GetValue(AppName);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public bool EnableStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return false;

            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            key.SetValue(AppName, $"\"{exePath}\"");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DisableStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return false;

            key.DeleteValue(AppName, false);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
