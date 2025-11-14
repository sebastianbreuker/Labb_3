using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Labb_3.Models
{
    public class Question : INotifyPropertyChanged
    {
        private string _text = string.Empty;

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged();
            }
        }

        public List<AnswerOption> Options { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
