// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameSettings.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    using LeagueSharp.Loader.Class;

    using Newtonsoft.Json;

    public class GameSettings : INotifyPropertyChanged
    {
        private string _name;

        private List<string> _posibleValues;

        private string _selectedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        [JsonIgnore]
        public string DisplayName => Utility.GetMultiLanguageText(this._name);

        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                this._name = value;
                this.OnPropertyChanged();
            }
        }

        public List<string> PosibleValues
        {
            get
            {
                return this._posibleValues;
            }

            set
            {
                this._posibleValues = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedValue
        {
            get
            {
                return this._selectedValue;
            }

            set
            {
                this._selectedValue = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}