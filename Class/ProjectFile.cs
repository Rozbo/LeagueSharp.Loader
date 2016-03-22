// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectFile.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.IO;

    using LeagueSharp.Loader.Data;

    using Microsoft.Build.Evaluation;

    [Serializable]
    internal class ProjectFile
    {
        public readonly Project Project;

        private readonly Log log;

        public ProjectFile(string file, Log log)
        {
            try
            {
                this.log = log;

                if (File.Exists(file))
                {
                    ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
                    this.Project = new Project(file);
                }
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, ex.Message);
            }
        }

        public string Configuration { get; set; }

        public string PlatformTarget { get; set; }

        public string ReferencesPath { get; set; }

        public void Change()
        {
            try
            {
                if (this.Project == null)
                {
                    return;
                }

                this.Project.SetGlobalProperty("Configuration", this.Configuration);
                this.Project.SetGlobalProperty("Platform", this.PlatformTarget);
                this.Project.SetGlobalProperty("PlatformTarget", this.PlatformTarget);

                this.Project.SetGlobalProperty("PreBuildEvent", string.Empty);
                this.Project.SetGlobalProperty("PostBuildEvent", string.Empty);
                this.Project.SetGlobalProperty("PreLinkEvent", string.Empty);

                this.Project.SetGlobalProperty("DebugSymbols", this.Configuration == "Release" ? "false" : "true");
                this.Project.SetGlobalProperty("DebugType", this.Configuration == "Release" ? "None" : "full");
                this.Project.SetGlobalProperty("Optimize", this.Configuration == "Release" ? "true" : "false");
                this.Project.SetGlobalProperty("DefineConstants", this.Configuration == "Release" ? "TRACE" : "DEBUG;TRACE");

                this.Project.SetGlobalProperty("OutputPath", "bin\\" + this.Configuration + "\\");

                foreach (var item in this.Project.GetItems("Reference"))
                {
                    var hintPath = item?.GetMetadata("HintPath");

                    if (!string.IsNullOrWhiteSpace(hintPath?.EvaluatedValue))
                    {
                        item.SetMetadataValue("HintPath", Path.Combine(this.ReferencesPath, Path.GetFileName(hintPath.EvaluatedValue)));
                    }
                }

                this.Project.Save();
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, ex.Message);
            }
        }
    }
}