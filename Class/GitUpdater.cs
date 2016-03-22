// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitUpdater.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using LeagueSharp.Loader.Data;

    using LibGit2Sharp;

    #endregion

    internal class GitUpdater
    {
        /// <summary>
        ///     Clearing unused folders to reduce file space usage.
        /// </summary>
        /// <param name="repoDirectory">Path to unused folder</param>
        /// <param name="log">Log</param>
        public static void ClearUnusedRepoFolder(string repoDirectory, Log log)
        {
            try
            {
                var dir = repoDirectory.Remove(repoDirectory.LastIndexOf("\\"));
                if (dir.EndsWith("trunk"))
                {
                    return;
                }

                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }

                dir = repoDirectory.Remove(dir.LastIndexOf("\\"));
                Directory.GetFiles(dir).ToList().ForEach(File.Delete);
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, $"{ex.Message} - {repoDirectory}");
            }
        }

        public static void ClearUnusedRepos(List<LeagueSharpAssembly> assemblyList)
        {
            try
            {
                var usedRepos = new List<string>();
                foreach (var assembly in assemblyList.Where(a => !string.IsNullOrEmpty(a.SvnUrl)))
                {
                    usedRepos.Add(assembly.SvnUrl.GetHashCode().ToString("X"));
                }

                var dirs = new List<string>(Directory.EnumerateDirectories(Directories.RepositoriesDirectory));

                foreach (var dir in dirs)
                {
                    if (!usedRepos.Contains(Path.GetFileName(dir)))
                    {
                        Utility.ClearDirectory(dir);
                        Directory.Delete(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, ex.Message);
            }
        }

        internal static string Update(string url)
        {
            var root = Path.Combine(Directories.RepositoriesDirectory, url.GetHashCode().ToString("X"), "trunk");

            if (!IsValid(root))
            {
                var cloneResult = Clone(url, root);

                if (!cloneResult)
                {
                    Utility.Log(LogStatus.Error, $"Failed to Clone - {url}");
                    return root;
                }
            }

            var pullResult = Pull(root);

            if (!pullResult)
            {
                Utility.Log(LogStatus.Error, $"Failed to Pull Updates - {url}");

                Clone(url, root);
            }

            return root;
        }

        private static bool Clone(string url, string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Utility.ClearDirectory(directory);
                    Directory.Delete(directory, true);
                }

                Utility.Log(LogStatus.Info, url);
                Repository.Clone(url, directory);
                return true;
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, e.Message);
                return false;
            }
        }

        private static bool IsValid(string directory)
        {
            try
            {
                if (Repository.IsValid(directory))
                {
                    using (var repo = new Repository(directory))
                    {
                        if (repo.Head == null)
                        {
                            return false;
                        }

                        if (repo.Info.IsHeadDetached)
                        {
                            return false;
                        }

                        if (repo.Info.IsBare)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, e.Message);
                return false;
            }

            return true;
        }

        private static bool Pull(string directory)
        {
            try
            {
                using (var repo = new Repository(directory))
                {
                    Utility.Log(LogStatus.Info, directory);

                    repo.Reset(ResetMode.Hard);
                    repo.RemoveUntrackedFiles();
                    repo.Network.Pull(
                        new Signature(Config.Instance.Username, $"{Config.Instance.Username}@joduska.me", DateTimeOffset.Now), 
                        new PullOptions
                        {
                            MergeOptions =
                                new MergeOptions
                                {
                                    FastForwardStrategy = FastForwardStrategy.Default, 
                                    FileConflictStrategy = CheckoutFileConflictStrategy.Theirs, 
                                    MergeFileFavor = MergeFileFavor.Theirs, 
                                    CommitOnSuccess = true
                                }
                        });

                    repo.Checkout(repo.Head, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

                    if (repo.Info.IsHeadDetached)
                    {
                        Utility.Log(LogStatus.Warning, "Update+Detached");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, e.Message);
                return false;
            }
        }
    }
}