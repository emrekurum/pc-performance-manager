using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Converters;

public class ServiceStatusToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ServiceStatus status && parameter is string param)
        {
            if (param == "Running")
            {
                return status == ServiceStatus.Running ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (param == "Stopped")
            {
                return status == ServiceStatus.Stopped ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
