using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UploaderToHosting
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected bool CanExecuteCommand(object param)
        {
            return param != null && !string.IsNullOrEmpty(param.ToString());
        }

        protected internal void RefreshProperty(string propertyName)
        {
            OnPropertyChanges(propertyName);
        }

        #region INotifyPropertyChanged Members
        public void OnPropertyChanges(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
