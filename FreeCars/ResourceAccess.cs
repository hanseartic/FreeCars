using System.ComponentModel;
using FreeCars.Resources;

namespace FreeCars {
    public class ResourceAccess : INotifyPropertyChanged {
        private static Strings strings;
        public Strings LocalizedStrings {
            get { return strings; }
            set { OnPropertyChanged("LocalizedStrings"); }
        }
        public ResourceAccess() {
            strings = new Strings();
        }
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
