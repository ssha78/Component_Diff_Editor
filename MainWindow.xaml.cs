using ComponentDiffEditor.ViewModels;
using System.Windows;

namespace ComponentDiffEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}