using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class TabVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentTab && parameter is string tabName)
        {
            return currentTab == tabName ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
