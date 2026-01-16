using System.Windows;
using System.Windows.Input;
using PcPerformanceManager.Models;

namespace PcPerformanceManager.Views;

public partial class DiskAnalyzerView : System.Windows.Controls.UserControl
{
    public DiskAnalyzerView()
    {
        InitializeComponent();
    }

    private void DiskBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Border border && border.DataContext is DiskInfo diskInfo)
        {
            if (DataContext is ViewModels.DiskAnalyzerViewModel viewModel)
            {
                viewModel.SelectedDisk = diskInfo;
            }
        }
    }
}
