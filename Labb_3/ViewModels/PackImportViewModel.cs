using Labb_3.Command;
using Labb_3.Models;
using Labb_3.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Labb_3.ViewModels
{
    public class PackImportViewModel : ViewModelBase, IDisposable
    {
        private readonly OpenTriviaService _openTriviaService;
        private readonly CancellationTokenSource _cts = new();

        private TriviaCategory? _selectedCategory;
        private DifficultyOption _selectedDifficulty;
        private int _amount = 10;
        private bool _isBusy;

        public PackImportViewModel(OpenTriviaService openTriviaService)
        {
            _openTriviaService = openTriviaService;

            DifficultyOptions = new ObservableCollection<DifficultyOption>(new[]
            {
                new DifficultyOption("Any", null),
                new DifficultyOption("Easy", Difficulty.Easy),
                new DifficultyOption("Medium", Difficulty.Medium),
                new DifficultyOption("Hard", Difficulty.Hard)
            });
            _selectedDifficulty = DifficultyOptions.First();

            ImportCommand = new DelegateCommand(async _ => await ImportAsync(), _ => CanImport());
            CancelCommand = new DelegateCommand(_ =>
            {
                Cancel();
                OnImportCompleted(false);
            });
        }

        public ObservableCollection<TriviaCategory> Categories { get; } = new();
        public ObservableCollection<DifficultyOption> DifficultyOptions { get; }

        public DelegateCommand ImportCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public TriviaCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged();
                ImportCommand.RaiseCanExecuteChanged();
            }
        }

        public DifficultyOption SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                _selectedDifficulty = value;
                RaisePropertyChanged();
            }
        }

        public int Amount
        {
            get => _amount;
            set
            {
                if (value == _amount) return;
                _amount = Math.Clamp(value, 1, 50);
                RaisePropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                RaisePropertyChanged();
                ImportCommand.RaiseCanExecuteChanged();
            }
        }

        public List<Question>? ImportedQuestions { get; private set; }
        public string? LastErrorMessage { get; private set; }

        public event EventHandler<ImportCompletedEventArgs>? ImportCompleted;
        public event EventHandler<string>? ErrorOccurred;

        public async Task LoadCategoriesAsync()
        {
            try
            {
                IsBusy = true;

                Categories.Clear();
                var categories = await _openTriviaService.GetCategoriesAsync(_cts.Token).ConfigureAwait(true);
                foreach (var category in categories.OrderBy(c => c.Name))
                {
                    Categories.Add(category);
                }

                if (Categories.Count == 0)
                {
                    OnErrorOccurred("No categories available from the Open Trivia Database.");
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = "No internet connection. Please check your network connection and try again.";
                LastErrorMessage = errorMessage;
                OnErrorOccurred(errorMessage);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                string errorMessage = "Request timed out. Please check your internet connection and try again.";
                LastErrorMessage = errorMessage;
                OnErrorOccurred(errorMessage);
            }
            catch (OpenTriviaException ex)
            {
                LastErrorMessage = ex.Message;
                OnErrorOccurred(ex.Message);
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred: {ex.Message}";
                LastErrorMessage = errorMessage;
                OnErrorOccurred(errorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanImport()
        {
            return !IsBusy && SelectedCategory != null && Amount >= 1 && Amount <= 50;
        }

        private async Task ImportAsync()
        {
            try
            {
                IsBusy = true;

                var questions = await _openTriviaService.ImportQuestionsAsync(
                    Amount,
                    SelectedCategory,
                    SelectedDifficulty.Difficulty,
                    _cts.Token).ConfigureAwait(true);

                ImportedQuestions = questions;
                LastErrorMessage = null;
                OnImportCompleted(true);
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = "No internet connection. Please check your network connection and try again.";
                LastErrorMessage = errorMessage;
                OnErrorOccurred(errorMessage);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                string errorMessage = "Request timed out. Please check your internet connection and try again.";
                LastErrorMessage = errorMessage;
                OnErrorOccurred(errorMessage);
            }
            catch (OpenTriviaException ex)
            {
                LastErrorMessage = ex.Message;
                OnErrorOccurred(ex.Message);
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred: {ex.Message}";
                LastErrorMessage = errorMessage;
                OnErrorOccurred(errorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnImportCompleted(bool succeeded)
        {
            ImportCompleted?.Invoke(this, new ImportCompletedEventArgs(succeeded, ImportedQuestions));
        }

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, errorMessage);
        }

        public void Cancel()
        {
            _cts.Cancel();
            LastErrorMessage = null;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public record DifficultyOption(string DisplayName, Difficulty? Difficulty);

    public class ImportCompletedEventArgs : EventArgs
    {
        public ImportCompletedEventArgs(bool succeeded, List<Question>? questions)
        {
            Succeeded = succeeded;
            Questions = questions;
        }

        public bool Succeeded { get; }
        public List<Question>? Questions { get; }
    }
}

