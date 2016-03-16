// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebService.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.Collections.Generic;

    using PlaySharp.Service.WebService;
    using PlaySharp.Service.WebService.Model;

    /// <summary>
    /// The web service.
    /// </summary>
    internal static class WebService
    {
        private static IReadOnlyList<AssemblyEntry> assemblies = new List<AssemblyEntry>();

        static WebService()
        {
            Client = new WebServiceClient();
        }

        public static IReadOnlyList<AssemblyEntry> Assemblies
        {
            get
            {
                try
                {
                    if (!Client.IsAuthenticated)
                    {
                        return new AssemblyEntry[0];
                    }

                    if (assemblies.Count == 0)
                    {
                        assemblies = Client.Assemblies();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return assemblies;
            }
        }

        public static WebServiceClient Client { get; }
    }
}