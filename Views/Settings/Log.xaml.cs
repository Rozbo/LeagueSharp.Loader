// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Log.xaml.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Views.Settings
{
    #region

    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    using LeagueSharp.Loader.Data;

    #endregion

    /// <summary>
    ///     Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : UserControl
    {
        public Log()
        {
            this.InitializeComponent();
            this.LogsDataGrid.ItemsSource = Logs.MainLog.Items;
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Directories.LogsDirectory))
            {
                Process.Start(Directories.LogsDirectory);
            }
        }
    }
}