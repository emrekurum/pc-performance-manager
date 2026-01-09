using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Converters;

public class ImpactToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is StartupImpact impact)
        {
            return impact switch
            {
                StartupImpact.High => new SolidColorBrush(Color.FromArgb(255, 248, 81, 73)),      // #F85149 (Red)
                StartupImpact.Medium => new SolidColorBrush(Color.FromArgb(255, 210, 153, 34)),  // #D29922 (Yellow)
                StartupImpact.Low => new SolidColorBrush(Color.FromArgb(255, 63, 185, 80)),     // #3FB950 (Green)
                _ => new SolidColorBrush(Color.FromArgb(255, 139, 148, 158))                    // #8B949E (Gray)
            };
        }
        return new SolidColorBrush(Color.FromArgb(255, 139, 148, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
