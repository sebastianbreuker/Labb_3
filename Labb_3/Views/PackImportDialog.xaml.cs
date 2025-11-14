using Labb_3.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Labb_3
{
    /// <summary>
    /// Interaction logic for PackImportDialog.xaml
    /// </summary>
    public partial class PackImportDialog : Window
    {
        public PackImportDialog()
        {
            InitializeComponent();
            Loaded += PackImportDialog_Loaded;
            Unloaded += PackImportDialog_Unloaded;
        }

        private async void PackImportDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PackImportViewModel vm)
            {
                vm.ImportCompleted += Vm_ImportCompleted;
                vm.ErrorOccurred += Vm_ErrorOccurred;
                await LoadCategoriesAsync(vm);
            }
        }

        private void PackImportDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PackImportViewModel vm)
            {
                vm.ImportCompleted -= Vm_ImportCompleted;
                vm.ErrorOccurred -= Vm_ErrorOccurred;
                vm.Cancel();
                vm.Dispose();
            }
        }

        private async Task LoadCategoriesAsync(PackImportViewModel viewModel)
        {
            await viewModel.LoadCategoriesAsync();
        }

        private void Vm_ErrorOccurred(object? sender, string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(this,
                    errorMessage,
                    "Import Questions",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }

        private void Vm_ImportCompleted(object? sender, ImportCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DialogResult = e.Succeeded;
                Close();
            });
        }
    }
}

