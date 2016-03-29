// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathRandomizer.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;

    using LeagueSharp.Loader.Data;

    using PlaySharp.Toolkit.StrongName;

    internal class PathRandomizer
    {
        public static bool CopyFiles()
        {
            var result = true;

            if (!File.Exists(Directories.CoreFilePath))
            {
                return false;
            }

            if (!File.Exists(Directories.CoreBridgeFilePath))
            {
                return false;
            }

            try
            {
                result = result && Utility.OverwriteFile(
                             Directories.BootstrapFilePath, 
                             Directories.BootstrapRandomFilePath, 
                             true);

                result = result && Utility.OverwriteFile(
                             Directories.AppDomainFilePath, 
                             Directories.AppDomainRandomFilePath, 
                             true);

                result = result && Utility.OverwriteFile(
                             Directories.CoreFilePath, 
                             Directories.CoreRandomFilePath, 
                             true);

                result = result && Utility.OverwriteFile(
                             Directories.CoreBridgeFilePath,
                             Directories.CoreBridgeRandomFilePath,
                             true);

                var fixedAssemblyData = Utility.ReplaceFilling(Directories.CoreBridgeRandomFilePath, Directories.CoreFileName, Directories.CoreRandomFileName);
                File.WriteAllBytes(Directories.CoreBridgeRandomFilePath, fixedAssemblyData);

                Utility.CreateFileFromResource(Directories.StrongNameKeyFilePath, "LeagueSharp.Loader.Resources.key.snk", true);
                result = result && StrongNameUtility.ReSign(Directories.CoreBridgeRandomFilePath, Directories.StrongNameKeyFilePath);

                File.Delete(Directories.StrongNameKeyFilePath);

                return result;
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, e.Message);
                return false;
            }
        }
    }
}