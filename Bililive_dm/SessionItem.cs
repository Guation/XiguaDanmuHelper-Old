using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bililive_dm.Annotations;

namespace Bililive_dm
{
    public class SessionItem : INotifyPropertyChanged
    {
        private string _item;
        private decimal _num;
        private string _userName;

        public string UserName
        {
            get => _userName;
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Item
        {
            get => _item;
            set
            {
                if (value == _item) return;
                _item = value;
                OnPropertyChanged();
            }
        }

        public decimal num
        {
            get => _num;
            set
            {
                if (value == _num) return;
                _num = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}