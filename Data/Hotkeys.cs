// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hotkeys.cs" company="LeagueSharp.Loader">
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
    public class Hotkeys : INotifyPropertyChanged
    {
        private ObservableCollection<HotkeyEntry> _selectedHotkeys;

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlArrayItem("SelectedHotkeys", IsNullable = true)]
        public ObservableCollection<HotkeyEntry> SelectedHotkeys
        {
            get
            {
                return this._selectedHotkeys;
            }

            set
            {
                this._selectedHotkeys = value;
                this.OnPropertyChanged("Hotkeys");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}