using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class EnabledToButtonStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
        {
            // Eğer enabled ise "Danger" (devre dışı bırak butonu), değilse "Success" (etkinleştir butonu)
            return Application.Current.FindResource(isEnabled ? "DangerButton" : "SuccessButton");
        }
        return Application.Current.FindResource("SecondaryButton");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
