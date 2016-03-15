// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Directories.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class Directories
    {
        public static readonly string AppDataDirectory =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "LS" + Environment.UserName.GetHashCode().ToString("X")) + "\\";

        public static readonly string AssembliesDir = Path.Combine(AppDataDirectory, "1") + "\\";

        public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string CoreDirectory = Path.Combine(CurrentDirectory, "System") + "\\";

        public static readonly string BootstrapFilePath = Path.Combine(CoreDirectory, "LeagueSharp.Bootstrap.dll");

        public static readonly string ConfigFilePath = Path.Combine(CurrentDirectory, "config.xml");

        public static readonly string CoreBridgeFilePath = Path.Combine(CoreDirectory, "Leaguesharp.dll");

        public static readonly string CoreFilePath = Path.Combine(CoreDirectory, "Leaguesharp.Core.dll");

        public static readonly string LoaderFilePath = Path.Combine(
            CurrentDirectory, 
            Process.GetCurrentProcess().ProcessName);

        public static readonly string LocalRepoDir = Path.Combine(CurrentDirectory, "LocalAssemblies") + "\\";

        public static readonly string LogsDir = Path.Combine(CurrentDirectory, "Logs") + "\\";

        public static readonly string RepositoryDir = Path.Combine(AppDataDirectory, "Repositories") + "\\";

        public static readonly string SandboxFilePath = Path.Combine(CoreDirectory, "LeagueSharp.Sandbox.dll");

        static Directories()
        {
            Directory.CreateDirectory(AssembliesDir);
            Directory.CreateDirectory(RepositoryDir);
            Directory.CreateDirectory(LogsDir);
        }

        public static string AssemblyConfigFile => Path.Combine(CurrentDirectory, $"{Config.Instance?.RandomName}.exe.config");

        public static string AssemblyFile => Path.Combine(CurrentDirectory, $"{Config.Instance?.RandomName}.exe");

        public static string AssemblyPdbFile => Path.Combine(CurrentDirectory, $"{Config.Instance?.RandomName}.pdb");
    }
}