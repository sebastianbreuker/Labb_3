using Labb_3.Command;
using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace Labb_3.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;
        private readonly Random _random = new();
        private readonly DispatcherTimer _questionTimer;
        private DispatcherTimer? _postAnswerTimer;

        private List<Question> _quizQuestions = new();
        private int _currentQuestionIndex = -1;
        private int _correctAnswers;
        private bool _isQuestionActive;
        private bool _isQuizFinished;

        public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            SelectAnswerCommand = new DelegateCommand(param => OnAnswerSelected(param as AnswerOptionViewModel),
                param => param is AnswerOptionViewModel && AreAnswersEnabled);
            RestartQuizCommand = new DelegateCommand(_ => RestartQuiz(), _ => IsQuizFinished);

            _questionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _questionTimer.Tick += QuestionTimer_Tick;

            if (_mainWindowViewModel is not null)
            {
                _mainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
            }
        }

        private Question? _currentQuestion;
        public Question? CurrentQuestion
        {
            get => _currentQuestion;
            private set
            {
                _currentQuestion = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CurrentQuestionText));
            }
        }

        public string CurrentQuestionText => CurrentQuestion?.Text ?? string.Empty;

        public ObservableCollection<AnswerOptionViewModel> CurrentAnswers { get; } = new();

        private bool _areAnswersEnabled;
        public bool AreAnswersEnabled
        {
            get => _areAnswersEnabled;
            private set
            {
                _areAnswersEnabled = value;
                RaisePropertyChanged();
                SelectAnswerCommand.RaiseCanExecuteChanged();
            }
        }

        private int _timeRemaining;
        public int TimeRemaining
        {
            get => _timeRemaining;
            private set
            {
                _timeRemaining = value;
                RaisePropertyChanged();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsStatusMessageVisible));
            }
        }

        public bool IsStatusMessageVisible => !string.IsNullOrWhiteSpace(StatusMessage);

        private int _questionNumber;
        public int QuestionNumber
        {
            get => _questionNumber;
            private set
            {
                _questionNumber = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(QuestionProgressText));
            }
        }

        public string QuestionProgressText =>
            _quizQuestions.Count == 0 || QuestionNumber == 0
                ? string.Empty
                : $"Question {QuestionNumber} of {_quizQuestions.Count}";

        public DelegateCommand SelectAnswerCommand { get; }
        public DelegateCommand RestartQuizCommand { get; }

        public bool IsQuizFinished
        {
            get => _isQuizFinished;
            private set
            {
                if (_isQuizFinished == value) return;
                _isQuizFinished = value;
                RaisePropertyChanged();
                RestartQuizCommand.RaiseCanExecuteChanged();
            }
        }

        public QuestionPackViewModel? ActivePack => _mainWindowViewModel?.ActivePack;

        private void MainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.ActivePack))
            {
                StopQuiz();
            }
        }

        public void StartQuiz()
        {
            StopQuiz();

            if (ActivePack == null || ActivePack.Questions.Count == 0)
            {
                StatusMessage = "No questions available in this pack.";
                return;
            }

            StatusMessage = string.Empty;
            _correctAnswers = 0;
            _quizQuestions = ActivePack.Questions.OrderBy(_ => _random.Next()).ToList();
            _currentQuestionIndex = -1;
            QuestionNumber = 0;
            AreAnswersEnabled = true;
            RaisePropertyChanged(nameof(QuestionProgressText));
            IsQuizFinished = false;
            LoadNextQuestion();
        }

        public void StopQuiz()
        {
            _questionTimer.Stop();
            _postAnswerTimer?.Stop();
            AreAnswersEnabled = false;
            _isQuestionActive = false;
            CurrentAnswers.Clear();
            CurrentQuestion = null;
            TimeRemaining = 0;
            QuestionNumber = 0;
            StatusMessage = string.Empty;
            RaisePropertyChanged(nameof(QuestionProgressText));
            IsQuizFinished = false;
        }

        private void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isQuestionActive) return;

            TimeRemaining--;
            if (TimeRemaining <= 0)
            {
                _questionTimer.Stop();
                HandleTimeExpired();
            }
        }

        private void HandleTimeExpired()
        {
            AreAnswersEnabled = false;
            _isQuestionActive = false;

            var correctAnswers = CurrentAnswers.Where(a => a.IsCorrect).ToList();
            foreach (var answer in CurrentAnswers)
            {
                if (answer.IsCorrect)
                {
                    answer.State = AnswerRevealState.Correct;
                }
            }

            StatusMessage = correctAnswers.Count > 0
                ? $"Time's up! Correct answer: {string.Join(", ", correctAnswers.Select(a => a.Text))}"
                : "Time's up!";

            ScheduleNextQuestion();
        }

        private void LoadNextQuestion()
        {
            _postAnswerTimer?.Stop();

            _currentQuestionIndex++;
            if (_currentQuestionIndex >= _quizQuestions.Count)
            {
                FinishQuiz();
                return;
            }

            IsQuizFinished = false;
            CurrentQuestion = _quizQuestions[_currentQuestionIndex];
            QuestionNumber = _currentQuestionIndex + 1;

            var shuffledAnswers = CurrentQuestion.Options
                .Select(option => new AnswerOptionViewModel(option.Text, option.isCorrect))
                .OrderBy(_ => _random.Next())
                .ToList();

            CurrentAnswers.Clear();
            foreach (var answer in shuffledAnswers)
            {
                CurrentAnswers.Add(answer);
            }

            AreAnswersEnabled = true;
            _isQuestionActive = true;
            TimeRemaining = Math.Max(2, ActivePack?.TimeLimitInSeconds ?? 20);
            _questionTimer.Start();

            StatusMessage = string.Empty;
            RaisePropertyChanged(nameof(QuestionProgressText));
        }

        private void OnAnswerSelected(AnswerOptionViewModel? selectedAnswer)
        {
            if (selectedAnswer == null || !_isQuestionActive) return;

            _isQuestionActive = false;
            _questionTimer.Stop();
            AreAnswersEnabled = false;

            if (selectedAnswer.IsCorrect)
            {
                _correctAnswers++;
                selectedAnswer.State = AnswerRevealState.Correct;
                StatusMessage = "Correct!";
            }
            else
            {
                selectedAnswer.State = AnswerRevealState.Incorrect;
                var correctAnswers = CurrentAnswers.Where(a => a.IsCorrect).ToList();
                foreach (var answer in correctAnswers)
                {
                    answer.State = AnswerRevealState.Correct;
                }

                StatusMessage = correctAnswers.Count > 0
                    ? $"Incorrect! Correct answer: {string.Join(", ", correctAnswers.Select(a => a.Text))}"
                    : "Incorrect!";
            }

            ScheduleNextQuestion();
        }

        private void ScheduleNextQuestion()
        {
            _postAnswerTimer?.Stop();
            _postAnswerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            _postAnswerTimer.Tick += (_, __) =>
            {
                _postAnswerTimer.Stop();
                LoadNextQuestion();
            };
            _postAnswerTimer.Start();
        }

        private void FinishQuiz()
        {
            _questionTimer.Stop();
            AreAnswersEnabled = false;
            _isQuestionActive = false;
            CurrentAnswers.Clear();
            CurrentQuestion = null;
            TimeRemaining = 0;
            StatusMessage = $"Quiz finished! You answered {_correctAnswers} out of {_quizQuestions.Count} questions correctly.";
            QuestionNumber = 0;
            RaisePropertyChanged(nameof(QuestionProgressText));
            IsQuizFinished = true;
        }

        private void RestartQuiz()
        {
            if (ActivePack == null || ActivePack.Questions.Count == 0)
            {
                StatusMessage = "No questions available in this pack.";
                return;
            }

            StartQuiz();
        }

        public class AnswerOptionViewModel : ViewModelBase
        {
            public AnswerOptionViewModel(string text, bool isCorrect)
            {
                Text = text;
                IsCorrect = isCorrect;
            }

            public string Text { get; }
            public bool IsCorrect { get; }

            private AnswerRevealState _state = AnswerRevealState.Default;
            public AnswerRevealState State
            {
                get => _state;
                set
                {
                    _state = value;
                    RaisePropertyChanged();
                }
            }
        }

        public enum AnswerRevealState
        {
            Default,
            Correct,
            Incorrect
        }
    }
}
