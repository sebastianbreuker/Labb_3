using Labb_3.Models;
using Labb_3.Command;
using Labb_3.Services;
using Labb_3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Labb_3.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly JsonStorageService? _storageService;
        private readonly HashSet<QuestionPackViewModel> _hookedPacks = new();
        private readonly OpenTriviaService _openTriviaService = new();

        public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();

		private QuestionPackViewModel? _activepack;
        private ApplicationView _activeView = ApplicationView.Configuration;

		public QuestionPackViewModel? ActivePack
		{
			get => _activepack;
			set {
				if (_activepack == value) return;
				_activepack = value;
				RaisePropertyChanged();
                PlayerViewModel.RaisePropertyChanged(nameof(PlayerViewModel.ActivePack));
                PlayerViewModel.StopQuiz();
                ConfigurationViewModel?.RemoveQuestionCommand.RaiseCanExecuteChanged();
                ConfigurationViewModel?.OpenPackOptionsCommand.RaiseCanExecuteChanged();
                DeleteActivePackCommand?.RaiseCanExecuteChanged();
                ImportQuestionsCommand?.RaiseCanExecuteChanged();
			}
		}

        public ApplicationView ActiveView
        {
            get => _activeView;
            set
            {
                _activeView = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsConfigurationViewVisible));
                RaisePropertyChanged(nameof(IsPlayerViewVisible));
            }
        }

        public bool IsConfigurationViewVisible => ActiveView == ApplicationView.Configuration;
        public bool IsPlayerViewVisible => ActiveView == ApplicationView.Player;

        public PlayerViewModel PlayerViewModel { get; }
        public ConfigurationViewModel? ConfigurationViewModel { get; }

        public DelegateCommand NewPackCommand { get; }
        public DelegateCommand DeleteActivePackCommand { get; }
        public DelegateCommand SelectPackCommand { get; }
        public DelegateCommand ShowConfigurationViewCommand { get; }
        public DelegateCommand ShowPlayerViewCommand { get; }
        public DelegateCommand ImportQuestionsCommand { get; }

        private bool _isImportingTrivia;

        private static bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public MainWindowViewModel()
        {
            Packs.CollectionChanged += Packs_CollectionChanged;

            PlayerViewModel = new PlayerViewModel(this);
            ConfigurationViewModel = new ConfigurationViewModel(this);

            if (!IsDesignMode)
            {
                _storageService = new JsonStorageService();
                var storedPacks = _storageService.LoadPacks();
                if (storedPacks.Count > 0)
                {
                    foreach (var packModel in storedPacks)
                    {
                        var vm = new QuestionPackViewModel(packModel);
                        HookPack(vm);
                        Packs.Add(vm);
                    }
                }
            }

            if (Packs.Count == 0)
            {
                var defaultPack = new QuestionPackViewModel(new QuestionPack { Name = "New pack" });
                HookPack(defaultPack);
                Packs.Add(defaultPack);
            }

            ActivePack = Packs.FirstOrDefault();

            NewPackCommand = new DelegateCommand(_ => CreateNewPack());
            DeleteActivePackCommand = new DelegateCommand(_ => DeleteActivePack(), _ => CanDeleteActivePack());
            SelectPackCommand = new DelegateCommand(p => SelectPack(p as QuestionPackViewModel));
            ShowConfigurationViewCommand = new DelegateCommand(_ =>
            {
                PlayerViewModel.StopQuiz();
                ActiveView = ApplicationView.Configuration;
            });
            ShowPlayerViewCommand = new DelegateCommand(_ =>
            {
                if (ActivePack == null)
                {
                    MessageBox.Show("Select or create a question pack before playing.", "No Pack Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                    ActiveView = ApplicationView.Configuration;
                    return;
                }

                if (ActivePack.Questions.Count == 0)
                {
                    MessageBox.Show("Add at least one question to the pack before starting the quiz.", "No Questions Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    ActiveView = ApplicationView.Configuration;
                    return;
                }

                PlayerViewModel.StartQuiz();
                ActiveView = ApplicationView.Player;
            });
            ImportQuestionsCommand = new DelegateCommand(_ => ImportQuestions(), _ => !_isImportingTrivia && ActivePack != null);

            RaisePropertyChanged(nameof(IsConfigurationViewVisible));
            RaisePropertyChanged(nameof(IsPlayerViewVisible));

            PersistPacks();
        }

        private void Packs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (QuestionPackViewModel pack in e.NewItems)
                {
                    HookPack(pack);
                }
            }

            if (e.OldItems != null)
            {
                foreach (QuestionPackViewModel pack in e.OldItems)
                {
                    UnhookPack(pack);
                }
            }

            PersistPacks();
        }

        private void CreateNewPack()
        {
            string baseName = "New pack";
            string uniqueName = baseName;
            int suffix = 1;
            while (Packs.Any(p => p.Name.Equals(uniqueName, StringComparison.OrdinalIgnoreCase)))
            {
                uniqueName = $"{baseName} {suffix++}";
            }

            var pack = new QuestionPackViewModel(new QuestionPack { Name = uniqueName });
            HookPack(pack);
            Packs.Add(pack);
            ActivePack = pack;
            DeleteActivePackCommand.RaiseCanExecuteChanged();

            if (ActiveView == ApplicationView.Player)
            {
                PlayerViewModel.StartQuiz();
            }
            ActiveView = ApplicationView.Configuration;

            var dlg = new PackOptionsDialog
            {
                Owner = Application.Current?.MainWindow,
                DataContext = pack
            };
            dlg.ShowDialog();
            PersistPacks();
        }

        private bool CanDeleteActivePack()
        {
            return ActivePack != null && Packs.Contains(ActivePack);
        }

        private void DeleteActivePack()
        {
            if (!CanDeleteActivePack()) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the pack \"{ActivePack.Name}\"? This cannot be undone.",
                "Delete Question Pack",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            var index = Packs.IndexOf(ActivePack);
            Packs.Remove(ActivePack);

            if (Packs.Count == 0)
            {
                ActivePack = null;
                ActiveView = ApplicationView.Configuration;
            }
            else
            {
                var newIndex = Math.Max(0, Math.Min(index, Packs.Count - 1));
                ActivePack = Packs[newIndex];
            }

            DeleteActivePackCommand.RaiseCanExecuteChanged();
            PersistPacks();
        }

        private void SelectPack(QuestionPackViewModel pack)
        {
            if (pack == null) return;
            ActivePack = pack;
            DeleteActivePackCommand.RaiseCanExecuteChanged();
        }

        private void ImportQuestions()
        {
            if (_isImportingTrivia) return;

            try
            {
                _isImportingTrivia = true;
                ImportQuestionsCommand.RaiseCanExecuteChanged();

                var importViewModel = new PackImportViewModel(_openTriviaService);
                var dialog = new PackImportDialog
                {
                    Owner = Application.Current?.MainWindow,
                    DataContext = importViewModel
                };

                var result = dialog.ShowDialog();
                var importedQuestions = importViewModel.ImportedQuestions;

                if (result == true && importedQuestions != null && importedQuestions.Count > 0)
                {
                    foreach (var question in importedQuestions)
                    {
                        ActivePack.Questions.Add(question);
                    }
                    PersistPacks();

                    MessageBox.Show(
                        $"Successfully imported {importedQuestions.Count} questions into \"{ActivePack.Name}\".",
                        "Import Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else if (result == false && importViewModel.LastErrorMessage is { Length: > 0 } errorMessage)
                {
                    // Show last status message when the dialog was dismissed with an error.
                    MessageBox.Show(
                        errorMessage,
                        "Import Trivia Pack",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (OpenTriviaException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Import Trivia Pack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unexpected error while importing trivia questions:\n{ex.Message}",
                    "Import Trivia Pack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isImportingTrivia = false;
                ImportQuestionsCommand.RaiseCanExecuteChanged();
            }
        }

        public void PersistPacks()
        {
            if (IsDesignMode || _storageService is null) return;

            var models = Packs.Select(p => p.ToModel()).ToList();
            try
            {
                _storageService.SavePacks(models);
            }
            catch
            {
                // ignore persistence errors
            }
        }

        private void HookPack(QuestionPackViewModel pack)
        {
            if (_hookedPacks.Contains(pack)) return;

            pack.PackChanged += Pack_PackChanged;
            _hookedPacks.Add(pack);
        }

        private void UnhookPack(QuestionPackViewModel pack)
        {
            if (_hookedPacks.Remove(pack))
            {
                pack.PackChanged -= Pack_PackChanged;
            }
        }

        private void Pack_PackChanged(object? sender, EventArgs e)
        {
            PersistPacks();
        }

        public enum ApplicationView
        {
            Configuration,
            Player
        }
    }
}
