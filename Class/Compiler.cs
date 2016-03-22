// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Compiler.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;

    using LeagueSharp.Loader.Data;

    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Logging;

    #endregion

    internal class Compiler
    {
        private static readonly List<string> ItemsTypeBlackList = new List<string>
                                                                  {
                                                                      "PreBuildEvent", 
                                                                      "PostBuildEvent", 
                                                                      "PreLinkEvent", 
                                                                      "CustomBuildStep"
                                                                  };

        public static bool Compile(Project project, string logfile, Log log)
        {
            try
            {
                if (project != null)
                {
                    foreach (var item in project.Items)
                    {
                        try
                        {
                            if (ItemsTypeBlackList.FindIndex(listItem => listItem.Equals(item.ItemType, StringComparison.InvariantCultureIgnoreCase))
                                >= 0)
                            {
                                Utility.Log(
                                    LogStatus.Error, 
                                    $"Compile - Blacklisted item type detected - {project.FullPath}");

                                return false;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    var doLog = false;
                    var logErrorFile = Path.Combine(Directories.LogsDirectory, "Error - " + Path.GetFileName(logfile));
                    if (File.Exists(logErrorFile))
                    {
                        File.Delete(logErrorFile);
                    }

                    if (!string.IsNullOrWhiteSpace(logfile))
                    {
                        var logDir = Path.GetDirectoryName(logfile);
                        if (!string.IsNullOrWhiteSpace(logDir))
                        {
                            doLog = true;
                            if (!Directory.Exists(logDir))
                            {
                                Directory.CreateDirectory(logDir);
                            }

                            var fileLogger = new FileLogger { Parameters = @"logfile=" + logfile, ShowSummary = true };
                            ProjectCollection.GlobalProjectCollection.RegisterLogger(fileLogger);
                        }
                    }

                    var result = project.Build();
                    ProjectCollection.GlobalProjectCollection.UnregisterAllLoggers();
                    ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
                    Utility.Log(
                        result ? LogStatus.Info : LogStatus.Error, 
                        result
                            ? $"Compile - {project.FullPath}"
                            : $"Compile - Check ./logs/ for details - {project.FullPath}");

                    if (!result && doLog && File.Exists(logfile))
                    {
                        var pathDir = Path.GetDirectoryName(logfile);
                        if (!string.IsNullOrWhiteSpace(pathDir))
                        {
                            File.Move(
                                logfile, 
                                Path.Combine(Directories.LogsDirectory, "Error - " + Path.GetFileName(logfile)));
                        }
                    }
                    else if (result && File.Exists(logfile))
                    {
                        File.Delete(logfile);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, ex.Message);
            }

            return false;
        }

        public static string GetOutputFilePath(Project project)
        {
            if (project != null)
            {
                var extension = project.GetPropertyValue("OutputType").ToLower().Contains("exe")
                                    ? ".exe"
                                    : (project.GetPropertyValue("OutputType").ToLower() == "library"
                                           ? ".dll"
                                           : string.Empty);
                var pathDir = Path.GetDirectoryName(project.FullPath);
                if (!string.IsNullOrWhiteSpace(extension) && !string.IsNullOrWhiteSpace(pathDir))
                {
                    return Path.Combine(
                        pathDir, 
                        project.GetPropertyValue("OutputPath"), 
                        project.GetPropertyValue("AssemblyName") + extension);
                }
            }

            return string.Empty;
        }
    }
}