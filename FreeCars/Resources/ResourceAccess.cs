using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace FreeCars.Resources {
    public class ResourceAccess : INotifyPropertyChanged {
        private static Strings strings;
        public Strings LocalizedStrings {
            get { return strings; }
            set { OnPropertyChanged("LocalizedStrings"); }
        }
        public ResourceAccess() {
            strings = new Strings();
            LocalizedStrings = strings;
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
