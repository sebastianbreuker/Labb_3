using Labb_3.Command;
using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Labb_3.Views;

namespace Labb_3.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;
        private QuestionPackViewModel? _hookedPack;

        public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            AddQuestionCommand = new DelegateCommand(_ => AddQuestion());
            RemoveQuestionCommand = new DelegateCommand(_ => RemoveQuestion(), _ => CanRemoveQuestion());
            OpenPackOptionsCommand = new DelegateCommand(_ => OpenPackOptions(), _ => CanOpenPackOptions());

            if (_mainWindowViewModel is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += MainWindowViewModel_PropertyChanged;
            }

            HookActivePack(ActivePack);
        }

        public QuestionPackViewModel? ActivePack => _mainWindowViewModel?.ActivePack;
        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand OpenPackOptionsCommand { get; }

        private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.ActivePack))
            {
                HookActivePack(ActivePack);
                RaisePropertyChanged(nameof(ActivePack));
                RemoveQuestionCommand.RaiseCanExecuteChanged();
                OpenPackOptionsCommand.RaiseCanExecuteChanged();
            }
        }

        private void HookActivePack(QuestionPackViewModel? packToHook)
        {
            if (_hookedPack is INotifyPropertyChanged oldInpc)
            {
                oldInpc.PropertyChanged -= ActivePack_PropertyChanged;
            }

            _hookedPack = packToHook;

            if (_hookedPack is INotifyPropertyChanged newInpc)
            {
                newInpc.PropertyChanged += ActivePack_PropertyChanged;
            }

            RemoveQuestionCommand.RaiseCanExecuteChanged();
            OpenPackOptionsCommand.RaiseCanExecuteChanged();
        }

        private void ActivePack_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(QuestionPackViewModel.SelectedQuestion))
            {
                RemoveQuestionCommand.RaiseCanExecuteChanged();
            }
        }

        private void AddQuestion()
        {
            if (ActivePack is null && _mainWindowViewModel != null)
            {
                var newPackVm = new QuestionPackViewModel(new QuestionPack());
                _mainWindowViewModel.Packs.Add(newPackVm);
                _mainWindowViewModel.ActivePack = newPackVm;
            }

            var question = new Question
            {
                Text = "New Question",
                Options = new List<AnswerOption>
                {
                    new AnswerOption { Text = "", isCorrect = true },
                    new AnswerOption { Text = "", isCorrect = false },
                    new AnswerOption { Text = "", isCorrect = false },
                    new AnswerOption { Text = "", isCorrect = false },
                }
            };

            if (ActivePack != null)
            {
                ActivePack.Questions.Add(question);
                ActivePack.SelectedQuestion = question;
                _mainWindowViewModel?.PersistPacks();
            }
        }

        private bool CanRemoveQuestion()
        {
            return ActivePack != null && ActivePack.SelectedQuestion != null;
        }

        private void RemoveQuestion()
        {
            if (ActivePack == null || ActivePack.SelectedQuestion == null) return;

            var questions = ActivePack.Questions;
            var selected = ActivePack.SelectedQuestion;
            var index = questions.IndexOf(selected);
            if (index < 0) return;

            questions.RemoveAt(index);

            if (questions.Count == 0)
            {
                ActivePack.SelectedQuestion = null;
            }
            else
            {
                var newIndex = Math.Min(index, questions.Count - 1);
                ActivePack.SelectedQuestion = questions[newIndex];
            }

            RemoveQuestionCommand.RaiseCanExecuteChanged();
            _mainWindowViewModel?.PersistPacks();
        }

        private bool CanOpenPackOptions()
        {
            return ActivePack != null;
        }

        private void OpenPackOptions()
        {
            if (ActivePack == null) return;

            var dlg = new PackOptionsDialog
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = ActivePack
            };
            dlg.ShowDialog();
            _mainWindowViewModel?.PersistPacks();
        }
    }
}
