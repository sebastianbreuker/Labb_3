using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();

		private QuestionPackViewModel _activepack;

		public QuestionPackViewModel ActivePack
		{
			get => _activepack;
			set {
				_activepack = value;
				RaisePropertyChanged();
                PlayerViewModel.RaisePropertyChanged(nameof(PlayerViewModel.ActivePack));
			}
		}
        public PlayerViewModel PlayerViewModel { get; }
        public ConfigurationViewModel? ConfigurationViewModel { get; }
        public MainWindowViewModel()
        {
            PlayerViewModel = new PlayerViewModel(this);
            ConfigurationViewModel = new ConfigurationViewModel(this);

            
            
        }

    }
}
