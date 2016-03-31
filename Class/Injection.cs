// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Injection.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using LeagueSharp.Loader.Data;
    using LeagueSharp.Sandbox.Shared;

    public static class Injection
    {
        private static IntPtr bootstrapper;

        private static GetFilePathDelegate getFilePath;

        private static HasModuleDelegate hasModule;

        private static InjectDLLDelegate injectDLL;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool GetFilePathDelegate(int processId, [Out] StringBuilder path, int size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool HasModuleDelegate(int processId, string path);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool InjectDLLDelegate(int processId, string path);

        public static bool IsInjected => LeagueProcess.Any(IsProcessInjected);

        public static bool PrepareDone { get; set; }

        public static SharedMemory<SharedMemoryLayout> SharedMemory { get; set; }

        private static List<Process> LeagueProcess => Process.GetProcessesByName("League of Legends").ToList();

        public static void Pulse()
        {
            if (injectDLL == null || hasModule == null)
            {
                ResolveInjectDLL();
            }

            if (LeagueProcess == null)
            {
                return;
            }

            // Don't inject untill we checked that there are not updates for the loader.
            if (Updater.Updating || !Updater.CheckedForUpdates || !PrepareDone)
            {
                return;
            }

            foreach (var instance in LeagueProcess)
            {
                try
                {
                    if (!IsProcessInjected(instance))
                    {
                        Config.Instance.LeagueOfLegendsExePath = GetFilePath(instance);

                        if (Config.Instance.UpdateCoreOnInject)
                        {
                            try
                            {
                                Updater.UpdateCore(Config.Instance.LeagueOfLegendsExePath, true).Wait();
                            }
                            catch (Exception e)
                            {
                                Utility.Log(LogStatus.Error, e.Message);
                            }
                        }

                        if (injectDLL != null && File.Exists(Directories.CoreRandomFilePath))
                        {
                            injectDLL(instance.Id, Directories.CoreRandomFilePath);
                            Utility.Log(LogStatus.Info, $"Inject {instance.Id} [{Directories.CoreRandomFilePath}]");
                        }
                    }
                }
                catch (Exception e)
                {
                    Utility.Log(LogStatus.Error, e.Message);

                    // ignored
                }
            }
        }

        public static void Unload()
        {
            if (bootstrapper != IntPtr.Zero)
            {
                try
                {
                    Win32Imports.FreeLibrary(bootstrapper);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static string GetFilePath(Process process)
        {
            var sb = new StringBuilder(255);
            getFilePath(process.Id, sb, sb.Capacity);
            return sb.ToString();
        }

        private static bool IsProcessInjected(Process leagueProcess)
        {
            if (leagueProcess != null)
            {
                try
                {
                    return hasModule(leagueProcess.Id, Directories.CoreRandomFileName);
                }
                catch (Exception e)
                {
                    Utility.Log(LogStatus.Error, e.Message);
                }
            }

            return false;
        }

        private static void ResolveInjectDLL()
        {
            try
            {
                SharedMemory = new SharedMemory<SharedMemoryLayout>("LeagueSharpBootstrap");
                SharedMemory.Data = new SharedMemoryLayout(
                    Directories.AppDomainRandomFilePath, 
                    Directories.BootstrapRandomFilePath, 
                    Config.Instance.Username, 
                    Config.Instance.Password);

                bootstrapper = Win32Imports.LoadLibrary(Directories.BootstrapFilePath);
                if (!(bootstrapper != IntPtr.Zero))
                {
                    return;
                }

                var procAddress = Win32Imports.GetProcAddress(bootstrapper, "InjectModule");
                if (!(procAddress != IntPtr.Zero))
                {
                    return;
                }

                injectDLL =
                    Marshal.GetDelegateForFunctionPointer(procAddress, typeof(InjectDLLDelegate)) as InjectDLLDelegate;

                procAddress = Win32Imports.GetProcAddress(bootstrapper, "HasModule");
                if (!(procAddress != IntPtr.Zero))
                {
                    return;
                }

                hasModule =
                    Marshal.GetDelegateForFunctionPointer(procAddress, typeof(HasModuleDelegate)) as HasModuleDelegate;

                procAddress = Win32Imports.GetProcAddress(bootstrapper, "GetFilePath");
                if (!(procAddress != IntPtr.Zero))
                {
                    return;
                }

                getFilePath =
                    Marshal.GetDelegateForFunctionPointer(procAddress, typeof(GetFilePathDelegate)) as
                    GetFilePathDelegate;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct SharedMemoryLayout
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string SandboxPath;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string BootstrapPath;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public readonly string User;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public readonly string Password;

        [MarshalAs(UnmanagedType.Bool)]
        public readonly bool IsLoaded;

        public SharedMemoryLayout(
            string sandboxPath, 
            string bootstrapPath, 
            string user, 
            string password, 
            bool isLoaded = false)
        {
            this.SandboxPath = sandboxPath;
            this.BootstrapPath = bootstrapPath;
            this.User = user;
            this.Password = password;
            this.IsLoaded = isLoaded;
        }
    }
}