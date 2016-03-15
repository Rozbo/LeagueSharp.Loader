// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Updater.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using LeagueSharp.Loader.Data;
    using LeagueSharp.Loader.Views;

    using PlaySharp.Service.WebService.Model;

    #endregion

    internal class Updater
    {
        public static bool CheckedForUpdates = false;

        public static string SetupFile = Path.Combine(Directories.CurrentDirectory, "LeagueSharp-update.exe");

        public static string UpdateZip = Path.Combine(Directories.CoreDirectory, "update.zip");

        public static bool Updating = false;

        private static IReadOnlyList<string> UpdateWhiteList = new[]
                                                               {
                                                                   "https://github.com/joduskame/", 
                                                                   "https://github.com/LeagueSharp/", 
                                                                   "https://github.com/Esk0r/"
                                                               };

        public delegate void RepositoriesUpdateDelegate(List<string> list);

        public enum CoreUpdateState
        {
            Operational, 

            Maintenance, 

            Unknown
        }

        public static async Task<bool> IsSupported(string path)
        {
            if (Directory.Exists(Path.Combine(Directories.CurrentDirectory, "iwanttogetbanned")))
            {
                return true;
            }

            try
            {
                if (!WebService.Client.IsAuthenticated)
                {
                    Utility.Log(LogStatus.Error, "IsSupported", "WebService authentication failed", Logs.MainLog);
                    return false;
                }

                var leagueChecksum = Utility.Md5Checksum(path);
                var coreChecksum = Utility.Md5Checksum(Directories.CoreFilePath);
                var core = await WebService.Client.CoreAsync(leagueChecksum);

                if (leagueChecksum == "-1")
                {
                    return false;
                }

                if (core == null)
                {
                    Utility.Log(LogStatus.Error, "IsSupported", "Failed to receive Core version from WebService", Logs.MainLog);
                    return false;
                }

                return core.HashCore == coreChecksum;
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, "IsSupported", e.Message, Logs.MainLog);
            }

            return false;
        }

        public static async Task<UpdateResponse> UpdateCore(string path, bool showMessages)
        {
            if (Directory.Exists(Path.Combine(Directories.CurrentDirectory, "iwanttogetbanned")))
            {
                return new UpdateResponse(CoreUpdateState.Operational, Utility.GetMultiLanguageText("NotUpdateNeeded"));
            }

            try
            {
                if (!WebService.Client.IsAuthenticated)
                {
                    Utility.Log(LogStatus.Error, "UpdateCore", "WebService authentication failed", Logs.MainLog);
                    return new UpdateResponse(CoreUpdateState.Unknown, "WebService authentication failed");
                }

                var outdated = false;
                var leagueChecksum = Utility.Md5Checksum(path);
                var coreChecksum = Utility.Md5Checksum(Directories.CoreFilePath);
                var coreBridgeChecksum = Utility.Md5Checksum(Directories.CoreBridgeFilePath);
                var core = await WebService.Client.CoreAsync(leagueChecksum);

                if (core == null)
                {
                    return new UpdateResponse(
                        CoreUpdateState.Maintenance, 
                        Utility.GetMultiLanguageText("WrongVersion") + Environment.NewLine + leagueChecksum);
                }

                if (!File.Exists(Directories.CoreFilePath) || !File.Exists(Directories.CoreBridgeFilePath))
                {
                    outdated = true;
                }

                if (!string.IsNullOrEmpty(core.HashCore) && core.HashCore != coreChecksum)
                {
                    outdated = true;
                }

                if (!string.IsNullOrEmpty(core.HashCoreBridge) && core.HashCoreBridge != coreBridgeChecksum)
                {
                    outdated = true;
                }

                if (outdated && UpdateWhiteList.Any(u => core.Url.StartsWith(u)))
                {
                    try
                    {
                        var result = CoreUpdateState.Unknown;

                        await Application.Current.Dispatcher.Invoke(
                            async () =>
                            {
                                var window = new UpdateWindow(UpdateAction.Core, core.Url);
                                window.Show();

                                if (await window.Update())
                                {
                                    result = CoreUpdateState.Operational;
                                }
                            });

                        return new UpdateResponse(result, Utility.GetMultiLanguageText("UpdateSuccess"));
                    }
                    catch (Exception e)
                    {
                        var message = Utility.GetMultiLanguageText("FailedToDownload") + e;

                        if (showMessages)
                        {
                            MessageBox.Show(message);
                        }

                        return new UpdateResponse(CoreUpdateState.Unknown, message);
                    }
                    finally
                    {
                        if (File.Exists(UpdateZip))
                        {
                            File.Delete(UpdateZip);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, "UpdateCore", e.Message, Logs.MainLog);
            }

            return new UpdateResponse(CoreUpdateState.Operational, Utility.GetMultiLanguageText("NotUpdateNeeded"));
        }

        public static async Task UpdateLoader(string url)
        {
            if (UpdateWhiteList.Any(url.StartsWith))
            {
                var window = new UpdateWindow(UpdateAction.Loader, url);
                window.Show();
                await window.Update();
            }
        }

        public static async Task UpdateRepositories()
        {
            try
            {
                var repos = await WebService.Client.RepositoriesAsync();

                Config.Instance.KnownRepositories = new ObservableCollection<RepositoryEntry>(repos.Where(r => r.Display));
                Config.Instance.BlockedRepositories = new ObservableCollection<RepositoryEntry>(repos.Where(r => r.HasRedirect));
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, "UpdateRepositories", e.Message, Logs.MainLog);
            }
        }

        public static async Task UpdateWebService()
        {
            try
            {
                var assemblies = new ObservableCollection<AssemblyEntry>();

                await Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            var entries = WebService.Assemblies.Where(a => a.Approved && !a.Deleted).ToList();
                            entries.ShuffleRandom();

                            assemblies = new ObservableCollection<AssemblyEntry>(entries);
                        }
                        catch (Exception e)
                        {
                            Utility.Log(LogStatus.Error, "UpdateWebService", e.Message, Logs.MainLog);
                        }
                    });

                Config.Instance.DatabaseAssemblies = assemblies;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class UpdateResponse
        {
            public UpdateResponse(CoreUpdateState state, string message = "")
            {
                this.State = state;
                this.Message = message;
            }

            public string Message { get; set; }

            public CoreUpdateState State { get; set; }
        }
    }
}