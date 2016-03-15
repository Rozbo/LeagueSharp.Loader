// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILoaderService.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Sandbox.Shared
{
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract]
    public interface ILoaderService
    {
        [OperationContract]
        List<LSharpAssembly> GetAssemblyList(int pid);

        [OperationContract]
        Configuration GetConfiguration(int pid);

        [OperationContract]
        void Recompile(int pid);
    }
}