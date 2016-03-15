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
    using System.IO.MemoryMappedFiles;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using LeagueSharp.Loader.Data;

    public static class Injection
    {
        public static MemoryMappedFile mmf = null;

        private static IntPtr bootstrapper;

        private static GetFilePathDelegate getFilePath;

        private static HasModuleDelegate hasModule;

        private static InjectDLLDelegate injectDLL;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool GetFilePathDelegate(int processId, [Out] StringBuilder path, int size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool HasModuleDelegate(int processId, string path);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool InjectDLLDelegate(int processId, string path);

        public static bool IsInjected => LeagueProcess.Any(IsProcessInjected);

        public static bool PrepareDone { get; set; }

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
                                Utility.Log(LogStatus.Error, "UpdateCoreOnInject", e.Message, Logs.MainLog);
                            }
                        }

                        var supported = true;

                        try
                        {
                            supported = Updater.IsSupported(Config.Instance.LeagueOfLegendsExePath).Result;
                        }
                        catch (Exception e)
                        {
                            Utility.Log(LogStatus.Error, "IsSupported", e.Message, Logs.MainLog);
                        }

                        if (injectDLL != null && supported)
                        {
                            injectDLL(instance.Id, PathRandomizer.LeagueSharpCoreDllPath);
                            Utility.Log(LogStatus.Info, "Pulse", $"Inject {instance.Id} [{PathRandomizer.LeagueSharpCoreDllPath}]", Logs.MainLog);
                        }
                    }
                }
                catch (Exception e)
                {
                    Utility.Log(LogStatus.Error, "Pulse", e.Message, Logs.MainLog);

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
                    return hasModule(leagueProcess.Id, PathRandomizer.LeagueSharpCoreDllName);
                }
                catch (Exception e)
                {
                    Utility.Log(LogStatus.Error, "Injector", $"Error - {e}", Logs.MainLog);
                }
            }

            return false;
        }

        private static void ResolveInjectDLL()
        {
            try
            {
                mmf = MemoryMappedFile.CreateOrOpen(
                    "Local\\LeagueSharpBootstrap", 
                    260 * 2, 
                    MemoryMappedFileAccess.ReadWrite);

                var sharedMem = new SharedMemoryLayout(
                    PathRandomizer.LeagueSharpSandBoxDllPath, 
                    PathRandomizer.LeagueSharpBootstrapDllPath, 
                    Config.Instance.Username, 
                    Config.Instance.Password);

                using (var writer = mmf.CreateViewAccessor())
                {
                    var len = Marshal.SizeOf(typeof(SharedMemoryLayout));
                    var arr = new byte[len];
                    var ptr = Marshal.AllocHGlobal(len);
                    Marshal.StructureToPtr(sharedMem, ptr, true);
                    Marshal.Copy(ptr, arr, 0, len);
                    Marshal.FreeHGlobal(ptr);
                    writer.WriteArray(0, arr, 0, arr.Length);
                }

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

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        private struct SharedMemoryLayout
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            private readonly string SandboxPath;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            private readonly string BootstrapPath;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            private readonly string User;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            private readonly string Password;

            public SharedMemoryLayout(string sandboxPath, string bootstrapPath, string user, string password)
            {
                this.SandboxPath = sandboxPath;
                this.BootstrapPath = bootstrapPath;
                this.User = user;
                this.Password = password;
            }
        }
    }
}