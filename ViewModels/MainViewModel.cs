using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ComponentDiffEditor.Models;
using ComponentDiffEditor.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System;
using System.Windows;
using System.Threading.Tasks;

namespace ComponentDiffEditor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ComponentComparisonService _comparisonService;

        [ObservableProperty]
        private string _scriptFolderPath = @"C:\Users\SSHA\claude_code\my_project\script_generater\script";

        [ObservableProperty]
        private string _defaultComponentsPath = @"C:\Users\SSHA\claude_code\my_project\script_generater\default_components";

        [ObservableProperty]
        private string _selectedComponentType = "rates";

        [ObservableProperty]
        private string _defaultComponentXml = string.Empty;

        [ObservableProperty]
        private string _currentComponentXml = string.Empty;

        [ObservableProperty]
        private ComponentFile? _selectedComponentFile;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusText = "Ready";

        public ObservableCollection<string> ComponentTypes { get; } = new();
        public ObservableCollection<ComponentFile> ComponentFiles { get; } = new();
        public ObservableCollection<Difference> CurrentDifferences { get; } = new();

        public ICommand LoadComponentsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SaveDefaultCommand { get; }
        public ICommand ApplyToFileCommand { get; }
        public ICommand ApplyToSelectedFilesCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand CopyFromFileCommand { get; }

        public MainViewModel()
        {
            _comparisonService = new ComponentComparisonService();

            LoadComponentsCommand = new AsyncRelayCommand(LoadComponentsAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            SaveDefaultCommand = new AsyncRelayCommand(SaveDefaultAsync);
            ApplyToFileCommand = new RelayCommand<ComponentFile>(ApplyToFile);
            ApplyToSelectedFilesCommand = new AsyncRelayCommand(ApplyToSelectedFilesAsync);
            SelectAllCommand = new RelayCommand(() => SetAllSelected(true));
            DeselectAllCommand = new RelayCommand(() => SetAllSelected(false));
            CopyFromFileCommand = new RelayCommand<ComponentFile>(CopyFromFile);

            // Initialize
            foreach (var componentType in _comparisonService.GetAvailableComponentTypes())
            {
                ComponentTypes.Add(componentType);
            }

            PropertyChanged += OnPropertyChanged;
            LoadDefaultComponent();
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedComponentType))
            {
                LoadDefaultComponent();
            }
            else if (e.PropertyName == nameof(SelectedComponentFile))
            {
                LoadCurrentComponent();
            }
        }

        private void LoadDefaultComponent()
        {
            try
            {
                var defaultPath = Path.Combine(DefaultComponentsPath, $"{SelectedComponentType}_default.xml");
                if (File.Exists(defaultPath))
                {
                    DefaultComponentXml = File.ReadAllText(defaultPath);
                }
                else
                {
                    DefaultComponentXml = $"<!-- Default {SelectedComponentType} component not found -->";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error loading default component: {ex.Message}";
            }
        }

        private void LoadCurrentComponent()
        {
            try
            {
                if (SelectedComponentFile?.Component != null)
                {
                    CurrentComponentXml = SelectedComponentFile.Component.ToString();

                    CurrentDifferences.Clear();
                    foreach (var diff in SelectedComponentFile.Differences)
                    {
                        CurrentDifferences.Add(diff);
                    }
                }
                else
                {
                    CurrentComponentXml = "<!-- No component selected -->";
                    CurrentDifferences.Clear();
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error loading current component: {ex.Message}";
            }
        }

        private async Task LoadComponentsAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Loading components...";

                ComponentFiles.Clear();

                if (!Directory.Exists(ScriptFolderPath))
                {
                    StatusText = "Script folder not found";
                    return;
                }

                var defaultPath = Path.Combine(DefaultComponentsPath, $"{SelectedComponentType}_default.xml");

                await Task.Run(() =>
                {
                    var components = _comparisonService.CompareWithDefault(
                        SelectedComponentType,
                        defaultPath,
                        ScriptFolderPath);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var component in components)
                        {
                            ComponentFiles.Add(component);
                        }
                    });
                });

                StatusText = $"Loaded {ComponentFiles.Count} components for {SelectedComponentType}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error loading components: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAsync()
        {
            await LoadComponentsAsync();
        }

        private async Task SaveDefaultAsync()
        {
            try
            {
                StatusText = "Saving default component...";

                var defaultPath = Path.Combine(DefaultComponentsPath, $"{SelectedComponentType}_default.xml");

                if (!Directory.Exists(DefaultComponentsPath))
                {
                    Directory.CreateDirectory(DefaultComponentsPath);
                }

                await File.WriteAllTextAsync(defaultPath, DefaultComponentXml);

                StatusText = "Default component saved";
            }
            catch (Exception ex)
            {
                StatusText = $"Error saving default: {ex.Message}";
            }
        }

        private void ApplyToFile(ComponentFile? file)
        {
            if (file == null) return;

            try
            {
                var defaultComponent = XDocument.Parse(DefaultComponentXml).Root;
                if (defaultComponent != null)
                {
                    _comparisonService.ApplyDefaultToFile(defaultComponent, file.FilePath, SelectedComponentType);
                    StatusText = $"Applied default to {file.DisplayName}";

                    // 업데이트된 similarity 계산
                    var updatedSimilarity = _comparisonService.CalculateSimilarity(
                        defaultComponent,
                        file.Component!);
                    file.Similarity = updatedSimilarity;
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error applying to {file.DisplayName}: {ex.Message}";
            }
        }

        private async Task ApplyToSelectedFilesAsync()
        {
            try
            {
                IsLoading = true;
                var selectedFiles = ComponentFiles.Where(f => f.IsSelected).ToList();
                StatusText = $"Applying to {selectedFiles.Count} files...";

                var defaultComponent = XDocument.Parse(DefaultComponentXml).Root;
                if (defaultComponent == null)
                {
                    StatusText = "Invalid default component XML";
                    return;
                }

                int successCount = 0;
                await Task.Run(() =>
                {
                    foreach (var file in selectedFiles)
                    {
                        try
                        {
                            _comparisonService.ApplyDefaultToFile(defaultComponent, file.FilePath, SelectedComponentType);

                            // UI 스레드에서 similarity 업데이트
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var updatedSimilarity = _comparisonService.CalculateSimilarity(
                                    defaultComponent,
                                    file.Component!);
                                file.Similarity = updatedSimilarity;
                            });

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error applying to {file.DisplayName}: {ex.Message}");
                        }
                    }
                });

                StatusText = $"Applied default to {successCount}/{selectedFiles.Count} files";
            }
            catch (Exception ex)
            {
                StatusText = $"Batch apply error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetAllSelected(bool selected)
        {
            foreach (var file in ComponentFiles)
            {
                file.IsSelected = selected;
            }
            StatusText = selected ? "All files selected" : "All files deselected";
        }

        private void CopyFromFile(ComponentFile? file)
        {
            if (file?.Component == null) return;

            try
            {
                DefaultComponentXml = file.Component.ToString();
                StatusText = $"Copied component from {file.DisplayName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error copying from {file.DisplayName}: {ex.Message}";
            }
        }
    }
}