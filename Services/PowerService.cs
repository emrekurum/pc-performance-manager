using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public class PowerService : IPowerService
{
    public List<PowerPlan> GetPowerPlans()
    {
        var powerPlans = new List<PowerPlan>();

        try
        {
            var activePlanGuid = GetActivePowerPlanGuid();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/list",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    var lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains("GUID:"))
                        {
                            var parts = line.Split(new[] { "GUID:" }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 1)
                            {
                                var guidPart = parts[1].Trim().Split('(')[0].Trim();
                                var namePart = parts[1].Contains("(") 
                                    ? parts[1].Substring(parts[1].IndexOf('(') + 1).TrimEnd(')', ' ', '\r', '\n')
                                    : "";

                                if (Guid.TryParse(guidPart, out Guid guid))
                                {
                                    powerPlans.Add(new PowerPlan
                                    {
                                        Guid = guid,
                                        Name = namePart,
                                        Description = namePart,
                                        IsActive = guid == activePlanGuid
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Güç planları alma hatası: {ex.Message}");
        }

        return powerPlans;
    }

    public PowerPlan? GetActivePowerPlan()
    {
        try
        {
            var activeGuid = GetActivePowerPlanGuid();
            var allPlans = GetPowerPlans();
            return allPlans.FirstOrDefault(p => p.Guid == activeGuid);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Aktif güç planı alma hatası: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SetActivePowerPlanAsync(Guid powerPlanGuid)
    {
        return await Task.Run(() =>
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/setactive {powerPlanGuid}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // Yönetici izinleri için
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Güç planı değiştirme hatası: {ex.Message}");
            }

            return false;
        });
    }

    public async Task<bool> SetActivePowerPlanByNameAsync(string powerPlanName)
    {
        var plans = GetPowerPlans();
        var plan = plans.FirstOrDefault(p => 
            p.Name.Contains(powerPlanName, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(powerPlanName, StringComparison.OrdinalIgnoreCase));

        if (plan != null)
        {
            return await SetActivePowerPlanAsync(plan.Guid);
        }

        return false;
    }

    private Guid GetActivePowerPlanGuid()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/getactivescheme",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    var parts = output.Split(new[] { "GUID:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        var guidString = parts[1].Trim().Split('(')[0].Trim();
                        if (Guid.TryParse(guidString, out Guid guid))
                        {
                            return guid;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Aktif güç planı GUID alma hatası: {ex.Message}");
        }

        return Guid.Empty;
    }
}

