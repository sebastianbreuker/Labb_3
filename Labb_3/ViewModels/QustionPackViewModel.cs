using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;


namespace Labb_3.ViewModels
{
    internal class QuestionPackViewModel : ViewModelBase
    {
        private readonly QuestionPack _model;

        public QuestionPackViewModel(QuestionPack model)
        {
            _model = model;
            Questions = new ObservableCollection<Question>(_model.Questions);
            Questions.CollectionChanged += Questions_CollectionChanged;
        }

        private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                foreach (Question question in e.NewItems) _model.Questions.Add(question);

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                foreach (Question question in e.OldItems) _model.Questions.Remove(question);

            if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems != null && e.NewItems != null)
                _model.Questions[e.OldStartingIndex] = (Question)e.NewItems[0]!;

            if (e.Action == NotifyCollectionChangedAction.Reset)
                _model.Questions.Clear();    
        }

        public string Name
        {
            get => _model.Name;
            set
            {
                _model.Name = value;
                RaisePropertyChanged();
            }
        }

        public Difficulty Difficulty
        {
            get => _model.Difficulty;
            set
            {
                _model.Difficulty = value;
                RaisePropertyChanged();
            }
        }

        public int TimeLimitInSeconds
        {
            get => _model.TimeLimitInSeconds;
            set
            {
                _model.TimeLimitInSeconds = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Question> Questions { get; set; }
    }
}
