using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class TabButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentTab && parameter is string tabName)
        {
            // Eğer currentTab == tabName ise PrimaryButton, değilse SecondaryButton
            return currentTab == tabName ? "PrimaryButton" : "SecondaryButton";
        }
        return "SecondaryButton";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
