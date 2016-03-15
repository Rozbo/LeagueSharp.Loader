// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hotkeys.xaml.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Views.Settings
{
    #region

    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using LeagueSharp.Loader.Data;

    #endregion

    public partial class Hotkeys
    {
        public Hotkeys()
        {
            this.InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.HotkeysDataGrid.Items.Cast<HotkeyEntry>())
            {
                item.Hotkey = item.DefaultKey;
            }
        }

        private void Hotkeys_OnKeyUp(object sender, KeyEventArgs e)
        {
            var item = this.HotkeysDataGrid.SelectedItem;
            if (item != null)
            {
                ((HotkeyEntry)item).Hotkey = e.Key;
            }
        }
    }
}