# PC Performance Manager

A modern Windows desktop application built with WPF and .NET 8.0 for optimizing system performance by managing RAM usage, power plans, and cleaning up unnecessary files.

## 🚀 Features

### Current Features
- **MVVM Architecture**: Clean separation of concerns using the Model-View-ViewModel pattern
- **Modern UI**: Responsive design with intuitive navigation
- **Admin Privileges**: Built-in administrator privilege handling

### Planned Features
- **RAM Management**: Monitor and optimize RAM usage with working set clearing
- **Power Management**: View and switch between Windows power plans
- **File Cleanup**: Analyze and clean temporary files, cache, and unnecessary system files
- **System Monitoring**: Real-time system information dashboard (CPU, RAM, Disk usage)
- **Process Management**: View running processes and their memory consumption

## 📋 Requirements

- **.NET 8.0 SDK** or later
- **Windows 10/11** (64-bit)
- **Administrator privileges** (required for system operations)

## 🛠️ Technologies

- **.NET 8.0**: Latest .NET framework
- **WPF**: Windows Presentation Foundation for UI
- **CommunityToolkit.Mvvm**: Modern MVVM implementation
- **System.Management**: System information and monitoring

## 📁 Project Structure

```
PcPerformanceManager/
├── Views/              # XAML view files
│   ├── DashboardView.xaml
│   ├── RamView.xaml
│   ├── PowerView.xaml
│   └── CleanupView.xaml
├── ViewModels/         # ViewModel classes (business logic)
│   └── MainViewModel.cs
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

## 🔧 Installation

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

## 📖 Usage

1. Launch the application (as administrator)
2. Navigate through the menu on the left:
   - **Dashboard**: System overview and quick actions
   - **RAM**: Memory usage monitoring and optimization
   - **Power**: Power plan management
   - **Cleanup**: File cleanup and disk space management

## 🏗️ Architecture

### MVVM Pattern

The application follows the Model-View-ViewModel (MVVM) architectural pattern:

- **Model**: Data structures and business entities
- **View**: XAML UI definitions (Views folder)
- **ViewModel**: Business logic and data binding (ViewModels folder)
- **Services**: Reusable business services (Services folder)
- **Helpers**: Utility and helper classes (Helpers folder)

### Key Components

- **MainViewModel**: Handles navigation and main window logic
- **MemoryService**: Provides RAM monitoring and optimization capabilities
- **PowerService**: Manages Windows power plans
- **CleanupService**: Handles file analysis and cleanup operations
- **SystemInfoHelper**: Collects system information using WMI

## 🔒 Security

The application requires administrator privileges to:
- Access system performance counters
- Modify power plans
- Clean system directories
- Optimize memory usage

All operations are performed with proper error handling and user consent.

## 📊 Development Status

### ✅ Phase 1: Complete
- Project setup and MVVM architecture
- Helper classes (AdminHelper, SystemInfoHelper)
- Data models (SystemInfo, MemoryInfo, PowerPlan, CleanupItem)
- Service layer (MemoryService, PowerService, CleanupService)

### 🚧 Phase 2: In Progress
- ViewModel implementations
- UI/UX enhancements
- Service integration

### 📅 Phase 3: Planned
- View implementations
- Real-time monitoring
- Advanced features

See [ROADMAP.md](ROADMAP.md) for detailed development plan.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is open source and available under the MIT License.

## 👤 Author

**emrekurum**

- GitHub: [@emrekurum](https://github.com/emrekurum)

## 🙏 Acknowledgments

- Built with [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Powered by .NET 8.0

---

**⚠️ Disclaimer**: This application performs system-level operations. Use at your own risk. Always backup your data before using cleanup features.

