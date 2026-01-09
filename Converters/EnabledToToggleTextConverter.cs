using System;
using System.Globalization;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class EnabledToToggleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
        {
            return isEnabled ? "Devre Dışı Bırak" : "Etkinleştir";
        }
        return "Bilinmiyor";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
