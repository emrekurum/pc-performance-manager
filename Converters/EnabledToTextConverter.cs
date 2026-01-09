using System;
using System.Globalization;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class EnabledToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
        {
            return isEnabled ? "Etkin" : "Devre Dışı";
        }
        return "Bilinmiyor";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
