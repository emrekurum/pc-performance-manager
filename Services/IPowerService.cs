using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Services;

public interface IPowerService
{
    /// <summary>
    /// Sistemdeki tüm güç planlarını listeler
    /// </summary>
    List<PowerPlan> GetPowerPlans();

    /// <summary>
    /// Aktif güç planını döndürür
    /// </summary>
    PowerPlan? GetActivePowerPlan();

    /// <summary>
    /// Belirtilen güç planını aktif yapar
    /// </summary>
    Task<bool> SetActivePowerPlanAsync(Guid powerPlanGuid);

    /// <summary>
    /// Güç planı adına göre aktif yapar
    /// </summary>
    Task<bool> SetActivePowerPlanByNameAsync(string powerPlanName);
}


