// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Auth.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.Threading.Tasks;

    internal static class Auth
    {
        public static string Hash(string input)
        {
            return Utility.Md5Hash(IPB_Clean_Password(input));
        }

        public static async Task<Tuple<bool, string>> Login(string user, string hash)
        {
            if (user == null || hash == null)
            {
                return new Tuple<bool, string>(false, Utility.GetMultiLanguageText("AuthEmpty"));
            }

            try
            {
                if (await WebService.Client.LoginAsync(user, hash))
                {
                    return new Tuple<bool, string>(true, "Success");
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }

            return new Tuple<bool, string>(false, string.Format(Utility.GetMultiLanguageText("WrongAuth"), "www.joduska.me"));
        }

        private static string IPB_Clean_Password(string pass)
        {
            pass = pass.Replace("\xC3\x8A", string.Empty);
            pass = pass.Replace("&", "&amp;");
            pass = pass.Replace("\\", "&#092;");
            pass = pass.Replace("!", "&#33;");
            pass = pass.Replace("$", "&#036;");
            pass = pass.Replace("\"", "&quot;");
            pass = pass.Replace("\"", "&quot;");
            pass = pass.Replace("<", "&lt;");
            pass = pass.Replace(">", "&gt;");
            pass = pass.Replace("'", "&#39;");

            return pass;
        }
    }
}