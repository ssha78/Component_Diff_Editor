# Component Diff Editor

A powerful WPF application for standardizing and managing XML-based milling script components. This tool helps analyze, compare, and optimize dental CAD/CAM milling parameters across multiple XML files.

## 🚀 Features

### 📊 Component Analysis
- **Automatic Component Extraction**: Analyzes 80+ XML files and extracts components like `wing`, `rates`, `gouge_check`, `pattern`, `tool`, etc.
- **Default Template Generation**: Creates standardized default components based on statistical analysis of all files
- **Detailed Reports**: Generates comprehensive analysis reports for each component type

### 🎨 Visual Diff Editor
- **3-Panel Layout**: Side-by-side comparison of default template, selected component, and file list
- **Real-time Similarity Calculation**: Color-coded similarity scores (🟢 >95%, 🟠 80-95%, 🔴 <80%)
- **Interactive Diff Visualization**: Detailed difference analysis with type indicators (Different, Missing, Extra, Identical)
- **Batch Operations**: Apply changes to multiple files simultaneously

### 🧩 Component-Based Recipe System
- **Modular Components**: Each component (wing, rates, pattern) stored as separate XML templates
- **Recipe Composition**: Mix and match components from different files to create custom configurations
- **Bidirectional Copy**: Copy components from files to templates or vice versa
- **Library Management**: Build a library of proven, optimized components

## 🛠️ Architecture

```
ComponentDiffEditor/
├── Models/                     # Data models
│   └── ComponentComparisonModel.cs
├── Services/                   # Business logic
│   └── ComponentComparisonService.cs
├── ViewModels/                 # MVVM ViewModels
│   └── MainViewModel.cs
├── Converters/                 # XAML value converters
│   └── ValueConverters.cs
├── Tools/                      # Standalone analysis tools
│   ├── ComponentAnalyzer.cs   # Console app for initial analysis
│   └── ComponentAnalyzer.csproj
├── default_components/         # Generated default templates
│   ├── rates_default.xml
│   ├── wing_default.xml
│   └── ...
└── script/                     # Original XML files (80+ files)
    ├── Ceramic_Crown_Fine.xml
    ├── LD_Crown_Normal.xml
    └── ...
```

## 🎯 Use Cases

### 1. Standardization
- Identify outlier configurations across your XML library
- Apply consistent parameters to similar file types
- Create standardized templates for different materials

### 2. Optimization
- Compare successful configurations from different files
- Copy proven parameters between similar scenarios
- Build optimized component libraries

### 3. Material-Specific Tuning
- **Ceramic**: High spindle speed, precise stepover
- **LD (Lithium Disilicate)**: Balanced parameters
- **PMMA**: Lower speeds, higher feed rates
- **Zirconia**: Specialized hard material settings

## 🔧 Getting Started

### Prerequisites
- .NET 8.0 or later
- Windows with WPF support

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/ssha78/Component_Diff_Editor.git
   cd Component_Diff_Editor
   ```

2. Build the project:
   ```bash
   dotnet build ComponentDiffEditor.csproj
   ```

3. Run the application:
   ```bash
   dotnet run --project ComponentDiffEditor.csproj
   ```

### First-Time Setup
1. **Generate Default Components**: Run the ComponentAnalyzer tool first:
   ```bash
   cd Tools
   dotnet run --project ComponentAnalyzer.csproj
   ```
   This creates default component templates in the `default_components/` folder.

2. **Launch the Editor**: Start the main WPF application and use the "🔍 Load" button to analyze your XML files.

## 📖 Usage Guide

### Basic Workflow
1. **Select Component Type**: Choose from wing, rates, gouge_check, pattern, etc.
2. **Load Files**: Click "🔍 Load" to analyze all XML files
3. **Review Similarities**: Files with low similarity scores are automatically selected
4. **Apply Changes**:
   - Use "📋 Copy From" to adopt settings from specific files
   - Use "▲ Apply To" for individual file updates
   - Use "🚀 Apply Selected" for batch operations

### Advanced Features
- **Template Editing**: Modify default templates directly in the XML editor
- **Selective Application**: Choose which files to update based on similarity scores
- **Backup System**: Automatic .backup file creation before modifications

## 🔍 Component Types

| Component | Description | Key Parameters |
|-----------|-------------|----------------|
| **rates** | Feed speeds and percentages | feed, plunge, retract, up_percentage |
| **wing** | Wing cutting parameters | width, inside_offset, tolerance |
| **gouge_check** | Collision detection settings | status, check_shaft, strategy |
| **pattern** | Cutting patterns and stepover | type, stepover, cut_method |
| **tool** | Tool specifications | external (tool number) |

## 📊 File Naming Convention

The system recognizes these file patterns:
```
{Material}_{Type}_{Quality}.xml

Materials: Ceramic, LD, PMMA, Zirconia, SinteredZir
Types: Crown, Bridge, Abutment, Cross, Cube, etc.
Quality: Fine, Normal, Sync
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-feature`
3. Commit changes: `git commit -am 'Add new feature'`
4. Push to branch: `git push origin feature/new-feature`
5. Submit a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Links

- **Repository**: https://github.com/ssha78/Component_Diff_Editor
- **Issues**: https://github.com/ssha78/Component_Diff_Editor/issues

## 🎉 Acknowledgments

- Built with WPF and .NET 8.0
- Uses AvalonEdit for XML syntax highlighting
- MVVM pattern with Microsoft.Toolkit.Mvvm
- Inspired by the need for efficient dental CAD/CAM workflow optimization