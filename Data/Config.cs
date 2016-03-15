// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using LeagueSharp.Loader.Class;

    using Newtonsoft.Json;

    using PlaySharp.Service.WebService.Model;

    using MessageBox = System.Windows.MessageBox;

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Config : INotifyPropertyChanged
    {
        private string authKey;

        private ObservableCollection<RepositoryEntry> blockedRepositories = new ObservableCollection<RepositoryEntry>();

        private bool championCheck = true;

        private double columnCheckWidth = 20;

        private double columnLocationWidth = 180;

        private double columnNameWidth = 150;

        private double columnTypeWidth = 75;

        private double columnVersionWidth = 90;

        private ObservableCollection<AssemblyEntry> databaseAssemblies = new ObservableCollection<AssemblyEntry>();

        private bool enableDebug;

        private bool firstRun = true;

        private Hotkeys hotkeys;

        private bool install = true;

        private ObservableCollection<RepositoryEntry> knownRepositories = new ObservableCollection<RepositoryEntry>();

        private string leagueOfLegendsExePath;

        private bool libraryCheck = true;

        private string password;

        private ObservableCollection<Profile> profiles = new ObservableCollection<Profile>();

        private string searchText = string.Empty;

        private string selectedColor;

        private string selectedLanguage;

        private int selectedProfileId;

        private ConfigSettings settings;

        private bool showDevOptions;

        private bool tosAccepted;

        private bool updateCoreOnInject = true;

        private bool updateOnLoad;

        private bool useCloudConfig = true;

        private string username;

        private bool utilityCheck = true;

        private double windowHeight = 450;

        private double windowLeft = 150;

        private double windowTop = 150;

        private double windowWidth = 800;

        private int workers = 5;

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        [JsonIgnore]
        public static Config Instance { get; set; }

        public string AuthKey
        {
            get
            {
                return this.authKey;
            }

            set
            {
                this.authKey = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<RepositoryEntry> BlockedRepositories
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

        public bool ChampionCheck
        {
            get
            {
                return this.championCheck;
            }

            set
            {
                this.championCheck = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnCheckWidth
        {
            get
            {
                return this.columnCheckWidth;
            }

            set
            {
                this.columnCheckWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnLocationWidth
        {
            get
            {
                return this.columnLocationWidth;
            }

            set
            {
                this.columnLocationWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnNameWidth
        {
            get
            {
                return this.columnNameWidth;
            }

            set
            {
                this.columnNameWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnTypeWidth
        {
            get
            {
                return this.columnTypeWidth;
            }

            set
            {
                this.columnTypeWidth = value;
                this.OnPropertyChanged();
            }
        }

        public double ColumnVersionWidth
        {
            get
            {
                return this.columnVersionWidth;
            }

            set
            {
                this.columnVersionWidth = value;
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

        public bool FirstRun
        {
            get
            {
                return this.firstRun;
            }

            set
            {
                this.firstRun = value;
                this.OnPropertyChanged();
            }
        }

        public Hotkeys Hotkeys
        {
            get
            {
                return this.hotkeys;
            }

            set
            {
                this.hotkeys = value;
                this.OnPropertyChanged();
            }
        }

        public bool Install
        {
            get
            {
                return this.install;
            }

            set
            {
                this.install = value;
                this.OnPropertyChanged();
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public ObservableCollection<RepositoryEntry> KnownRepositories
        {
            get
            {
                return this.knownRepositories;
            }

            set
            {
                this.knownRepositories = value;
                this.OnPropertyChanged();
            }
        }

        public string LeagueOfLegendsExePath
        {
            get
            {
                return this.leagueOfLegendsExePath;
            }

            set
            {
                if (!value.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    Utility.Log(LogStatus.Error, "LeagueOfLegendsExePath", $"Invalid file: {value}", Logs.MainLog);
                    return;
                }

                this.leagueOfLegendsExePath = value;
                this.OnPropertyChanged();
            }
        }

        public bool LibraryCheck
        {
            get
            {
                return this.libraryCheck;
            }

            set
            {
                this.libraryCheck = value;
                this.OnPropertyChanged();
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.password = value;
                this.OnPropertyChanged();
            }
        }

        [XmlArrayItem("Profiles", IsNullable = true)]
        public ObservableCollection<Profile> Profiles
        {
            get
            {
                return this.profiles;
            }

            set
            {
                this.profiles = value;
                this.OnPropertyChanged();
            }
        }

        public string RandomName { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public string SearchText
        {
            get
            {
                return this.searchText;
            }

            set
            {
                this.searchText = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedColor
        {
            get
            {
                return this.selectedColor;
            }

            set
            {
                this.selectedColor = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedLanguage
        {
            get
            {
                return this.selectedLanguage;
            }

            set
            {
                this.selectedLanguage = value;
                this.OnPropertyChanged();
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public Profile SelectedProfile
        {
            get
            {
                if (this.SelectedProfileId >= this.Profiles.Count)
                {
                    return this.Profiles.FirstOrDefault();
                }

                return this.Profiles[this.SelectedProfileId];
            }

            set
            {
                var index = this.Profiles.IndexOf(value);
                this.SelectedProfileId = index < 0 ? 0 : index;
                this.OnPropertyChanged();
                this.OnPropertyChanged("SelectedProfileId");
            }
        }

        public int SelectedProfileId
        {
            get
            {
                return this.selectedProfileId;
            }

            set
            {
                this.selectedProfileId = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("SelectedProfile");
            }
        }

        public ConfigSettings Settings
        {
            get
            {
                return this.settings;
            }

            set
            {
                this.settings = value;
                this.OnPropertyChanged();
            }
        }

        public bool ShowDevOptions
        {
            get
            {
                return this.showDevOptions;
            }

            set
            {
                this.showDevOptions = value;
                this.OnPropertyChanged();
            }
        }

        public bool TosAccepted
        {
            get
            {
                return this.tosAccepted;
            }

            set
            {
                this.tosAccepted = value;
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
                return this.updateOnLoad;
            }

            set
            {
                this.updateOnLoad = value;
                this.OnPropertyChanged();
            }
        }

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

        public string Username
        {
            get
            {
                return this.username;
            }

            set
            {
                this.username = value;
                this.OnPropertyChanged();
            }
        }

        public bool UtilityCheck
        {
            get
            {
                return this.utilityCheck;
            }

            set
            {
                this.utilityCheck = value;
                this.OnPropertyChanged();
            }
        }

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
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
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

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                // ignore
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

        private bool IsOnScreen()
        {
            var screens = Screen.AllScreens;
            foreach (var screen in screens)
            {
                var formTopLeft = new Point((int)this.WindowLeft, (int)this.WindowTop);

                if (screen.WorkingArea.Contains(formTopLeft))
                {
                    return true;
                }
            }

            return false;
        }
    }
}