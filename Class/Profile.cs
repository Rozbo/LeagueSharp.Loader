// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Profile.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    #region

    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    #endregion

    public class Profile : INotifyPropertyChanged
    {
        private ObservableCollection<LeagueSharpAssembly> _installedAssemblies;

        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LeagueSharpAssembly> InstalledAssemblies
        {
            get
            {
                return this._installedAssemblies;
            }

            set
            {
                this._installedAssemblies = value;
                this.OnPropertyChanged();
            }
        }

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

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}