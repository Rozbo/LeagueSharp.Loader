// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LSUriScheme.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    #region

    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using LeagueSharp.Loader.Views;

    using MahApps.Metro.Controls;

    #endregion

    public static class LSUriScheme
    {
        public const string Name = "ls";

        public static string FullName
        {
            get
            {
                return Name + "://";
            }
        }

        public static async Task HandleUrl(string url, MetroWindow window)
        {
            url = url.Remove(0, FullName.Length).WebDecode();

            var r = Regex.Matches(url, "(project|projectGroup)/([^/]*)/([^/]*)/([^/]*)/?");
            foreach (Match m in r)
            {
                var linkType = m.Groups[1].ToString();

                switch (linkType)
                {
                    case "project":
                        InstallerWindow.InstallAssembly(m);
                        break;
                }
            }
        }
    }
}