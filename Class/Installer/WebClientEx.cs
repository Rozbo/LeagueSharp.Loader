// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebClientEx.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class.Installer
{
    using System;
    using System.Net;
    using System.Net.Cache;

    public class WebClientEx : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var request = base.GetWebRequest(uri) as HttpWebRequest;
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Timeout = 10000;

            return request;
        }
    }
}