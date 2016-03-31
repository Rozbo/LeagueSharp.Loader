// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Log.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    #region

    using System.Collections.ObjectModel;
    using System.ComponentModel;

    #endregion

    public static class Logs
    {
        public static Log MainLog = new Log();
    }

    public class Log : INotifyPropertyChanged
    {
        private ObservableCollection<LogItem> _items = new ObservableCollection<LogItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogItem> Items
        {
            get
            {
                return this._items;
            }

            set
            {
                this._items = value;
                this.OnPropertyChanged("Items");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LogItem : INotifyPropertyChanged
    {
        private string _message;

        private string _source;

        private string _status;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Message
        {
            get
            {
                return this._message;
            }

            set
            {
                this._message = value;
                this.OnPropertyChanged("Message");
            }
        }

        public string Source
        {
            get
            {
                return this._source;
            }

            set
            {
                this._source = value;
                this.OnPropertyChanged("Source");
            }
        }

        public string Status
        {
            get
            {
                return this._status;
            }

            set
            {
                this._status = value;
                this.OnPropertyChanged("Status");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum LogStatus
    {
        Info, 

        Error, 

        Debug, 

        Warning
    }
}