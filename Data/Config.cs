﻿#region LICENSE

// Copyright 2016-2016 LeagueSharp.Loader
// Config.cs is part of LeagueSharp.Loader.
// 
// LeagueSharp.Loader is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LeagueSharp.Loader is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LeagueSharp.Loader. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace LeagueSharp.Loader.Data
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Xml.Serialization;

    using LeagueSharp.Loader.Class;

    using Newtonsoft.Json;

    using PlaySharp.Service.Model;

    using MessageBox = System.Windows.MessageBox;

    #endregion

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Config : INotifyPropertyChanged
    {
        private string _appDirectory;

        private double _columnCheckWidth = 20;

        private double _columnLocationWidth = 180;

        private double _columnNameWidth = 150;

        private double _columnTypeWidth = 75;

        private double _columnVersionWidth = 90;

        private bool _firstRun = true;

        private Hotkeys _hotkeys;

        private bool _install = true;

        private ObservableCollection<string> _knownRepositories;

        private string _leagueOfLegendsExePath;

        private ObservableCollection<Profile> _profiles;

        private string _selectedColor;

        private string _selectedLanguage;

        private Profile _selectedProfile;

        private ConfigSettings _settings;

        private bool _showDevOptions;

        private bool _tosAccepted;

        private bool _updateOnLoad;

        private List<string> blockedRepositories;

        private ObservableCollection<AssemblyEntry> databaseAssemblies = new ObservableCollection<AssemblyEntry>();

        private bool enableDebug;

        private bool updateCoreOnInject = true;

        private double windowHeight = 450;

        private double windowLeft = 150;

        private WindowState windowState;

        private double windowTop = 150;

        private double windowWidth = 800;

        private int workers = 5;

        private bool useCloudConfig = true;

        [XmlIgnore]
        [JsonIgnore]
        public static Config Instance { get; set; }

        public bool UseCloudConfig
        {
            get
            {
                return this.useCloudConfig;
            }
            set
            {
                this.useCloudConfig = value;
                this.OnPropertyChanged();
            }
        }

        public int Workers
        {
            get
            {
                return this.workers;
            }
            set
            {
                this.workers = value;
                this.OnPropertyChanged();
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public ObservableCollection<AssemblyEntry> DatabaseAssemblies
        {
            get
            {
                return this.databaseAssemblies;
            }
            set
            {
                this.databaseAssemblies = value;
                this.OnPropertyChanged();
            }
        }

        public bool EnableDebug
        {
            get
            {
                return this.enableDebug;
            }
            set
            {
                this.enableDebug = value;
                this.OnPropertyChanged();
            }
        }

        public string AppDirectory
        {
            get
            {
                return this._appDirectory;
            }
            set
            {
                this._appDirectory = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnCheckWidth
        {
            get
            {
                return this._columnCheckWidth;
            }
            set
            {
                this._columnCheckWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnLocationWidth
        {
            get
            {
                return this._columnLocationWidth;
            }
            set
            {
                this._columnLocationWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnNameWidth
        {
            get
            {
                return this._columnNameWidth;
            }
            set
            {
                this._columnNameWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnTypeWidth
        {
            get
            {
                return this._columnTypeWidth;
            }
            set
            {
                this._columnTypeWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnVersionWidth
        {
            get
            {
                return this._columnVersionWidth;
            }
            set
            {
                this._columnVersionWidth = value;
                this.OnPropertyChanged();
            }
        }

        public bool FirstRun
        {
            get
            {
                return this._firstRun;
            }
            set
            {
                this._firstRun = value;
                this.OnPropertyChanged();
            }
        }

        public Hotkeys Hotkeys
        {
            get
            {
                return this._hotkeys;
            }
            set
            {
                this._hotkeys = value;
                this.OnPropertyChanged();
            }
        }

        public bool Install
        {
            get
            {
                return this._install;
            }
            set
            {
                this._install = value;
                this.OnPropertyChanged();
            }
        }

        [XmlArrayItem("KnownRepositories", IsNullable = true)]
        public ObservableCollection<string> KnownRepositories
        {
            get
            {
                return this._knownRepositories;
            }
            set
            {
                this._knownRepositories = value;
                this.OnPropertyChanged();
            }
        }

        public string LeagueOfLegendsExePath
        {
            get
            {
                return this._leagueOfLegendsExePath;
            }
            set
            {
                if (value.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                this._leagueOfLegendsExePath = value;
                this.OnPropertyChanged();
            }
        }

        public string Password { get; set; }

        [XmlArrayItem("Profiles", IsNullable = true)]
        public ObservableCollection<Profile> Profiles
        {
            get
            {
                return this._profiles;
            }
            set
            {
                this._profiles = value;
                this.OnPropertyChanged();
            }
        }

        public string RandomName { get; set; }

        public string SelectedColor
        {
            get
            {
                return this._selectedColor;
            }
            set
            {
                this._selectedColor = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedLanguage
        {
            get
            {
                return this._selectedLanguage;
            }
            set
            {
                this._selectedLanguage = value;
                this.OnPropertyChanged();
            }
        }

        public Profile SelectedProfile
        {
            get
            {
                return this._selectedProfile;
            }
            set
            {
                this._selectedProfile = value;
                this.OnPropertyChanged();
            }
        }

        public ConfigSettings Settings
        {
            get
            {
                return this._settings;
            }
            set
            {
                this._settings = value;
                this.OnPropertyChanged();
            }
        }

        public bool ShowDevOptions
        {
            get
            {
                return this._showDevOptions;
            }
            set
            {
                this._showDevOptions = value;
                this.OnPropertyChanged();
            }
        }

        public bool TosAccepted
        {
            get
            {
                return this._tosAccepted;
            }
            set
            {
                this._tosAccepted = value;
                this.OnPropertyChanged();
            }
        }

        public bool UpdateCoreOnInject
        {
            get
            {
                return this.updateCoreOnInject;
            }
            set
            {
                this.updateCoreOnInject = value;
                this.OnPropertyChanged();
            }
        }

        public bool UpdateOnLoad
        {
            get
            {
                return this._updateOnLoad;
            }
            set
            {
                this._updateOnLoad = value;
                this.OnPropertyChanged();
            }
        }

        public string Username { get; set; }

        public double WindowHeight
        {
            get
            {
                return this.windowHeight;
            }
            set
            {
                this.windowHeight = value;
                this.OnPropertyChanged();
            }
        }

        public double WindowLeft
        {
            get
            {
                return this.windowLeft;
            }
            set
            {
                this.windowLeft = value;
                this.OnPropertyChanged();
            }
        }

        public WindowState WindowState
        {
            get
            {
                return this.windowState;
            }
            set
            {
                this.windowState = value;
                this.OnPropertyChanged();
            }
        }

        public double WindowTop
        {
            get
            {
                return this.windowTop;
            }
            set
            {
                this.windowTop = value;
                this.OnPropertyChanged();
            }
        }

        public double WindowWidth
        {
            get
            {
                return this.windowWidth;
            }
            set
            {
                this.windowWidth = value;
                this.OnPropertyChanged();
            }
        }

        public List<string> BlockedRepositories
        {
            get
            {
                return this.blockedRepositories;
            }
            set
            {
                this.blockedRepositories = value;
                this.OnPropertyChanged();
            }
        }

        public string AuthKey { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsOnScreen()
        {
            var screens = Screen.AllScreens;
            foreach (var screen in screens)
            {
                var formTopLeft = new System.Drawing.Point((int)this.WindowLeft, (int)this.WindowTop);

                if (screen.WorkingArea.Contains(formTopLeft))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SaveAndRestart(bool cloud = false)
        {
            Instance.FirstRun = false;
            Save(cloud);

            var info = new ProcessStartInfo
                {
                    Arguments = "/C choice /C Y /N /D Y /T 1 & " + Path.Combine(Directories.CurrentDirectory, "loader.exe"),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                };

            Process.Start(info);
            Environment.Exit(0);
        }

        public static void Save(bool cloud = false)
        {
            try
            {
                if (!Instance.IsOnScreen())
                {
                    Instance.WindowTop = 100;
                    Instance.WindowLeft = 100;
                }

                Utility.MapClassToXmlFile(typeof(Config), Instance, Directories.ConfigFilePath);

                if (cloud && 
                    Instance.UseCloudConfig && 
                    !string.IsNullOrEmpty(Instance.Username) && 
                    !string.IsNullOrEmpty(Instance.Password) && 
                    WebService.Client.IsAuthenticated)
                {
                    WebService.Client.CloudStore(Instance, "Config");
                }
            }
            catch
            {
                MessageBox.Show(Utility.GetMultiLanguageText("ConfigWriteError"));
            }
        }

        private static bool LoadFromCloud()
        {
            try
            {
                try
                {
                    if (File.Exists(Directories.ConfigFilePath))
                    {
                        Instance = (Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath);
                    }
                }
                catch
                {
                    // load error
                }

                if (!Instance.UseCloudConfig)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(Instance?.Username) || string.IsNullOrEmpty(Instance?.Password))
                {
                    return false;
                }

                if (!WebService.Client.Login(Instance.Username, Instance.Password))
                {
                    Instance.Username = string.Empty;
                    Instance.Password = string.Empty;
                    return false;
                }

                var configContent = WebService.Client.Cloud("Config");
                if (string.IsNullOrEmpty(configContent))
                {
                    return false;
                }

                var config = JsonConvert.DeserializeObject<Config>(configContent);
                if (config == null)
                {
                    return false;
                }

                config.RandomName = Instance.RandomName;
                config.Username = Instance.Username;
                config.Password = Instance.Password;
                config.AuthKey = WebService.Client.LoginData.Token;
                Instance = config;

                Save(false);

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static bool LoadFromBackup()
        {
            try
            {
                if (!File.Exists($"{Directories.ConfigFilePath}.bak"))
                {
                    return false;
                }

                Instance = (Config)Utility.MapXmlFileToClass(typeof(Config), $"{Directories.ConfigFilePath}.bak");
                Save(false);

                return true;
            }
            catch
            {
                File.Delete($"{Directories.ConfigFilePath}.bak");
            }

            return false;
        }

        private static bool LoadFromResource()
        {
            try
            {
                Utility.CreateFileFromResource(Directories.ConfigFilePath, "LeagueSharp.Loader.Resources.config.xml");
                Instance = (Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath);
                Save(false);

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static bool LoadFromFile()
        {
            try
            {
                if (!File.Exists(Directories.ConfigFilePath))
                {
                    return false;
                }

                Instance = (Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath);
                var backupFile = $"{Directories.ConfigFilePath}.bak";

                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                }

                File.Copy(Directories.ConfigFilePath, backupFile);
                File.SetAttributes(backupFile, FileAttributes.Hidden);

                return true;
            }
            catch
            {
                //ignore
            }

            return false;
        }

        public static void Load(bool isLoader = false)
        {
            if (App.Args.Length == 0 && !isLoader)
            {
                if (LoadFromCloud())
                {
                    return;
                }
            }

            if (LoadFromFile())
            {
                return;
            }

            if (LoadFromBackup())
            {
                return;
            }

            if (LoadFromResource())
            {
                return;
            }

            MessageBox.Show("Something went horribly wrong while loading your Configuration /ff");
            Environment.Exit(0);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [XmlType(AnonymousType = true)]
    public class ConfigSettings : INotifyPropertyChanged
    {
        private ObservableCollection<GameSettings> _gameSettings;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameSettings : INotifyPropertyChanged
    {
        private string _name;

        private List<string> _posibleValues;

        private string _selectedValue;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [XmlType(AnonymousType = true)]
    public class Hotkeys : INotifyPropertyChanged
    {
        private ObservableCollection<HotkeyEntry> _selectedHotkeys;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HotkeyEntry : INotifyPropertyChanged
    {
        private Key _hotkey;

        private string _name;

        public Key DefaultKey { get; set; }

        public string Description { get; set; }

        public string DisplayDescription => Utility.GetMultiLanguageText(this.Description);

        public Key Hotkey
        {
            get
            {
                return this._hotkey;
            }
            set
            {
                this._hotkey = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("HotkeyString");
            }
        }

        public byte HotkeyInt
        {
            get
            {
                if (this.Hotkey == Key.LeftShift || this.Hotkey == Key.RightShift)
                {
                    return 16;
                }

                if (this.Hotkey == Key.LeftAlt || this.Hotkey == Key.RightAlt)
                {
                    return 0x12;
                }

                if (this.Hotkey == Key.LeftCtrl || this.Hotkey == Key.RightCtrl)
                {
                    return 0x11;
                }

                return (byte)KeyInterop.VirtualKeyFromKey(this.Hotkey);
            }
            set
            {
            }
        }

        public string HotkeyString => this._hotkey.ToString();

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}