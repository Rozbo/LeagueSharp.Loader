// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LeagueSharpAssembly.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Xml.Serialization;

    using LeagueSharp.Loader.Data;

    using Microsoft.Build.Evaluation;

    using PlaySharp.Service.WebService.Model;

    [XmlType(AnonymousType = true)]
    [Serializable]
    public class LeagueSharpAssembly : INotifyPropertyChanged
    {
        private string author;

        private string description;

        private string displayName = string.Empty;

        private bool injectChecked;

        private bool installChecked;

        private string name;

        private string pathToBinary = null;

        private string pathToProjectFile = string.Empty;

        private string svnUrl;

        private AssemblyType? type = null;

        public LeagueSharpAssembly()
        {
            this.Status = AssemblyStatus.Ready;
        }

        public LeagueSharpAssembly(string name, string path, string svnUrl)
        {
            this.Name = name;
            this.PathToProjectFile = path;
            this.SvnUrl = svnUrl;
            this.Description = string.Empty;
            this.Status = AssemblyStatus.Ready;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Author
        {
            get
            {
                if (string.IsNullOrEmpty(this.SvnUrl) && string.IsNullOrEmpty(this.author))
                {
                    return "Local";
                }

                try
                {
                    if (string.IsNullOrEmpty(this.author))
                    {
                        var assembly =
                            WebService.Assemblies.FirstOrDefault(
                                a => Path.GetFileName(a.GithubUrl) == Path.GetFileName(this.PathToProjectFile) && a.GithubUrl.Contains(this.SvnUrl));
                        if (assembly != null)
                        {
                            this.author = assembly.AuthorName;
                        }
                        else
                        {
                            var repositoryMatch = Regex.Match(this.SvnUrl, @"^(http[s]?)://(?<host>.*?)/(?<author>.*?)/(?<repo>.*?)(/{1}|$)");
                            if (repositoryMatch.Success)
                            {
                                this.author = repositoryMatch.Groups["author"].Value;
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return this.author;
            }

            set
            {
                this.author = value;
                this.OnPropertyChanged();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
                this.OnPropertyChanged();
            }
        }

        public string DisplayName
        {
            get
            {
                return this.displayName == string.Empty ? this.Name : this.displayName;
            }

            set
            {
                this.displayName = value;
                this.OnPropertyChanged();
            }
        }

        public bool InjectChecked
        {
            get
            {
                if (this.Type == AssemblyType.Library)
                {
                    return true;
                }

                return this.injectChecked;
            }

            set
            {
                this.injectChecked = value;
                this.OnPropertyChanged();
            }
        }

        public bool InstallChecked
        {
            get
            {
                return this.installChecked;
            }

            set
            {
                this.installChecked = value;
                this.OnPropertyChanged();
            }
        }

        public string Location => this.SvnUrl == string.Empty ? "Local" : this.SvnUrl;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public string PathToBinary
        {
            get
            {
                if (this.pathToBinary == null)
                {
                    var binFileName = Path.GetFileName(Compiler.GetOutputFilePath(this.GetProject()));

                    switch (this.Type)
                    {
                        case AssemblyType.Library:
                            this.pathToBinary = Path.Combine(Directories.CoreDirectory, binFileName);
                            break;

                        default:
                            this.pathToBinary = Path.Combine(
                                Directories.AssembliesDirectory, 
                                this.PathToProjectFile.GetHashCode().ToString("X") + binFileName);
                            break;
                    }
                }

                return this.pathToBinary;
            }
        }

        public string PathToProjectFile
        {
            get
            {
                if (File.Exists(this.pathToProjectFile))
                {
                    return this.pathToProjectFile;
                }

                try
                {
                    var folderToSearch = Path.Combine(Directories.RepositoriesDirectory, this.SvnUrl.GetHashCode().ToString("X"), "trunk");

                    if (Directory.Exists(folderToSearch))
                    {
                        var projectFile =
                            Directory.GetFiles(folderToSearch, "*.csproj", SearchOption.AllDirectories)
                                     .FirstOrDefault(file => Path.GetFileNameWithoutExtension(file) == this.Name);

                        if (!string.IsNullOrEmpty(projectFile))
                        {
                            this.OnPropertyChanged();
                            this.pathToProjectFile = projectFile;
                            return projectFile;
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return this.pathToProjectFile;
            }

            set
            {
                if (!value.Contains("%AppData%"))
                {
                    this.pathToProjectFile = value;
                }
                else
                {
                    this.pathToProjectFile = value.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                }

                this.OnPropertyChanged();
            }
        }

        public AssemblyStatus Status { get; set; }

        public string SvnUrl
        {
            get
            {
                var redirect = Config.Instance.BlockedRepositories.Where(r => r.HasRedirect).FirstOrDefault(r => r.Url == this.svnUrl);
                if (redirect != null)
                {
                    this.OnPropertyChanged();
                    return redirect.Redirect;
                }

                return this.svnUrl;
            }

            set
            {
                this.svnUrl = value;
                this.OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public AssemblyType Type
        {
            get
            {
                if (this.type == null)
                {
                    var assembly = WebService.Assemblies.FirstOrDefault(a => a.Name == this.Name || a.Name == this.DisplayName);
                    if (assembly != null)
                    {
                        this.type = assembly.Type;
                        return assembly.Type;
                    }

                    var project = this.GetProject();
                    if (project != null)
                    {
                        this.type = project.GetPropertyValue("OutputType").ToLower().Contains("exe") ? AssemblyType.Champion : AssemblyType.Library;
                    }
                }

                return this.type ?? AssemblyType.Unknown;
            }
        }

        public string Version
        {
            get
            {
                if (this.Status != AssemblyStatus.Ready)
                {
                    return this.Status.ToString();
                }

                if (!string.IsNullOrEmpty(this.PathToBinary) && File.Exists(this.PathToBinary))
                {
                    return AssemblyName.GetAssemblyName(this.PathToBinary).Version.ToString();
                }

                return "?";
            }
        }

        public static LeagueSharpAssembly FromAssemblyEntry(AssemblyEntry entry)
        {
            try
            {
                var repositoryMatch = Regex.Match(entry.GithubUrl, @"^(http[s]?)://(?<host>.*?)/(?<author>.*?)/(?<repo>.*?)(/{1}|$)");
                var repositoryUrl = $"https://{repositoryMatch.Groups["host"]}/{repositoryMatch.Groups["author"]}/{repositoryMatch.Groups["repo"]}";
                var repositoryDirectory = Path.Combine(Directories.RepositoriesDirectory, repositoryUrl.GetHashCode().ToString("X"), "trunk");
                var path = Path.Combine(
                    repositoryDirectory, 
                    entry.GithubUrl.Replace(repositoryUrl, string.Empty).Replace("/blob/master/", string.Empty).Replace("/", "\\"));

                return new LeagueSharpAssembly(entry.Name, path, repositoryUrl);
            }
            catch
            {
                return null;
            }
        }

        public bool Compile()
        {
            this.Status = AssemblyStatus.Compiling;
            this.OnPropertyChanged("Version");
            var project = this.GetProject();

            if (Compiler.Compile(project, Path.Combine(Directories.LogsDirectory, this.Name + ".txt"), Logs.MainLog))
            {
                var result = true;
                var assemblySource = Compiler.GetOutputFilePath(project);
                var assemblyDestination = this.PathToBinary;
                var pdbSource = Path.ChangeExtension(assemblySource, ".pdb");
                var pdbDestination = Path.ChangeExtension(assemblyDestination, ".pdb");

                if (File.Exists(assemblySource))
                {
                    result = Utility.OverwriteFile(assemblySource, assemblyDestination);
                }

                if (File.Exists(pdbSource))
                {
                    Utility.OverwriteFile(pdbSource, pdbDestination);
                }

                Utility.ClearDirectory(Path.Combine(project.DirectoryPath, "bin"));
                Utility.ClearDirectory(Path.Combine(project.DirectoryPath, "obj"));

                if (result)
                {
                    this.Status = AssemblyStatus.Ready;
                }
                else
                {
                    this.Status = AssemblyStatus.CompilingError;
                }

                this.OnPropertyChanged("Version");
                this.OnPropertyChanged("Type");
                return result;
            }

            this.Status = AssemblyStatus.CompilingError;
            this.OnPropertyChanged("Version");
            return false;
        }

        public LeagueSharpAssembly Copy()
        {
            return new LeagueSharpAssembly(this.Name, this.PathToProjectFile, this.SvnUrl);
        }

        public override bool Equals(object obj)
        {
            if (obj is LeagueSharpAssembly)
            {
                return ((LeagueSharpAssembly)obj).PathToProjectFile == this.PathToProjectFile;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.PathToProjectFile.GetHashCode();
        }

        public Project GetProject()
        {
            if (File.Exists(this.PathToProjectFile))
            {
                try
                {
                    var pf = new ProjectFile(this.PathToProjectFile, Logs.MainLog)
                             {
                                 Configuration = Config.Instance.EnableDebug ? "Debug" : "Release", 
                                 PlatformTarget = "x86", 
                                 ReferencesPath = Directories.CoreDirectory
                             };
                    pf.Change();

                    return pf.Project;
                }
                catch (Exception e)
                {
                    Utility.Log(LogStatus.Error, e.Message);
                }
            }

            return null;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update()
        {
            if (this.Status == AssemblyStatus.Updating || this.SvnUrl == string.Empty)
            {
                return;
            }

            this.Status = AssemblyStatus.Updating;
            this.OnPropertyChanged("Version");
            try
            {
                GitUpdater.Update(this.SvnUrl);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            this.Status = AssemblyStatus.Ready;
            this.OnPropertyChanged("Version");
        }
    }
}