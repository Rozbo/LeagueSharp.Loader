// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LeagueSharpAssemblies.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using LeagueSharp.Loader.Data;

    public static class LeagueSharpAssemblies
    {
        public static List<LeagueSharpAssembly> GetAssemblies(string directory, string url = "")
        {
            var projectFiles = new List<string>();
            var foundAssemblies = new List<LeagueSharpAssembly>();

            try
            {
                projectFiles.AddRange(Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories));
                foreach (var projectFile in projectFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(projectFile);
                    var assembly = new LeagueSharpAssembly(name, projectFile, url);

                    if (!string.IsNullOrEmpty(url))
                    {
                        var entry = Config.Instance.DatabaseAssemblies?.FirstOrDefault(a => a.GithubUrl.Contains(url) && a.Name == name)
                                    ?? Config.Instance.DatabaseAssemblies?.FirstOrDefault(
                                        a => a.GithubUrl.Contains(url) && Path.GetFileNameWithoutExtension(a.GithubUrl) == name);

                        if (entry != null)
                        {
                            assembly.Author = entry.AuthorName;
                            assembly.Description = entry.Description;
                            assembly.DisplayName = entry.Name;
                        }
                        else
                        {
                            var repositoryMatch = Regex.Match(url, @"^(http[s]?)://(?<host>.*?)/(?<author>.*?)/(?<repo>.*?)(/{1}|$)");
                            if (repositoryMatch.Success)
                            {
                                assembly.Author = repositoryMatch.Groups["author"].Value;
                            }
                        }
                    }

                    foundAssemblies.Add(assembly);
                }
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, e.Message);
            }

            return foundAssemblies;
        }
    }
}