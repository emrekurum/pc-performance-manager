using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Converters;

public class ServiceStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ServiceStatus status)
        {
            return status switch
            {
                ServiceStatus.Running => new SolidColorBrush(Color.FromArgb(255, 63, 185, 80)),      // #3FB950 (Green)
                ServiceStatus.Stopped => new SolidColorBrush(Color.FromArgb(255, 248, 81, 73)),      // #F85149 (Red)
                ServiceStatus.Paused => new SolidColorBrush(Color.FromArgb(255, 210, 153, 34)),      // #D29922 (Yellow)
                ServiceStatus.Starting => new SolidColorBrush(Color.FromArgb(255, 88, 166, 255)),    // #58A6FF (Blue)
                ServiceStatus.Stopping => new SolidColorBrush(Color.FromArgb(255, 210, 153, 34)),    // #D29922 (Yellow)
                _ => new SolidColorBrush(Color.FromArgb(255, 139, 148, 158))                         // #8B949E (Gray)
            };
        }
        return new SolidColorBrush(Color.FromArgb(255, 139, 148, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
