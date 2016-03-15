// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Dependency.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class.Installer
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;

    using LeagueSharp.Loader.Views;

    using PlaySharp.Service.WebService.Model;

    public class Dependency
    {
        public AssemblyEntry AssemblyEntry { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public string Repository { get; set; }

        public static Dependency FromAssemblyEntry(AssemblyEntry assembly)
        {
            try
            {
                var repositoryMatch = Regex.Match(assembly.GithubUrl, @"^(http[s]?)://(?<host>.*?)/(?<author>.*?)/(?<repo>.*?)(/{1}|$)");
                var projectName = assembly.GithubUrl.Substring(assembly.GithubUrl.LastIndexOf("/") + 1);
                var repositoryUrl = $"https://{repositoryMatch.Groups["host"]}/{repositoryMatch.Groups["author"]}/{repositoryMatch.Groups["repo"]}";

                return new Dependency
                       {
                           AssemblyEntry = assembly, 
                           Repository = repositoryUrl, 
                           Project = projectName.WebDecode(), 
                           Name = assembly.Name.WebDecode(), 
                           Description = assembly.Description.WebDecode()
                       };
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return null;
        }

        public async Task<bool> InstallAsync()
        {
            try
            {
                await InstallerWindow.InstallAssembly(this.AssemblyEntry, true);

                // await Application.Current.Dispatcher.InvokeAsync(() => InstallerWindow.InstallAssembly(this.AssemblyEntry, true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{this.Name} - {this.Project} - {this.Repository}";
        }
    }
}