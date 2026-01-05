using System;
using System.Globalization;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class BytesToGBConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            return bytes / (1024.0 * 1024.0 * 1024.0);
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


