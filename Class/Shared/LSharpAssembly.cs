// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LSharpAssembly.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Sandbox.Shared
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LSharpAssembly
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string PathToBinary { get; set; }
    }
}