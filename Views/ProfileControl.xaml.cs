// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfileControl.xaml.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Views
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;

    using MahApps.Metro.Controls.Dialogs;

    /// <summary>
    /// Interaction logic for ProfileControl.xaml
    /// </summary>
    public partial class ProfileControl : INotifyPropertyChanged
    {
        public ProfileControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Config Config => Config.Instance;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void EditProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            this.ShowProfileNameChangeDialog();
        }

        private void NewProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.Profiles.Add(
                new Profile
                {
                    InstalledAssemblies = new ObservableCollection<LeagueSharpAssembly>(), 
                    Name = Utility.GetMultiLanguageText("NewProfile2")
                });

            Config.Instance.SelectedProfile = Config.Instance.Profiles.Last();
        }

        private void ProfilesButton_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ShowProfileNameChangeDialog();
        }

        private void ProfilesButton_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Config.OnPropertyChanged("SearchText");
        }

        private void RemoveProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (Config.Instance.Profiles.Count > 1)
            {
                Config.Instance.Profiles.Remove(this.Config.SelectedProfile);
                Config.Instance.SelectedProfile = Config.Instance.Profiles.First();
            }
            else
            {
                Config.Instance.SelectedProfile.InstalledAssemblies = new ObservableCollection<LeagueSharpAssembly>();
                Config.Instance.SelectedProfile.Name = Utility.GetMultiLanguageText("DefaultProfile");
            }
        }

        private async void ShowProfileNameChangeDialog()
        {
            var result =
                await
                MainWindow.Instance.ShowInputAsync(
                    Utility.GetMultiLanguageText("Rename"), 
                    Utility.GetMultiLanguageText("RenameText"), 
                    new MetroDialogSettings { DefaultText = Config.Instance.SelectedProfile.Name, });

            if (!string.IsNullOrEmpty(result))
            {
                Config.Instance.SelectedProfile.Name = result;
            }
        }
    }
}