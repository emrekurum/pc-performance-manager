using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PcPerformanceManager.Converters;

public class EnabledToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
        {
            return isEnabled 
                ? new SolidColorBrush(Color.FromArgb(255, 63, 185, 80))    // #3FB950 (Green)
                : new SolidColorBrush(Color.FromArgb(255, 248, 81, 73));   // #F85149 (Red)
        }
        return new SolidColorBrush(Color.FromArgb(255, 139, 148, 158));    // #8B949E (Gray)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
