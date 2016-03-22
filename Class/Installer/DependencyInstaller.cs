// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyInstaller.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class.Installer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using LeagueSharp.Loader.Data;

    using PlaySharp.Service.WebService.Model;

    public class DependencyInstaller
    {
        static DependencyInstaller()
        {
            UpdateReferenceCache();
        }

        public DependencyInstaller(List<string> projects)
        {
            this.Projects = projects;
        }

        public static List<Dependency> Cache { get; set; } = new List<Dependency>();

        public IReadOnlyList<string> Projects { get; set; }

        public async Task<bool> SatisfyAsync()
        {
            var successful = true;

            foreach (var project in this.Projects)
            {
                try
                {
                    var projectReferences = this.ParseReferences(project);
                    var missingReferences =
                        projectReferences.Where(r => this.IsKnown(r) && !this.IsInstalled(r)).Select(r => Cache.First(d => r == d.Name));

                    foreach (var dependency in missingReferences)
                    {
                        if (!await dependency.InstallAsync())
                        {
                            successful = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    successful = false;
                    Console.WriteLine(e);
                }
            }

            return successful;
        }

        private static Dependency ParseAssemblyName(AssemblyEntry assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                var project = assembly.GithubUrl;
                project = project.Replace("https://github.com/", "https://raw.githubusercontent.com/");
                project = project.Replace("/blob/master/", "/master/");

                using (var client = new WebClientEx())
                {
                    var dependency = Dependency.FromAssemblyEntry(assembly);
                    var content = client.DownloadString(project);
                    var assemblyNameMatch = Regex.Match(content, "<AssemblyName>(?<name>.*?)</AssemblyName>");
                    dependency.Name = assemblyNameMatch.Groups["name"].Value;

                    return dependency;
                }
            }
            catch
            {
                Utility.Log(LogStatus.Info, $"Invalid Library: {assembly.Id} - {assembly.GithubUrl}");
            }

            return null;
        }

        private static void UpdateReferenceCache()
        {
            var assemblies = new List<AssemblyEntry>();

            try
            {
                assemblies = WebService.Assemblies.Where(a => a.Type == AssemblyType.Library && !a.Deleted && a.Approved).ToList();
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, e.Message);
            }

            Cache.Clear();
            foreach (var lib in assemblies)
            {
                Cache.Add(ParseAssemblyName(lib));
            }

            Cache.RemoveAll(a => a == null);
        }

        private bool IsInstalled(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return Config.Instance.Profiles.First().InstalledAssemblies.Any(a => Path.GetFileNameWithoutExtension(a.PathToBinary) == name);
        }

        private bool IsKnown(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return Cache.Any(d => d.Name == name);
        }

        private List<string> ParseReferences(string project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var projectReferences = new List<string>();

            try
            {
                var matches = Regex.Matches(File.ReadAllText(project), "<Reference Include=\"(?<assembly>.*?)\"(?<space>.*?)>");

                foreach (Match match in matches)
                {
                    var m = match.Groups["assembly"].Value;

                    if (m.Contains(","))
                    {
                        m = m.Split(',')[0];
                    }

                    projectReferences.Add(m);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return projectReferences;
        }
    }
}