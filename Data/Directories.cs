// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Directories.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    using System;
    using System.IO;

    using LeagueSharp.Loader.Class;

    public static class Directories
    {
        private static string appDomainRandomFileName;

        private static string bootstrapRandomFileName;

        private static string coreBridgeRandomFileName;

        private static string coreRandomFileName;

        static Directories()
        {
            Directory.CreateDirectory(AssembliesDirectory);
            Directory.CreateDirectory(RepositoriesDirectory);
            Directory.CreateDirectory(LogsDirectory);
        }

        public static string AppDataDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "LS" + Environment.UserName.GetHashCode().ToString("X"));

        public static string AppDomainFileName => "LeagueSharp.Sandbox.dll";

        public static string AppDomainFilePath => Path.Combine(CoreDirectory, AppDomainFileName);

        public static string AppDomainRandomFileName
        {
            get
            {
                if (appDomainRandomFileName == null)
                {
                    appDomainRandomFileName = Utility.GetUniqueFile(AppDomainFileName);
                }

                return appDomainRandomFileName;
            }
        }

        public static string AppDomainRandomFilePath => Path.Combine(AssembliesDirectory, AppDomainRandomFileName);

        public static string AssembliesDirectory => Path.Combine(AppDataDirectory, "bin");

        public static string BootstrapFileName => "LeagueSharp.Bootstrap.dll";

        public static string BootstrapFilePath => Path.Combine(CoreDirectory, BootstrapFileName);

        public static string BootstrapRandomFileName
        {
            get
            {
                if (bootstrapRandomFileName == null)
                {
                    bootstrapRandomFileName = Utility.GetUniqueFile(BootstrapFileName);
                }

                return bootstrapRandomFileName;
            }
        }

        public static string BootstrapRandomFilePath => Path.Combine(AssembliesDirectory, BootstrapRandomFileName);

        public static string ConfigFileName => "config.xml";

        public static string ConfigFilePath => Path.Combine(CurrentDirectory, ConfigFileName);

        public static string CoreBridgeFileName => "LeagueSharp.dll";

        public static string CoreBridgeFilePath => Path.Combine(CoreDirectory, CoreBridgeFileName);

        public static string CoreBridgeRandomFileName
        {
            get
            {
                if (coreBridgeRandomFileName == null)
                {
                    coreBridgeRandomFileName = Utility.GetUniqueFile(CoreBridgeFileName);
                }

                return coreBridgeRandomFileName;
            }
        }

        public static string CoreBridgeRandomFilePath => Path.Combine(AssembliesDirectory, CoreBridgeRandomFileName);

        public static string CoreDirectory => Path.Combine(CurrentDirectory, "System");

        public static string CoreFileName => "LeagueSharp.Core.dll";

        public static string CoreFilePath => Path.Combine(CoreDirectory, CoreFileName);

        public static string CoreRandomFileName
        {
            get
            {
                if (coreRandomFileName == null)
                {
                    coreRandomFileName = Utility.GetUniqueFile(CoreFileName);
                }

                return coreRandomFileName;
            }
        }

        public static string CoreRandomFilePath => Path.Combine(AssembliesDirectory, CoreRandomFileName);

        public static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;

        public static string LoaderFileName => "loader.exe";

        public static string LoaderFilePath => Path.Combine(CurrentDirectory, LoaderFileName);

        public static string LoaderRandomConfigFilePath => Path.Combine(CurrentDirectory, $"{Config.Instance?.RandomName}.exe.config");

        public static string LoaderRandomFilePath => Path.Combine(CurrentDirectory, $"{Config.Instance?.RandomName}.exe");

        public static string LoaderRandomPdbFilePath => Path.Combine(CurrentDirectory, $"{Config.Instance?.RandomName}.pdb");

        public static string LocalRepositoriesDirectory => Path.Combine(CurrentDirectory, "LocalAssemblies");

        public static string LogsDirectory => Path.Combine(CurrentDirectory, "Logs");

        public static string RepositoriesDirectory => Path.Combine(AppDataDirectory, "Repositories");

        public static string StrongNameKeyFileName => "key.snk";

        public static string StrongNameKeyFilePath => Path.Combine(Path.GetTempPath(), StrongNameKeyFileName);
    }
}