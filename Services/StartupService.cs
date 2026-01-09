using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Win32;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class StartupService : IStartupService
{
    public async Task<List<StartupProgram>> GetStartupProgramsAsync()
    {
        return await Task.Run(() =>
        {
            var programs = new List<StartupProgram>();
            try
            {
                programs.AddRange(GetRegistryStartupPrograms(Registry.CurrentUser, StartupLocation.Registry));
                programs.AddRange(GetRegistryStartupPrograms(Registry.LocalMachine, StartupLocation.RegistryMachine));
                programs.AddRange(GetStartupFolderPrograms());
                programs.AddRange(GetServiceStartupPrograms());
                
                foreach (var program in programs)
                {
                    program.Impact = AnalyzeStartupImpact(program);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting startup programs: {ex.Message}");
            }
            return programs.OrderBy(p => p.DisplayName).ToList();
        });
    }

    private List<StartupProgram> GetRegistryStartupPrograms(RegistryKey rootKey, StartupLocation location)
    {
        var programs = new List<StartupProgram>();
        try
        {
            using var runKey = rootKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (runKey == null) return programs;

            foreach (var valueName in runKey.GetValueNames())
            {
                var value = runKey.GetValue(valueName);
                if (value == null) continue;

                var command = value.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(command)) continue;

                programs.Add(new StartupProgram
                {
                    Name = valueName,
                    DisplayName = GetDisplayName(valueName, command),
                    Command = command,
                    Location = location,
                    LocationPath = $@"{rootKey.Name}\Software\Microsoft\Windows\CurrentVersion\Run\{valueName}",
                    IsEnabled = true,
                    Publisher = GetPublisherFromPath(command)
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading registry: {ex.Message}");
        }
        return programs;
    }

    private List<StartupProgram> GetStartupFolderPrograms()
    {
        var programs = new List<StartupProgram>();
        try
        {
            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (!Directory.Exists(startupFolderPath)) return programs;

            var files = Directory.GetFiles(startupFolderPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                programs.Add(new StartupProgram
                {
                    Name = fileName,
                    DisplayName = fileName,
                    Command = file,
                    Location = StartupLocation.StartupFolder,
                    LocationPath = file,
                    IsEnabled = true,
                    Publisher = GetPublisherFromPath(file)
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading startup folder: {ex.Message}");
        }
        return programs;
    }

    private List<StartupProgram> GetTaskSchedulerStartupPrograms()
    {
        return new List<StartupProgram>(); // BasitleÅŸtirilmiÅŸ - daha sonra geniÅŸletilebilir
    }

    private List<StartupProgram> GetServiceStartupPrograms()
    {
        var programs = new List<StartupProgram>();
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DisplayName, PathName, StartMode FROM Win32_Service WHERE StartMode = 'Auto' OR StartMode = 'Automatic'"
            );

            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "";
                var displayName = obj["DisplayName"]?.ToString() ?? name;
                var pathName = obj["PathName"]?.ToString() ?? "";
                var startMode = obj["StartMode"]?.ToString() ?? "";

                programs.Add(new StartupProgram
                {
                    Name = name,
                    DisplayName = displayName,
                    Command = pathName,
                    Location = StartupLocation.Service,
                    LocationPath = $"Service: {name}",
                    IsEnabled = true,
                    Description = $"Service - {startMode}"
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading services: {ex.Message}");
        }
        return programs;
    }

    public async Task<bool> EnableStartupAsync(StartupProgram program)
    {
        return await Task.Run(() =>
        {
            try
            {
                return program.Location switch
                {
                    StartupLocation.Registry => EnableRegistryStartup(program, Registry.CurrentUser),
                    StartupLocation.RegistryMachine => EnableRegistryStartup(program, Registry.LocalMachine),
                    StartupLocation.StartupFolder => File.Exists(program.Command),
                    StartupLocation.Service => EnableServiceStartup(program),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error enabling startup: {ex.Message}");
                return false;
            }
        });
    }

    public async Task<bool> DisableStartupAsync(StartupProgram program)
    {
        return await Task.Run(() =>
        {
            try
            {
                return program.Location switch
                {
                    StartupLocation.Registry => DisableRegistryStartup(program, Registry.CurrentUser),
                    StartupLocation.RegistryMachine => DisableRegistryStartup(program, Registry.LocalMachine),
                    StartupLocation.StartupFolder => DisableStartupFolderProgram(program),
                    StartupLocation.Service => DisableServiceStartup(program),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disabling startup: {ex.Message}");
                return false;
            }
        });
    }

    private bool EnableRegistryStartup(StartupProgram program, RegistryKey rootKey)
    {
        try
        {
            using var runKey = rootKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (runKey == null) return false;
            runKey.SetValue(program.Name, program.Command);
            return true;
        }
        catch { return false; }
    }

    private bool DisableRegistryStartup(StartupProgram program, RegistryKey rootKey)
    {
        try
        {
            using var runKey = rootKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (runKey == null) return false;
            runKey.DeleteValue(program.Name, false);
            return true;
        }
        catch { return false; }
    }

    private bool EnableServiceStartup(StartupProgram program)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"config \"{program.Name}\" start= auto",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null) return false;
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch { return false; }
    }

    private bool DisableServiceStartup(StartupProgram program)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"config \"{program.Name}\" start= disabled",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null) return false;
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch { return false; }
    }

    private bool DisableStartupFolderProgram(StartupProgram program)
    {
        try
        {
            if (File.Exists(program.Command))
            {
                var backupFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PcPerformanceManager",
                    "StartupBackup"
                );
                Directory.CreateDirectory(backupFolder);
                var backupPath = Path.Combine(backupFolder, Path.GetFileName(program.Command));
                File.Move(program.Command, backupPath, true);
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public StartupImpact AnalyzeStartupImpact(StartupProgram program)
    {
        if (program.Location == StartupLocation.Service) return StartupImpact.High;
        if (program.Location == StartupLocation.TaskScheduler) return StartupImpact.Medium;

        var exePath = ExtractExePath(program.Command);
        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            var fileInfo = new FileInfo(exePath);
            if (fileInfo.Length > 50 * 1024 * 1024) return StartupImpact.High;
            if (fileInfo.Length > 10 * 1024 * 1024) return StartupImpact.Medium;
        }

        return StartupImpact.Low;
    }

    public async Task<double> EstimateTotalStartupTimeAsync(List<StartupProgram> programs)
    {
        return await Task.Run(() =>
        {
            double totalSeconds = 0;
            foreach (var program in programs.Where(p => p.IsEnabled))
            {
                totalSeconds += program.Impact switch
                {
                    StartupImpact.High => 5.0,
                    StartupImpact.Medium => 2.0,
                    StartupImpact.Low => 0.5,
                    _ => 1.0
                };
            }
            return totalSeconds;
        });
    }

    private string GetDisplayName(string name, string command)
    {
        if (!string.IsNullOrWhiteSpace(name) && name.Length > 3) return name;
        var exePath = ExtractExePath(command);
        if (!string.IsNullOrEmpty(exePath)) return Path.GetFileNameWithoutExtension(exePath);
        return name;
    }

    private string ExtractExePath(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return string.Empty;
        command = command.Trim('"');
        var parts = command.Split(' ');
        if (parts.Length > 0)
        {
            var potentialPath = parts[0].Trim('"');
            if (File.Exists(potentialPath)) return potentialPath;
        }
        if (File.Exists(command)) return command;
        return string.Empty;
    }

    private string GetPublisherFromPath(string path)
    {
        try
        {
            var exePath = ExtractExePath(path);
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return "Bilinmeyen";
            var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
            return versionInfo.CompanyName ?? versionInfo.ProductName ?? "Bilinmeyen";
        }
        catch { return "Bilinmeyen"; }
    }
}
