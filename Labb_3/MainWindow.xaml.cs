using Labb_3.Models;
using Labb_3.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Labb_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isFullscreen;
        private WindowState _storedWindowState;
        private WindowStyle _storedWindowStyle;
        private ResizeMode _storedResizeMode;

        public MainWindow()
        {
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new MainWindowViewModel();
            }
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void ToggleFullscreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_isFullscreen)
            {
                _storedWindowState = WindowState;
                _storedWindowStyle = WindowStyle;
                _storedResizeMode = ResizeMode;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                Topmost = true;
            }
            else
            {
                Topmost = false;
                WindowStyle = _storedWindowStyle;
                ResizeMode = _storedResizeMode;
                WindowState = _storedWindowState;
            }

            _isFullscreen = !_isFullscreen;
        }
    }
}