// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginCredentials.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Sandbox.Shared
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LoginCredentials
    {
        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string User { get; set; }
    }
}