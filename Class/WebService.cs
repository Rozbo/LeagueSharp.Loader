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

        public static WebServiceClient Client { get; }

        public static IReadOnlyList<AssemblyEntry> Assemblies
        {
            get
            {
                try
                {
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

        static WebService()
        {
            Client = new WebServiceClient();
        }
    }
}