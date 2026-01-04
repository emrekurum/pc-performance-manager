# PC Performance Manager

A modern Windows desktop application built with WPF and .NET 8.0 for optimizing system performance by managing RAM usage, power plans, and cleaning up unnecessary files.

## Overview

PC Performance Manager is a comprehensive system optimization tool designed for Windows 10/11. It provides users with powerful features to monitor system resources, optimize memory usage, manage power plans, and clean up temporary files to improve overall system performance.

## Features

### Current Features

- **MVVM Architecture**: Clean separation of concerns using the Model-View-ViewModel pattern
- **Modern UI**: Responsive design with intuitive navigation
- **Administrator Privileges**: Built-in administrator privilege handling and validation
- **System Information**: Real-time system information gathering (CPU, RAM, Disk)
- **RAM Management**: Memory usage monitoring and optimization capabilities
- **Power Management**: Windows power plan management and switching
- **File Cleanup**: Temporary file analysis and cleanup functionality

### Planned Features

- **Real-time Monitoring**: Live system performance dashboard with charts
- **Advanced Process Management**: Detailed process monitoring and management
- **Scheduled Tasks**: Automated cleanup and optimization schedules
- **Settings Panel**: Application configuration and preferences
- **Logging System**: Comprehensive logging for debugging and auditing

## Requirements

- .NET 8.0 SDK or later
- Windows 10/11 (64-bit)
- Administrator privileges (required for system operations)

## Technologies

- **.NET 8.0**: Latest .NET framework for Windows desktop applications
- **WPF**: Windows Presentation Foundation for rich UI development
- **CommunityToolkit.Mvvm**: Modern MVVM implementation with code generation
- **System.Management**: System information and performance monitoring via WMI

## Project Structure

```
PcPerformanceManager/
├── Views/              # XAML view files
│   ├── DashboardView.xaml
│   ├── RamView.xaml
│   ├── PowerView.xaml
│   └── CleanupView.xaml
├── ViewModels/         # ViewModel classes (business logic)
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── RamViewModel.cs
│   ├── PowerViewModel.cs
│   └── CleanupViewModel.cs
├── Models/             # Data models
│   ├── SystemInfo.cs
│   ├── MemoryInfo.cs
│   ├── PowerPlan.cs
│   ├── CleanupItem.cs
│   ├── ProcessMemoryInfo.cs
│   └── NavigationItem.cs
├── Services/           # Business services
│   ├── IMemoryService.cs / MemoryService.cs
│   ├── IPowerService.cs / PowerService.cs
│   └── ICleanupService.cs / CleanupService.cs
├── Helpers/            # Utility classes
│   ├── AdminHelper.cs
│   └── SystemInfoHelper.cs
├── MainWindow.xaml     # Main application window
├── App.xaml            # Application definition
└── app.manifest        # Application manifest (requires admin privileges)
```

## Installation

### Prerequisites

1. Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Clone the repository:
   ```bash
   git clone https://github.com/emrekurum/pc-performance-manager.git
   cd pc-performance-manager
   ```

### Build

```bash
dotnet restore
dotnet build
```

### Run

```bash
dotnet run
```

**Note**: The application requires administrator privileges. Run your terminal/IDE as administrator, or the application will request elevation at startup.

## Usage

1. Launch the application (as administrator)
2. Navigate through the menu on the left:
   - **Dashboard**: System overview and quick actions
   - **RAM**: Memory usage monitoring and optimization
   - **Power**: Power plan management
   - **Cleanup**: File cleanup and disk space management

## Architecture

### MVVM Pattern

The application follows the Model-View-ViewModel (MVVM) architectural pattern:

- **Model**: Data structures and business entities located in the Models folder
- **View**: XAML UI definitions in the Views folder
- **ViewModel**: Business logic and data binding in the ViewModels folder
- **Services**: Reusable business services in the Services folder
- **Helpers**: Utility and helper classes in the Helpers folder

### Key Components

- **MainViewModel**: Handles navigation and main window logic
- **DashboardViewModel**: Manages system overview and quick actions
- **RamViewModel**: Provides RAM monitoring and optimization capabilities
- **PowerViewModel**: Manages Windows power plans
- **CleanupViewModel**: Handles file analysis and cleanup operations
- **MemoryService**: Implements RAM optimization using Windows API
- **PowerService**: Manages power plans using Windows powercfg utility
- **CleanupService**: Handles file system cleanup operations
- **SystemInfoHelper**: Collects system information using WMI queries

## Security

The application requires administrator privileges to:
- Access system performance counters
- Modify power plans
- Clean system directories
- Optimize memory usage

All operations are performed with proper error handling and user consent. The application does not collect or transmit any user data.

## Development Status

### Phase 1: Complete
- Project setup and MVVM architecture
- Helper classes (AdminHelper, SystemInfoHelper)
- Data models (SystemInfo, MemoryInfo, PowerPlan, CleanupItem)
- Service layer (MemoryService, PowerService, CleanupService)

### Phase 2: Complete
- ViewModel implementations for all modules
- Service integration
- Command patterns and async operations

### Phase 3: In Progress
- View implementations with modern UI
- Real-time monitoring
- Advanced features

See [ROADMAP.md](ROADMAP.md) for detailed development plan.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

Please ensure your code follows the existing code style and includes appropriate documentation.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**emrekurum**

- GitHub: [@emrekurum](https://github.com/emrekurum)

## Acknowledgments

- Built with [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Powered by .NET 8.0
- Windows Presentation Foundation (WPF) framework

## Disclaimer

This application performs system-level operations that can affect system performance and stability. Use at your own risk. Always backup your data before using cleanup features. The authors are not responsible for any data loss or system damage that may occur from using this software.
