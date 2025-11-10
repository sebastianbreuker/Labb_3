using Labb_3.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Labb_3.ViewModels
{
    class PlayerViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        public DelegateCommand SetPackNameCommand { get; }
        public QuestionPackViewModel? ActivePack { get => _mainWindowViewModel?.ActivePack; }
        public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            this._mainWindowViewModel = mainWindowViewModel;

            SetPackNameCommand = new DelegateCommand(SetPackName, CanSetPackName);
            DemoText = string.Empty;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2.0);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            ActivePack.Name += "+";
        }

        private string _demoText;

        public string DemoText
        {
            get { return _demoText; }
            set
            {
                _demoText = value;
                RaisePropertyChanged();
                SetPackNameCommand.RaiseCanExecuteChanged();
            }
        }


        private bool CanSetPackName(object? arg)
        {
            return DemoText.Length > 0;
        }

        private void SetPackName(object? obj)
        {
            ActivePack.Name = DemoText;
        }
    }
}
