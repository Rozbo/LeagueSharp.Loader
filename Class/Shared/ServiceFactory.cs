// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceFactory.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Sandbox.Shared
{
    using System;
    using System.ServiceModel;
    using System.Windows;

    public static class ServiceFactory
    {
        public static TInterfaceType CreateProxy<TInterfaceType>() where TInterfaceType : class
        {
            try
            {
                return
                    new ChannelFactory<TInterfaceType>(
                        new NetNamedPipeBinding(), 
                        new EndpointAddress("net.pipe://localhost/" + typeof(TInterfaceType).Name)).CreateChannel();
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Failed to connect to pipe for communication. The targetted pipe may not be loaded yet. Desired interface: "
                    + typeof(TInterfaceType).Name, 
                    e);
            }
        }

        public static ServiceHost CreateService<TInterfaceType, TImplementationType>(bool open = true)
            where TImplementationType : class
        {
            try
            {
                if (!typeof(TInterfaceType).IsAssignableFrom(typeof(TImplementationType)))
                {
                    throw new NotImplementedException(
                        typeof(TImplementationType).FullName + " does not implement " + typeof(TInterfaceType).FullName);
                }

                var endpoint = new Uri("net.pipe://localhost/" + typeof(TInterfaceType).Name);
                var host = new ServiceHost(typeof(TImplementationType));

                host.AddServiceEndpoint(typeof(TInterfaceType), new NetNamedPipeBinding(), endpoint);
                host.Opened += (sender, args) => { Console.WriteLine("Opened: " + endpoint); };
                host.Faulted += (sender, args) => { Console.WriteLine("Faulted: " + endpoint); };
                host.UnknownMessageReceived += (sender, args) => { Console.WriteLine("UnknownMessage: " + endpoint); };

                if (open)
                {
                    host.Open();
                }

                return host;
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"Make sure only one LeagueSharp Instance is running on your Computer.\n\n{e.Message}", 
                    "Failed to initialize Remoting");
                Environment.Exit(0);
            }

            return null;
        }
    }
}