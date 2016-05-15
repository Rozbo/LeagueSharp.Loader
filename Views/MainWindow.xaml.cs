// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;

    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Class.Installer;
    using LeagueSharp.Loader.Data;

    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;

    using Microsoft.Build.Evaluation;

    using PlaySharp.Service.WebService.Model;
    using PlaySharp.Toolkit.Helper;

    using Polly;

    public partial class MainWindow : INotifyPropertyChanged
    {
        public const int TAB_ASSEMBLIES = 2;

        public const int TAB_DATABASE = 4;

        public const int TAB_NEWS = 1;

        public const int TAB_SETTINGS = 3;

        public const int TAB_TOS = 0;

        public readonly BackgroundWorker AssembliesWorker = new BackgroundWorker();

        public bool AssembliesWorkerCancelled;

        private bool checkingForUpdates;

        private bool columnWidthChanging;

        private int rowIndex = -1;

        private string statusString = "?";

        private string updateMessage;

        private bool working;

        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();
            this.DataContext = this;
        }

        public delegate Point GetPosition(IInputElement element);

        public event PropertyChangedEventHandler PropertyChanged;

        public static MainWindow Instance { get; private set; }

        public string BaseUrl => "https://services.joduska.me/api/v2.0";

        public bool CheckingForUpdates
        {
            get
            {
                return this.checkingForUpdates;
            }

            set
            {
                this.checkingForUpdates = value;
                this.OnPropertyChanged();
            }
        }

        public Config Config => Config.Instance;

        public Thread InjectThread { get; set; }

        public string StatusString
        {
            get
            {
                return this.statusString;
            }

            set
            {
                this.statusString = value;
                this.OnPropertyChanged();
            }
        }

        public bool Working
        {
            get
            {
                return this.working;
            }

            set
            {
                this.working = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime LastAccountUpdate { get; set; }

        public void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.Hide();

            if (this.AssembliesWorker.IsBusy && e != null)
            {
                this.AssembliesWorker.CancelAsync();
                e.Cancel = true;
                this.Hide();
                return;
            }

            Config.Save(true);

            try
            {
                this.InjectThread?.Abort();
            }
            catch
            {
                // ignored
            }
        }

        public async Task PrepareAssemblies(
            IEnumerable<LeagueSharpAssembly> assemblies, 
            bool update, 
            bool compile)
        {
            this.Working = true;
            var leagueSharpAssemblies = assemblies as IList<LeagueSharpAssembly> ?? assemblies.ToList();
            Directory.CreateDirectory(Directories.AssembliesDirectory);

            await Task.Factory.StartNew(
                () =>
                {
                    if (update)
                    {
                        var updateList = leagueSharpAssemblies.GroupBy(a => a.SvnUrl).Select(g => g.First()).ToList();

                        Parallel.ForEach(
                            updateList, 
                            new ParallelOptions { MaxDegreeOfParallelism = this.Config.Workers }, 
                            (assembly, state) =>
                            {
                                assembly.Update();
                                if (this.AssembliesWorker.CancellationPending)
                                {
                                    this.AssembliesWorkerCancelled = true;
                                    state.Break();
                                }
                            });
                    }
                });

            try
            {
                var di = new DependencyInstaller(leagueSharpAssemblies.Select(a => a.PathToProjectFile).ToList());
                await di.SatisfyAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Factory.StartNew(
                () =>
                {
                    if (compile)
                    {
                        var list =
                            leagueSharpAssemblies.OrderByDescending(a => a.Type == AssemblyType.Library)
                                                 .ThenByDescending(a => a.Name.StartsWith("LeagueSharp."))
                                                 .ToList();

                        foreach (var assembly in list)
                        {
                            assembly.Compile();

                            if (this.AssembliesWorker.CancellationPending)
                            {
                                this.AssembliesWorkerCancelled = true;
                                break;
                            }
                        }
                    }
                });

            Injection.PrepareDone = true;

            await Task.Factory.StartNew(
                () =>
                {
                    ProjectCollection.GlobalProjectCollection.UnloadAllProjects();

                    if (this.AssembliesWorkerCancelled)
                    {
                        this.Close();
                    }
                });

            if (!this.Config.EnableDebug)
            {
                foreach (var file in Directory.EnumerateFiles(Directories.CoreDirectory, "*.pdb"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                foreach (var file in Directory.EnumerateFiles(Directories.AssembliesDirectory, "*.pdb"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            this.Working = false;
        }

        public async void ShowTextMessage(string title, string message)
        {
            this.Browser.Visibility = Visibility.Hidden;
            this.TosBrowser.Visibility = Visibility.Hidden;

            await this.ShowMessageAsync(title, message);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AssemblyButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Browser.Visibility = Visibility.Hidden;
            this.TosBrowser.Visibility = Visibility.Hidden;

            this.MainTabControl.SelectedIndex = TAB_ASSEMBLIES;
            this.UpdateFilters();
        }

        private async void AssemblyDBButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Browser.Visibility = Visibility.Hidden;
            this.TosBrowser.Visibility = Visibility.Hidden;

            try
            {
                if (Config.Instance.DatabaseAssemblies?.Count == 0)
                {
                    await Updater.UpdateWebService();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            this.MainTabControl.SelectedIndex = TAB_DATABASE;
            this.UpdateFilters();
        }

        private void BaseDataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.columnWidthChanging)
            {
                this.columnWidthChanging = false;

                Config.Instance.ColumnCheckWidth = this.ColumnCheck.Width.DesiredValue;
                Config.Instance.ColumnNameWidth = this.ColumnName.Width.DesiredValue;
                Config.Instance.ColumnTypeWidth = this.ColumnType.Width.DesiredValue;
                Config.Instance.ColumnVersionWidth = this.ColumnVersion.Width.DesiredValue;
                Config.Instance.ColumnLocationWidth = this.ColumnLocation.Width.DesiredValue;
            }
        }

        /// <summary>
        /// Bootstraps this instance.
        /// </summary>
        /// <remarks>
        /// Bootstrap flow:
        /// splash
        /// ui setup
        /// loader/core update
        /// tos
        /// auth
        /// update webservice
        /// compile
        /// remoting/injection
        /// </remarks>
        /// <returns></returns>
        private async Task Bootstrap()
        {
            try
            {
                var splash = new SplashScreen("resources/splash.png");
                this.Visibility = Visibility.Hidden;
                splash.Show(false, true);

                this.Browser.Visibility = Visibility.Hidden;
                this.TosBrowser.Visibility = Visibility.Hidden;
                this.GeneralSettingsItem.IsSelected = true;

                PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.ActualWidthProperty, 
                    typeof(DataGridColumn));

                foreach (var column in this.InstalledAssembliesDataGrid.Columns)
                {
                    pd.AddValueChanged(column, this.ColumnWidthPropertyChanged);
                }

                this.ColumnCheck.Width = Config.Instance.ColumnCheckWidth;
                this.ColumnName.Width = Config.Instance.ColumnNameWidth;
                this.ColumnType.Width = Config.Instance.ColumnTypeWidth;
                this.ColumnVersion.Width = Config.Instance.ColumnVersionWidth;
                this.ColumnLocation.Width = Config.Instance.ColumnLocationWidth;

                this.NewsTabItem.Visibility = Visibility.Hidden;
                this.AssembliesTabItem.Visibility = Visibility.Hidden;
                this.SettingsTabItem.Visibility = Visibility.Hidden;
                this.AssemblyDB.Visibility = Visibility.Hidden;

                this.DevMenu.Visibility = Config.Instance.ShowDevOptions ? Visibility.Visible : Visibility.Collapsed;
                this.Config.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName == "ShowDevOptions")
                    {
                        this.DevMenu.Visibility = Config.Instance.ShowDevOptions
                                                      ? Visibility.Visible
                                                      : Visibility.Collapsed;
                    }
                };

                await this.CheckForUpdates(true, true, false);

                if (!Config.Instance.TosAccepted)
                {
                    splash.Close(TimeSpan.FromMilliseconds(300));
                    this.Visibility = Visibility.Visible;
                    this.RightWindowCommands.Visibility = Visibility.Collapsed;
                    this.TosButton_OnClick(null, null);
                }
                else
                {
                    this.AssemblyButton_OnClick(null, null);
                }

                // wait for tos accept
                await Task.Factory.StartNew(
                    () =>
                    {
                        while (Config.Instance.TosAccepted == false)
                        {
                            Thread.Sleep(100);
                        }
                    });

                this.Config.PropertyChanged += this.ConfigOnSearchTextChanged;
                this.UpdateFilters();

                // Try to login with the saved credentials.
                if (!WebService.IsAuthenticated)
                {
                    splash.Close(TimeSpan.FromMilliseconds(300));

                    this.Browser.Visibility = Visibility.Hidden;
                    this.TosBrowser.Visibility = Visibility.Hidden;

                    this.Visibility = Visibility.Visible;
                    await this.ShowLoginDialog();
                    this.NewsButton_OnClick(null, null);
                }
                else
                {
                    this.OnLogin(Config.Instance.Username);
                }

                if (Config.Instance.FirstRun)
                {
                    Config.SaveAndRestart();
                }

                this.RightWindowCommands.Visibility = Visibility.Visible;

                splash.Close(TimeSpan.FromMilliseconds(300));
                this.Visibility = Visibility.Visible;
                await Updater.UpdateRepositories();
                await Updater.UpdateWebService();
                await this.UpdateAccount();
                Utility.Log(LogStatus.Info, "Update Complete");

                var allAssemblies = new List<LeagueSharpAssembly>();

                foreach (var profile in Config.Instance.Profiles)
                {
                    allAssemblies.AddRange(profile.InstalledAssemblies);
                }

                allAssemblies = allAssemblies.Distinct().ToList();

                GitUpdater.ClearUnusedRepos(allAssemblies);
                await this.PrepareAssemblies(allAssemblies, true, true);

                Utility.Log(LogStatus.Info, "Compile Complete");

                // injection, randomizer, remoting
                this.InitSystem();
                Utility.Log(LogStatus.Info, "System Initialisation Complete");

                this.MainTabControl.SelectedIndex = TAB_ASSEMBLIES;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Bootstrap Error");
            }
        }

        private async Task CheckForUpdates(bool loader, bool core, bool showDialogOnFinish)
        {
            try
            {
                if (this.CheckingForUpdates)
                {
                    return;
                }

                this.StatusString = Utility.GetMultiLanguageText("Checking");
                this.updateMessage = string.Empty;
                this.CheckingForUpdates = true;

                if (loader)
                {
                    var loaderVersionRequest = await WebService.RequestLoaderVersionAsync();
                    if (loaderVersionRequest.Outcome == OutcomeType.Successful)
                    {
                        var loaderVersion = loaderVersionRequest.Result;

                        try
                        {
                            if (File.Exists(Updater.SetupFile))
                            {
                                Thread.Sleep(1000);
                                File.Delete(Updater.SetupFile);
                            }
                        }
                        catch
                        {
                            MessageBox.Show(Utility.GetMultiLanguageText("FailedToDelete"));
                            Environment.Exit(0);
                        }

                        if (loaderVersion.Version > Assembly.GetExecutingAssembly().GetName().Version)
                        {
                            // Update the loader only when we are not injected to be able to replace the core files.
                            if (!Injection.IsInjected)
                            {
                                Console.WriteLine("Update Loader");
                                Updater.Updating = true;
                                await Updater.UpdateLoader(loaderVersion.Url);
                            }
                        }
                    }
                }

                if (core)
                {
                    if (Config.Instance.LeagueOfLegendsExePath != null)
                    {
                        var exe = Utility.GetLatestLeagueOfLegendsExePath(Config.Instance.LeagueOfLegendsExePath);
                        if (exe != null)
                        {
                            Console.WriteLine("Update Core");
                            var updateResult = await Updater.UpdateCore(exe, !showDialogOnFinish);
                            this.updateMessage = updateResult.Message;

                            switch (updateResult.State)
                            {
                                case Updater.CoreUpdateState.Operational:
                                    this.StatusString = Utility.GetMultiLanguageText("Updated");
                                    break;
                                case Updater.CoreUpdateState.Maintenance:
                                    this.StatusString = Utility.GetMultiLanguageText("OUTDATED");
                                    break;

                                default:
                                    this.StatusString = Utility.GetMultiLanguageText("Unknown");
                                    break;
                            }

                            return;
                        }
                    }

                    this.StatusString = Utility.GetMultiLanguageText("Unknown");
                    this.updateMessage = Utility.GetMultiLanguageText("LeagueNotFound");
                }
            }
            finally
            {
                this.CheckingForUpdates = false;
                Updater.CheckedForUpdates = true;

                if (showDialogOnFinish)
                {
                    this.ShowTextMessage(Utility.GetMultiLanguageText("UpdateStatus"), this.updateMessage);
                }
            }
        }

        private void CloneItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)this.InstalledAssembliesDataGrid.SelectedItems[0];
            try
            {
                var source = Path.GetDirectoryName(selectedAssembly.PathToProjectFile);
                var destination = Path.Combine(Directories.LocalRepositoriesDirectory, selectedAssembly.Name) + "_clone_"
                                  + Environment.TickCount.GetHashCode().ToString("X");
                Utility.CopyDirectory(source, destination);
                var leagueSharpAssembly = new LeagueSharpAssembly(
                    selectedAssembly.Name + "_clone", 
                    Path.Combine(destination, Path.GetFileName(selectedAssembly.PathToProjectFile)), 
                    string.Empty);
                leagueSharpAssembly.Compile();
                this.Config.SelectedProfile.InstalledAssemblies.Add(leagueSharpAssembly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ColumnWidthPropertyChanged(object sender, EventArgs e)
        {
            // listen for when the mouse is released
            this.columnWidthChanging = true;
            if (sender != null)
            {
                Mouse.AddPreviewMouseUpHandler(this, this.BaseDataGrid_MouseLeftButtonUp);
            }
        }

        private async void CompileAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.Working)
            {
                return;
            }

            await this.PrepareAssemblies(Config.Instance.SelectedProfile.InstalledAssemblies, false, true);
        }

        private void ConfigOnSearchTextChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!args.PropertyName.EndsWith("Check") && args.PropertyName != "SearchText")
            {
                return;
            }

            this.UpdateFilters();
        }

        private async void DeleteWithConfirmation(IEnumerable<LeagueSharpAssembly> asemblies)
        {
            var result =
                await
                this.ShowMessageAsync(
                    Utility.GetMultiLanguageText("Uninstall"), 
                    Utility.GetMultiLanguageText("UninstallConfirm"), 
                    MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Negative)
            {
                return;
            }

            foreach (var ee in asemblies)
            {
                Config.Instance.SelectedProfile.InstalledAssemblies.Remove(ee);

                if (ee.Type == AssemblyType.Library)
                {
                    try
                    {
                        if (File.Exists(ee.PathToBinary))
                        {
                            File.Delete(ee.PathToBinary);
                        }
                    }
                    catch
                    {
                        // locked kappa
                    }
                }
            }
        }

        private void EditItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)this.InstalledAssembliesDataGrid.SelectedItems[0];
            if (File.Exists(selectedAssembly.PathToProjectFile))
            {
                Process.Start(selectedAssembly.PathToProjectFile);
            }
        }

        private bool FilterAssemblies(object item)
        {
            try
            {
                var searchText = this.Config.SearchText.Replace("*", "(.*)");

                var assembly = item as LeagueSharpAssembly;
                if (assembly == null)
                {
                    return false;
                }

                if (searchText == "checked")
                {
                    return assembly.InjectChecked;
                }

                switch (assembly.Type)
                {
                    case AssemblyType.Champion:
                        if (!this.Config.ChampionCheck)
                        {
                            return false;
                        }

                        break;

                    case AssemblyType.Utility:
                        if (!this.Config.UtilityCheck)
                        {
                            return false;
                        }

                        break;

                    case AssemblyType.Library:
                        if (!this.Config.LibraryCheck)
                        {
                            return false;
                        }

                        break;
                }

                var nameMatch = Regex.Match(assembly.Name, searchText, RegexOptions.IgnoreCase);
                var displayNameMatch = Regex.Match(assembly.DisplayName, searchText, RegexOptions.IgnoreCase);
                var svnNameMatch = Regex.Match(assembly.SvnUrl, searchText, RegexOptions.IgnoreCase);
                var descNameMatch = Regex.Match(assembly.Description, searchText, RegexOptions.IgnoreCase);

                return displayNameMatch.Success || nameMatch.Success || svnNameMatch.Success || descNameMatch.Success;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool FilterDatabaseAssemblies(object item)
        {
            try
            {
                var searchText = this.Config.SearchText.Replace("*", "(.*)");

                var assembly = item as AssemblyEntry;
                if (assembly == null)
                {
                    return false;
                }

                switch (assembly.Type)
                {
                    case AssemblyType.Champion:
                        if (!this.Config.ChampionCheck)
                        {
                            return false;
                        }

                        break;

                    case AssemblyType.Utility:
                        if (!this.Config.UtilityCheck)
                        {
                            return false;
                        }

                        break;

                    case AssemblyType.Library:
                        if (!this.Config.LibraryCheck)
                        {
                            return false;
                        }

                        break;
                }

                var nameMatch = Regex.Match(assembly.Name, searchText, RegexOptions.IgnoreCase);
                var champeMatch = assembly.Type == AssemblyType.Champion
                                  && Regex.Match(string.Join(", ", assembly.Champions), searchText, RegexOptions.IgnoreCase).Success;
                var authorMatch = Regex.Match(assembly.AuthorName, searchText, RegexOptions.IgnoreCase);
                var svnNameMatch = Regex.Match(assembly.GithubUrl, searchText, RegexOptions.IgnoreCase);
                var descNameMatch = Regex.Match(assembly.Description, searchText, RegexOptions.IgnoreCase);

                return authorMatch.Success || champeMatch || nameMatch.Success || svnNameMatch.Success || descNameMatch.Success;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is TChildItem)
                {
                    return (TChildItem)child;
                }
                else
                {
                    var childOfChild = this.FindVisualChild<TChildItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        private DataGridCell GetCell(DataGridRow row, int columnIndex = 0)
        {
            var presenterE = row?.FindChildren<DataGridCellsPresenter>(true);
            if (presenterE == null)
            {
                return null;
            }

            var presenter = presenterE.ToList()[0];
            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (cell != null)
            {
                return cell;
            }

            // alternative way - now try to bring into view and retreive the cell
            this.InstalledAssembliesDataGrid.ScrollIntoView(row, this.InstalledAssembliesDataGrid.Columns[columnIndex]);
            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);

            return cell;
        }

        private int GetCurrentRowIndex(GetPosition pos)
        {
            var curIndex = -1;
            for (var i = 0; i < this.InstalledAssembliesDataGrid.Items.Count; i++)
            {
                var row = this.GetRowItem(i);
                if (row != null)
                {
                    var cell = this.GetCell(row);
                    if (cell != null && this.GetMouseTargetRow(row, pos) && !this.GetMouseTargetRow(cell, pos))
                    {
                        curIndex = i;
                        break;
                    }
                }
            }

            return curIndex;
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            var rect = VisualTreeHelper.GetDescendantBounds(theTarget);
            var point = position((IInputElement)theTarget);
            return rect.Contains(point);
        }

        private DataGridRow GetRowItem(int index)
        {
            if (this.InstalledAssembliesDataGrid.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return null;
            }

            return this.InstalledAssembliesDataGrid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private void GithubAssembliesItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)this.InstalledAssembliesDataGrid.SelectedItems[0];
            if (selectedAssembly.SvnUrl != string.Empty)
            {
                var window = new InstallerWindow { Owner = this };
                window.ShowProgress(selectedAssembly.SvnUrl, true);
                window.ShowDialog();
            }
        }

        private void GithubItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)this.InstalledAssembliesDataGrid.SelectedItems[0];
            if (selectedAssembly.SvnUrl != string.Empty)
            {
                Process.Start(selectedAssembly.SvnUrl);
            }
            else if (Directory.Exists(Path.GetDirectoryName(selectedAssembly.PathToProjectFile)))
            {
                Process.Start(Path.GetDirectoryName(selectedAssembly.PathToProjectFile));
            }
        }

        private void InitSystem()
        {
            PathRandomizer.CopyFiles();
            Remoting.Init();

            this.InjectThread = new Thread(
                () =>
                {
                    var trigger = new EdgeTrigger();

                    trigger.Rising += (sender, args) =>
                    {
                        Application.Current.Dispatcher.InvokeAsync(
                            () =>
                            {
                                this.icon_connected.Visibility = Visibility.Visible;
                                this.icon_disconnected.Visibility = Visibility.Collapsed;
                            });
                    };

                    trigger.Falling += (sender, args) =>
                    {
                        Application.Current.Dispatcher.InvokeAsync(
                            async () =>
                            {
                                this.icon_connected.Visibility = Visibility.Collapsed;
                                this.icon_disconnected.Visibility = Visibility.Visible;
                                await this.UpdateAccount();
                            });
                    };

                    while (true)
                    {
                        try
                        {
                            Thread.Sleep(3000);

                            if (Config.Instance.Install)
                            {
                                Injection.Pulse();
                                trigger.Value = Injection.IsInjected;

                                Console.WriteLine(Injection.SharedMemory.Data.IsLoaded);
                            }
                        }
                        catch
                        {
                            // ignored - A task was canceled.
                        }
                    }
                });

            this.InjectThread.SetApartmentState(ApartmentState.STA);
            this.InjectThread.Start();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new InstallerWindow { Owner = this };
            window.ShowDialog();
        }

        private void InstalledAssembliesDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (this.rowIndex < 0)
            {
                return;
            }

            var index = this.GetCurrentRowIndex(e.GetPosition);
            if (index < 0)
            {
                return;
            }

            if (index == this.rowIndex)
            {
                return;
            }

            var changedAssembly = this.Config.SelectedProfile.InstalledAssemblies[this.rowIndex];
            this.Config.SelectedProfile.InstalledAssemblies.RemoveAt(this.rowIndex);
            this.Config.SelectedProfile.InstalledAssemblies.Insert(index, changedAssembly);
        }

        private void InstalledAssembliesDataGrid_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            if (dataGrid != null)
            {
                if (dataGrid.SelectedItems.Count == 0)
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void InstalledAssembliesDataGrid_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            var container = sender as FrameworkElement;

            if (container == null)
            {
                return;
            }

            var scrollViewer = this.FindVisualChild<ScrollViewer>(container);

            if (scrollViewer == null)
            {
                return;
            }

            double tolerance = 15;
            var verticalPos = e.GetPosition(container).Y;
            double offset = 1;

            if (verticalPos < tolerance)
            {
                // Top visible? 
                // Scroll up
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset);
            }
            else if (verticalPos > container.ActualHeight - tolerance)
            {
                // Bot visible? 
                // Scroll down
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
            }
        }

        private void InstalledAssembliesDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.rowIndex = this.GetCurrentRowIndex(e.GetPosition);
            if (this.rowIndex < 0)
            {
                return;
            }

            if (this.IsColumnSelected(e))
            {
                return;
            }

            this.InstalledAssembliesDataGrid.SelectedIndex = this.rowIndex;
            var selectedAssembly = this.InstalledAssembliesDataGrid.Items[this.rowIndex] as LeagueSharpAssembly;
            if (selectedAssembly == null)
            {
                return;
            }

            if (DragDrop.DoDragDrop(this.InstalledAssembliesDataGrid, selectedAssembly, DragDropEffects.Move)
                != DragDropEffects.None)
            {
            }
        }

        private async void InstallFromDbItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.AssembliesDBDataGrid.SelectedItems.Count == 0)
            {
                return;
            }

            foreach (var result in this.AssembliesDBDataGrid.SelectedItems.Cast<AssemblyEntry>())
            {
                if (this.Config.SelectedProfile.InstalledAssemblies.Any(a => a.Name == result.Name.WebDecode()))
                {
                    await this.ShowMessageAsync("Installer", $"{result.Name} is already installed");
                    continue;
                }

                await InstallerWindow.InstallAssembly(result, false);
            }
        }

        private bool IsColumnSelected(MouseEventArgs e)
        {
            var dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is DataGridColumnHeader)
            {
                return true;
            }

            return false;
        }

        private void LogItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)this.InstalledAssembliesDataGrid.SelectedItems[0];
            var logFile = Path.Combine(
                Directories.LogsDirectory, 
                "Error - " + Path.GetFileName(selectedAssembly.Name + ".txt"));
            if (File.Exists(logFile))
            {
                Process.Start(logFile);
            }
            else
            {
                this.ShowTextMessage("Error", Utility.GetMultiLanguageText("LogNotFound"));
            }
        }

        private async void MainWindow_OnActivated(object sender, EventArgs e)
        {
            try
            {
                var text = Clipboard.GetText();
                if (text.StartsWith(LSUriScheme.FullName))
                {
                    Clipboard.SetText(string.Empty);
                    await LSUriScheme.HandleUrl(text, this);
                }
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, ex.Message);
            }
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            await this.Bootstrap();
            this.SetForeground();
        }

        private void NewItem_OnClick(object sender, RoutedEventArgs e)
        {
            this.ShowNewAssemblyDialog();
        }

        private void NewsButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.TosBrowser.Visibility = Visibility.Hidden;

            this.Browser.Navigate($"{this.BaseUrl}/loader/news/1/html");
            this.MainTabControl.SelectedIndex = TAB_NEWS;

            this.Browser.Visibility = Visibility.Visible;
        }

        private void OnLogin(string username)
        {
            Utility.Log(LogStatus.Info, $"Succesfully signed in as {username}");
            this.Browser.Visibility = Visibility.Visible;
            this.TosBrowser.Visibility = Visibility.Visible;
        }

        private void RemoveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var remove = this.InstalledAssembliesDataGrid.SelectedItems.Cast<LeagueSharpAssembly>().ToList();
            this.DeleteWithConfirmation(remove);
        }

        private void SetForeground()
        {
            this.Show();

            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Browser.Visibility = Visibility.Hidden;
            this.TosBrowser.Visibility = Visibility.Hidden;

            this.MainTabControl.SelectedIndex = TAB_SETTINGS;
        }

        private void ShareItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var stringToAppend = string.Empty;
            var count = 0;
            foreach (var selectedAssembly in this.InstalledAssembliesDataGrid.SelectedItems.Cast<LeagueSharpAssembly>())
            {
                if (selectedAssembly.SvnUrl.StartsWith(
                    "https://github.com", 
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    var user = selectedAssembly.SvnUrl.Remove(0, 19);
                    stringToAppend += $"{user}/{selectedAssembly.Name}/";
                    count++;
                }
            }

            if (count > 0)
            {
                var uri = LSUriScheme.FullName + (count == 1 ? "project" : "projectGroup") + "/" + stringToAppend;
                Clipboard.SetText(uri.Replace(" ", "%20"));
                this.ShowTextMessage(
                    Utility.GetMultiLanguageText("MenuShare"), 
                    Utility.GetMultiLanguageText("ShareText"));
            }
        }

        private async Task ShowLoginDialog()
        {
            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;

            while (true)
            {
                var result =
                    await
                    this.ShowLoginAsync(
                        "LeagueSharp", 
                        "Sign in", 
                        new LoginDialogSettings
                        {
                            ColorScheme = this.MetroDialogOptions.ColorScheme, 
                            NegativeButtonVisibility = Visibility.Visible
                        });

                if (result == null)
                {
                    this.MainWindow_OnClosing(null, null);
                    Environment.Exit(0);
                }

                var hash = Auth.Hash(result.Password);
                var loginResult = await Auth.Login(result.Username, hash);

                if (loginResult.Item1)
                {
                    // Save the login credentials
                    Config.Instance.Username = result.Username;
                    Config.Instance.Password = Auth.Hash(result.Password);

                    this.OnLogin(result.Username);
                    break;
                }

                await this.ShowMessageAsync("Login", string.Format(Utility.GetMultiLanguageText("FailedToLogin"), loginResult.Item2));

                Utility.Log(
                    LogStatus.Error, 
                    string.Format(
                        Utility.GetMultiLanguageText("LoginError"), 
                        result.Username, 
                        loginResult.Item2));
            }
        }

        private async void ShowNewAssemblyDialog()
        {
            var assemblyName = await this.ShowInputAsync("New Project", "Enter the new project name");

            if (assemblyName != null)
            {
                assemblyName = Regex.Replace(assemblyName, @"[^A-Za-z0-9]+", string.Empty);
            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
                var leagueSharpAssembly = Utility.CreateEmptyAssembly(assemblyName);
                if (leagueSharpAssembly != null)
                {
                    leagueSharpAssembly.Compile();
                    this.Config.SelectedProfile.InstalledAssemblies.Add(leagueSharpAssembly);
                }
            }
        }

        private async void StatusButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Browser.Visibility = Visibility.Hidden;
            this.TosBrowser.Visibility = Visibility.Hidden;

            await this.UpdateAccount();
            await this.CheckForUpdates(true, true, true);
        }

        private void TosAccept_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.TosAccepted = true;
            NewsButton_OnClick(null, null);
        }

        private void TosButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Browser.Visibility = Visibility.Hidden;

            this.TosBrowser.Navigate($"{this.BaseUrl}/loader/tos/1");
            this.MainTabControl.SelectedIndex = TAB_TOS;

            this.TosBrowser.Visibility = Visibility.Visible;
        }

        private void TosDecline_Click(object sender, RoutedEventArgs e)
        {
            this.MainWindow_OnClosing(null, null);
            Environment.Exit(0);
        }

        private void TrayIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Hide();
                this.MenuItemLabelHide.Header = "Show";
            }
        }

        private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.SetForeground();
                this.MenuItemLabelHide.Header = "Hide";
            }
        }

        private void TrayMenuClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TrayMenuHide_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Hide();
                this.MenuItemLabelHide.Header = "Show";
            }
            else
            {
                this.SetForeground();
                this.MenuItemLabelHide.Header = "Hide";
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var name = ((TreeViewItem)((TreeView)sender).SelectedItem).Uid;
            var viewType = Type.GetType($"LeagueSharp.Loader.Views.Settings.{name}");

            if (viewType == null)
            {
                Utility.Log(
                    LogStatus.Warning, 
                    $"Could not find Settings View (LeagueSharp.Loader.Views.Settings.{name})");
                return;
            }

            this.SettingsFrame.Content = Activator.CreateInstance(viewType);
        }

        private async Task UpdateAccount()
        {
            try
            {
                if (DateTime.Now - this.LastAccountUpdate < TimeSpan.FromMinutes(10))
                {
                    return;
                }

                this.LastAccountUpdate = DateTime.Now;

                var accountRequest = await WebService.RequestAccountAsync();
                if (accountRequest.Outcome == OutcomeType.Failure)
                {
                    this.Header.Text = $"L# {Assembly.GetExecutingAssembly().GetName().Version}";
                    return;
                }

                var account = accountRequest.Result;
                var text = "Normal";

                if (account.IsSubscriber)
                {
                    text = "Sub";
                }

                if (account.IsBotter)
                {
                    text = "Bot";
                }

                if (account.IsDev)
                {
                    text = "Dev";
                }

                this.Header.Text = $"L# - {account.DisplayName}/{text} - {account.GamesPlayed}/{account.MaxGames} - {Assembly.GetExecutingAssembly().GetName().Version}";
            }
            catch
            {
                // ignored
            }
        }

        private async void UpdateAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.Working)
            {
                return;
            }

            await this.PrepareAssemblies(Config.Instance.SelectedProfile.InstalledAssemblies, true, true);
        }

        private async void UpdateAndCompileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.InstalledAssembliesDataGrid.SelectedItems.Count == 0)
            {
                return;
            }

            if (this.Working)
            {
                return;
            }

            await this.PrepareAssemblies(this.InstalledAssembliesDataGrid.SelectedItems.Cast<LeagueSharpAssembly>(), true, true);
        }

        private void UpdateFilters()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                return;
            }

            ICollectionView view = null;

            switch (this.MainTabControl.SelectedIndex)
            {
                case TAB_ASSEMBLIES:
                    view = CollectionViewSource.GetDefaultView(Config.Instance.SelectedProfile.InstalledAssemblies);
                    view.Filter = this.FilterAssemblies;
                    break;

                case TAB_DATABASE:
                    view = CollectionViewSource.GetDefaultView(Config.Instance.DatabaseAssemblies);
                    view.Filter = this.FilterDatabaseAssemblies;
                    break;
            }
        }
    }
}
