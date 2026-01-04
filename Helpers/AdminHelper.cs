using System.Security.Principal;

namespace PcPerformanceManager.Helpers;

public static class AdminHelper
{
    /// <summary>
    /// Uygulamanın yönetici izinleriyle çalışıp çalışmadığını kontrol eder
    /// </summary>
    /// <returns>Yönetici izinleri varsa true, yoksa false</returns>
    public static bool IsRunningAsAdministrator()
    {
        try
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Yönetici izinleri kontrolü yapar ve gerekirse uyarı mesajı gösterir
    /// </summary>
    /// <returns>Yönetici izinleri varsa true</returns>
    public static bool CheckAdministratorPrivileges()
    {
        if (!IsRunningAsAdministrator())
        {
            System.Windows.MessageBox.Show(
                "Bu uygulama yönetici izinleri gerektirir. Lütfen uygulamayı yönetici olarak çalıştırın.",
                "Yönetici İzinleri Gerekli",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }
        return true;
    }
}

