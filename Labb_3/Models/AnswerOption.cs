using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Labb_3.Models
{
    public class AnswerOption : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private bool _isCorrect;

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

        public bool isCorrect
        {
            get => _isCorrect;
            set
            {
                if (_isCorrect == value) return;
                _isCorrect = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
