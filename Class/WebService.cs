// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebService.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading.Tasks;

    using LeagueSharp.Loader.Data;

    using PlaySharp.Service.JSend.Client;
    using PlaySharp.Service.WebService;
    using PlaySharp.Service.WebService.Endpoints;
    using PlaySharp.Service.WebService.Model;

    using Polly;
    using Polly.Retry;

    /// <summary>
    /// The web service.
    /// </summary>
    internal static class WebService
    {
        private static IReadOnlyList<AssemblyEntry> assemblies = new List<AssemblyEntry>();

        private static ObservableCollection<AssemblyEntry> databaseAssemblies;

        static WebService()
        {
            Client = new WebServiceClient();

            RetryPolicy = Policy
                .Handle<WebException>()
                .Or<AggregateException>()
                .Or<JSendParseException>()
                .Or<JSendRequestException>()
                .WaitAndRetry(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                    (exception, timeSpan, context) => { Utility.Log(LogStatus.Error, $"ReTry {exception.Message}"); });

            RetryPolicyAsync = Policy
                .Handle<WebException>()
                .Or<AggregateException>()
                .Or<JSendParseException>()
                .Or<JSendRequestException>()
                .WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                    (exception, timeSpan, context) => { Utility.Log(LogStatus.Error, $"ReTry {exception.Message}"); });
        }

        public static IReadOnlyList<AssemblyEntry> Assemblies
        {
            get
            {
                if (!Client.IsAuthenticated)
                {
                    return new AssemblyEntry[0];
                }

                if (assemblies.Count == 0)
                {
                    RetryPolicy.Execute(() => { assemblies = Client.Assemblies(); });
                }

                return assemblies;
            }
        }

        public static bool IsAuthenticated => Client.IsAuthenticated;

        private static WebServiceClient Client { get; }

        private static RetryPolicy RetryPolicy { get; set; }

        private static RetryPolicy RetryPolicyAsync { get; set; }

        public static async Task<PolicyResult<CoreEntry>> RequestCore(string md5)
        {
            return await RetryPolicyAsync.ExecuteAndCaptureAsync(() => Client.CoreAsync(md5), true);
        }

        public static async Task<PolicyResult<IReadOnlyList<RepositoryEntry>>> RequestRepositories()
        {
            return await RetryPolicyAsync.ExecuteAndCaptureAsync(() => Client.RepositoriesAsync(), true);
        }

        public static Task<bool> Login(string user, string hash)
        {
            return Client.LoginAsync(user, hash);
        }

        public static void CloudStore(object instance, string key)
        {
            Client.CloudStore(instance, key);
        }

        public static PolicyResult<string> RequestCloud(string key)
        {
            return RetryPolicy.ExecuteAndCapture(() => Client.Cloud(key));
        }

        public static async Task<PolicyResult<LoaderVersionData>> RequestLoaderVersionAsync()
        {
            return await RetryPolicyAsync.ExecuteAndCaptureAsync(() => Client.LoaderVersionAsync(), true);
        }

        public static async Task<PolicyResult<Account>> RequestAccountAsync()
        {
            return await RetryPolicyAsync.ExecuteAndCaptureAsync(() => Client.AccountAsync(), true);
        }
    }
}