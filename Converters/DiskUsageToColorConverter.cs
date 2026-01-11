using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PcPerformanceManager.Converters;

public class DiskUsageToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double usagePercentage)
        {
            return usagePercentage switch
            {
                >= 90 => new SolidColorBrush(Color.FromArgb(255, 248, 81, 73)),      // #F85149 (Red) - Kritik
                >= 75 => new SolidColorBrush(Color.FromArgb(255, 210, 153, 34)),      // #D29922 (Yellow) - Uyarı
                >= 50 => new SolidColorBrush(Color.FromArgb(255, 88, 166, 255)),      // #58A6FF (Blue) - Orta
                _ => new SolidColorBrush(Color.FromArgb(255, 63, 185, 80))            // #3FB950 (Green) - İyi
            };
        }
        return new SolidColorBrush(Color.FromArgb(255, 139, 148, 158)); // #8B949E (Gray)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
