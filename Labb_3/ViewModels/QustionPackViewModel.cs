using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Labb_3.ViewModels
{
    public class QuestionPackViewModel : ViewModelBase
    {
        private readonly QuestionPack _model;
        private readonly HashSet<Question> _hookedQuestions = new();

        public QuestionPackViewModel(QuestionPack model)
        {
            _model = model;
            Questions = new ObservableCollection<Question>(_model.Questions);
            Questions.CollectionChanged += Questions_CollectionChanged;

            foreach (var question in Questions)
            {
                HookQuestion(question);
            }
        }

        private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (Question question in e.NewItems)
                {
                    _model.Questions.Add(question);
                    HookQuestion(question);
                }

                OnPackChanged();
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (Question question in e.OldItems)
                {
                    _model.Questions.Remove(question);
                    UnhookQuestion(question);
                }

                OnPackChanged();
            }

            if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems != null && e.NewItems != null)
            {
                foreach (Question question in e.OldItems)
                {
                    UnhookQuestion(question);
                }

                foreach (Question question in e.NewItems)
                {
                    HookQuestion(question);
                }

                _model.Questions[e.OldStartingIndex] = (Question)e.NewItems[0]!;
                OnPackChanged();
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var question in _hookedQuestions.ToList())
                {
                    UnhookQuestion(question);
                }

                _model.Questions.Clear();
                OnPackChanged();
            }
        }

        public string Name
        {
            get => _model.Name;
            set
            {
                _model.Name = value;
                RaisePropertyChanged();
                OnPackChanged();
            }
        }

        public Difficulty Difficulty
        {
            get => _model.Difficulty;
            set
            {
                _model.Difficulty = value;
                RaisePropertyChanged();
                OnPackChanged();
            }
        }

        public int TimeLimitInSeconds
        {
            get => _model.TimeLimitInSeconds;
            set
            {
                _model.TimeLimitInSeconds = value;
                RaisePropertyChanged();
                OnPackChanged();
            }
        }

        public ObservableCollection<Question> Questions { get; }

        private Question? _selectedQuestion;
        public Question? SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                RaisePropertyChanged();
            }
        }

        public QuestionPack ToModel() => _model;

        public event EventHandler? PackChanged;

        private void OnPackChanged() => PackChanged?.Invoke(this, EventArgs.Empty);

        private void HookQuestion(Question question)
        {
            if (_hookedQuestions.Contains(question)) return;

            question.PropertyChanged += Question_PropertyChanged;
            foreach (var option in question.Options)
            {
                option.PropertyChanged += AnswerOption_PropertyChanged;
            }
            _hookedQuestions.Add(question);
        }

        private void UnhookQuestion(Question question)
        {
            if (!_hookedQuestions.Remove(question)) return;

            question.PropertyChanged -= Question_PropertyChanged;
            foreach (var option in question.Options)
            {
                option.PropertyChanged -= AnswerOption_PropertyChanged;
            }
        }

        private void Question_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPackChanged();
        }

        private void AnswerOption_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPackChanged();
        }
    }
}
