using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KFC.ViewModel {
    abstract public class ViewModelBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ViewModelBase() { }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
