using Labb_3.Command;
using Labb_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3.ViewModels
{
    class ConfigurationViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;
        

        public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            this._mainWindowViewModel = mainWindowViewModel;

            AddQuestionCommand = new DelegateCommand(_ => AddQuestion());
        }
        public DelegateCommand AddQuestionCommand { get; }
        private void AddQuestion()
        {
            var question = new Question
            {
                Text = "New Question",
                Options = new List<AnswerOption>()
                {
                    new AnswerOption {Text = "", isCorrect = true},
                    new AnswerOption {Text = "", isCorrect = false},
                    new AnswerOption {Text = "", isCorrect = false},
                    new AnswerOption {Text = "", isCorrect = false},
                }
            };
        }
    }
}
