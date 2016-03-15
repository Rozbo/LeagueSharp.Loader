// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigSettings.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlType(AnonymousType = true)]
    public class ConfigSettings : INotifyPropertyChanged
    {
        private ObservableCollection<GameSettings> _gameSettings;

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlArrayItem("GameSettings", IsNullable = true)]
        public ObservableCollection<GameSettings> GameSettings
        {
            get
            {
                return this._gameSettings;
            }

            set
            {
                this._gameSettings = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}