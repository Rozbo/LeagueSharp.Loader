// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;

    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;

    using MahApps.Metro;

    public partial class App
    {
        private bool createdNew;

        private Mutex mutex;

        public static string[] Args { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (File.Exists(Updater.SetupFile))
            {
                Thread.Sleep(1000);
            }

            this.mutex = new Mutex(true, Utility.Md5Hash(Environment.UserName), out this.createdNew);
            Args = e.Args;

            try
            {
                if (string.Compare(Process.GetCurrentProcess().ProcessName, "LeagueSharp.Loader.exe", StringComparison.InvariantCultureIgnoreCase)
                    != 0 && File.Exists(Path.Combine(Directories.CurrentDirectory, "LeagueSharp.Loader.exe")))
                {
                    File.Delete(Path.Combine(Directories.CurrentDirectory, "LeagueSharp.Loader.exe"));
                    File.Delete(Path.Combine(Directories.CurrentDirectory, "LeagueSharp.Loader.exe.config"));
                }
            }
            catch
            {
                // ignore
            }

            this.ConfigInit();
            this.AppDataRandomization();

#if !DEBUG
            this.ExecutableRandomization();
#endif

            this.Localize();

            if (Config.Instance.SelectedColor != null)
            {
                ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(Config.Instance.SelectedColor), ThemeManager.GetAppTheme("BaseLight"));
            }

            await Auth.Login(Config.Instance.Username, Config.Instance.Password);

            base.OnStartup(e);
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void AppDataRandomization()
        {
            try
            {
                if (!Directory.Exists(Directories.AppDataDirectory))
                {
                    Directory.CreateDirectory(Directories.AppDataDirectory);

                    var oldPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                        "LeagueSharp" + Environment.UserName.GetHashCode().ToString("X"));

                    var oldPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueSharp");

                    if (Directory.Exists(oldPath))
                    {
                        Utility.CopyDirectory(oldPath, Directories.AppDataDirectory, true, true);
                        Utility.ClearDirectory(oldPath);
                        Directory.Delete(oldPath, true);
                    }

                    if (Directory.Exists(oldPath2))
                    {
                        Utility.CopyDirectory(oldPath2, Directories.AppDataDirectory, true, true);
                        Utility.ClearDirectory(oldPath2);
                        Directory.Delete(oldPath2, true);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private void ConfigInit()
        {
            Config.Load(Assembly.GetExecutingAssembly().Location.EndsWith("loader.exe", StringComparison.OrdinalIgnoreCase));

            if (Config.Instance.Settings.GameSettings.All(x => x.Name != "Show Drawings"))
            {
                Config.Instance.Settings.GameSettings.Add(
                    new GameSettings { Name = "Show Drawings", PosibleValues = new List<string> { "True", "False" }, SelectedValue = "True" });
            }

            if (Config.Instance.Settings.GameSettings.All(x => x.Name != "Show Ping"))
            {
                Config.Instance.Settings.GameSettings.Add(
                    new GameSettings { Name = "Show Ping", PosibleValues = new List<string> { "True", "False" }, SelectedValue = "True" });
            }

            if (Config.Instance.Settings.GameSettings.All(x => x.Name != "Send Anonymous Assembly Statistics"))
            {
                Config.Instance.Settings.GameSettings.Add(
                    new GameSettings
                    {
                        Name = "Send Anonymous Assembly Statistics", 
                        PosibleValues = new List<string> { "True", "False" }, 
                        SelectedValue = "True"
                    });
            }

            if (Config.Instance.Settings.GameSettings.All(x => x.Name != "Always Inject Default Profile"))
            {
                Config.Instance.Settings.GameSettings.Add(
                    new GameSettings
                    {
                        Name = "Always Inject Default Profile", 
                        PosibleValues = new List<string> { "True", "False" }, 
                        SelectedValue = "False"
                    });
            }
        }

        private void ExecutableRandomization()
        {
            if (Assembly.GetExecutingAssembly().Location.EndsWith("loader.exe", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (Config.Instance.RandomName != null)
                    {
                        try
                        {
                            if (File.Exists(Directories.LoaderRandomFilePath))
                            {
                                File.SetAttributes(Directories.LoaderRandomFilePath, FileAttributes.Normal);
                                File.Delete(Directories.LoaderRandomFilePath);
                            }

                            if (File.Exists(Directories.LoaderRandomPdbFilePath))
                            {
                                File.SetAttributes(Directories.LoaderRandomPdbFilePath, FileAttributes.Normal);
                                File.Delete(Directories.LoaderRandomPdbFilePath);
                            }

                            if (File.Exists(Directories.LoaderRandomConfigFilePath))
                            {
                                File.SetAttributes(Directories.LoaderRandomConfigFilePath, FileAttributes.Normal);
                                File.Delete(Directories.LoaderRandomConfigFilePath);
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        if (!this.createdNew)
                        {
                            if (Args.Length > 0)
                            {
                                var loader = Process.GetProcessesByName(Config.Instance.RandomName).FirstOrDefault();

                                if (loader != null && loader.MainWindowHandle != IntPtr.Zero)
                                {
                                    Clipboard.SetText(Args[0]);
                                    ShowWindow(loader.MainWindowHandle, 5);
                                    SetForegroundWindow(loader.MainWindowHandle);
                                }
                            }

                            this.mutex = null;
                            Environment.Exit(0);
                        }
                    }

                    try
                    {
                        Config.Instance.RandomName = Utility.GetUniqueKey(6);
                        Config.Save(false);

                        File.Copy(Path.Combine(Directories.CurrentDirectory, "loader.exe"), Directories.LoaderRandomFilePath);
                        File.Copy(Path.Combine(Directories.CurrentDirectory, "loader.pdb"), Directories.LoaderRandomPdbFilePath);
                        File.Copy(Path.Combine(Directories.CurrentDirectory, "loader.exe.config"), Directories.LoaderRandomConfigFilePath);

                        Process.Start(Directories.LoaderRandomFilePath);
                    }
                    catch (Exception e)
                    {
                        Utility.Log(LogStatus.Error, e.Message);
                    }

                    Environment.Exit(0);
                }
                catch (Exception)
                {
                    // restart
                }
            }

            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                try
                {
                    Injection.Unload();
                    Utility.ClearDirectory(Directories.AssembliesDirectory);
                    Utility.ClearDirectory(Directories.LogsDirectory);

                    Views.MainWindow.Instance?.TrayIcon?.Dispose();

                    if (this.mutex != null && this.createdNew)
                    {
                        this.mutex.ReleaseMutex();
                    }
                }
                catch
                {
                    // ignored
                }

                if (!Assembly.GetExecutingAssembly().Location.EndsWith("loader.exe"))
                {
                    var info = new ProcessStartInfo
                               {
                                   Arguments =
                                       "/C choice /C Y /N /D Y /T 1 & Del \"" + Directories.LoaderRandomFilePath + "\" \""
                                       + Directories.LoaderRandomConfigFilePath
                                       + "\" \"" + Directories.LoaderRandomPdbFilePath + "\"", 
                                   WindowStyle = ProcessWindowStyle.Hidden, 
                                   CreateNoWindow = true, 
                                   FileName = "cmd.exe"
                               };
                    Process.Start(info);
                }
            };
        }

        private void Localize()
        {
            // Load the language resources.
            var dict = new ResourceDictionary();

            if (Config.Instance.SelectedLanguage != null)
            {
                dict.Source = new Uri("..\\Resources\\Language\\" + Config.Instance.SelectedLanguage + ".xaml", UriKind.Relative);
            }
            else
            {
                var lid = Thread.CurrentThread.CurrentCulture.ToString().Contains("-")
                              ? Thread.CurrentThread.CurrentCulture.ToString().Split('-')[0].ToUpperInvariant()
                              : Thread.CurrentThread.CurrentCulture.ToString().ToUpperInvariant();
                switch (lid)
                {
                    case "DE":
                        dict.Source = new Uri("..\\Resources\\Language\\German.xaml", UriKind.Relative);
                        break;
                    case "AR":
                        dict.Source = new Uri("..\\Resources\\Language\\Arabic.xaml", UriKind.Relative);
                        break;
                    case "ES":
                        dict.Source = new Uri("..\\Resources\\Language\\Spanish.xaml", UriKind.Relative);
                        break;
                    case "FR":
                        dict.Source = new Uri("..\\Resources\\Language\\French.xaml", UriKind.Relative);
                        break;
                    case "IT":
                        dict.Source = new Uri("..\\Resources\\Language\\Italian.xaml", UriKind.Relative);
                        break;
                    case "KO":
                        dict.Source = new Uri("..\\Resources\\Language\\Korean.xaml", UriKind.Relative);
                        break;
                    case "NL":
                        dict.Source = new Uri("..\\Resources\\Language\\Dutch.xaml", UriKind.Relative);
                        break;
                    case "PL":
                        dict.Source = new Uri("..\\Resources\\Language\\Polish.xaml", UriKind.Relative);
                        break;
                    case "PT":
                        dict.Source = new Uri("..\\Resources\\Language\\Portuguese.xaml", UriKind.Relative);
                        break;
                    case "RO":
                        dict.Source = new Uri("..\\Resources\\Language\\Romanian.xaml", UriKind.Relative);
                        break;
                    case "RU":
                        dict.Source = new Uri("..\\Resources\\Language\\Russian.xaml", UriKind.Relative);
                        break;
                    case "SE":
                        dict.Source = new Uri("..\\Resources\\Language\\Swedish.xaml", UriKind.Relative);
                        break;
                    case "TR":
                        dict.Source = new Uri("..\\Resources\\Language\\Turkish.xaml", UriKind.Relative);
                        break;
                    case "VI":
                        dict.Source = new Uri("..\\Resources\\Language\\Vietnamese.xaml", UriKind.Relative);
                        break;
                    case "ZH":
                        dict.Source = new Uri("..\\Resources\\Language\\Chinese.xaml", UriKind.Relative);
                        break;
                    case "LT":
                        dict.Source = new Uri("..\\Resources\\Language\\Lithuanian.xaml", UriKind.Relative);
                        break;
                    case "CZ":
                        dict.Source = new Uri("..\\Resources\\Language\\Czech.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("..\\Resources\\Language\\English.xaml", UriKind.Relative);
                        break;
                }
            }

            this.Resources.MergedDictionaries.Add(dict);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }
}