using System.Windows;
using PcPerformanceManager.ViewModels;

namespace PcPerformanceManager.Views;

public partial class RamView : System.Windows.Controls.UserControl
{
    public RamView()
    {
        InitializeComponent();
    }

    private async void ClearProcessButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is int processId && DataContext is RamViewModel viewModel)
        {
            var process = viewModel.Processes.FirstOrDefault(p => p.ProcessId == processId);
            if (process == null) return;

            viewModel.SelectedProcess = process;
            await viewModel.ClearSelectedProcessMemoryCommand.ExecuteAsync(null);
        }
    }
}
