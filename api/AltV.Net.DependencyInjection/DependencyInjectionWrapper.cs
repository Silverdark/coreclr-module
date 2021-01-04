using AltV.Net.DependencyInjection.Contracts;
using AltV.Net.Elements.Entities;
using AltV.Net.Elements.Factories;
using AltV.Net.Elements.Pools;
using AltV.Net.ResourceLoaders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.Loader;

namespace AltV.Net.DependencyInjection
{
    // ReSharper disable once UnusedType.Global
    // TODO: Rename to match code
    internal class DependencyInjectionWrapper
    {
        // ReSharper disable once UnusedMember.Global
        // This is called by `AltV.Net.Host` when dependency injection is enabled.
        public static void Main(IntPtr serverPointer, IntPtr resourcePointer, AssemblyLoadContext assemblyLoadContext)
        {
            // Build service provider
            if (!AssemblyLoader.FindType(assemblyLoadContext.Assemblies, out IStartup startup))
            {
                return;
            }

            var serviceCollection = new ServiceCollection();

            // TODO: Move into `AltDep.UsePools` or so... But with this it's possible to have missing dependencies.
            // TODO: Add `UseAltVAsync` or so...
            serviceCollection.AddSingleton<WrapperContext>(new WrapperContext
            {
                ServerPointer = serverPointer,
                ResourcePointer = resourcePointer,
                AssemblyLoadContext = assemblyLoadContext
            });

            serviceCollection.AddSingleton<INativeResource>(provider =>
            {
                var nativeResourcePool = provider.GetRequiredService<INativeResourcePool>();
                var wrapperContext = provider.GetRequiredService<WrapperContext>();
                nativeResourcePool.GetOrCreate(wrapperContext.ServerPointer, wrapperContext.ResourcePointer,
                    out var resource);

                return resource;
            });
            
            serviceCollection.AddSingleton<IResource, EmptyResource>();
            serviceCollection.AddSingleton<IServer, Server>();
            serviceCollection.AddSingleton<IServerModule, Module>();
            
            serviceCollection.AddSingleton<IBaseBaseObjectPool, BaseBaseObjectPool>();
            serviceCollection.AddSingleton<IEntityPool<IPlayer>, PlayerPool>();
            serviceCollection.AddSingleton<IEntityFactory<IPlayer>, PlayerFactory>();
            serviceCollection.AddSingleton<IEntityPool<IVehicle>, VehiclePool>();
            serviceCollection.AddSingleton<IEntityFactory<IVehicle>, VehicleFactory>();
            serviceCollection.AddSingleton<IBaseObjectPool<IBlip>, BlipPool>();
            serviceCollection.AddSingleton<IBaseObjectFactory<IBlip>, BlipFactory>();
            serviceCollection.AddSingleton<IBaseObjectPool<ICheckpoint>, CheckpointPool>();
            serviceCollection.AddSingleton<IBaseObjectFactory<ICheckpoint>, CheckpointFactory>();
            serviceCollection.AddSingleton<IBaseObjectPool<IVoiceChannel>, VoiceChannelPool>();
            serviceCollection.AddSingleton<IBaseObjectFactory<IVoiceChannel>, VoiceChannelFactory>();
            serviceCollection.AddSingleton<IBaseObjectPool<IColShape>, ColShapePool>();
            serviceCollection.AddSingleton<IBaseObjectFactory<IColShape>, ColShapeFactory>();
            
            serviceCollection.AddSingleton<IBaseEntityPool, BaseEntityPool>();
            serviceCollection.AddSingleton<IBaseEntityPool, BaseEntityPool>();
            
            serviceCollection.AddSingleton<INativeResourcePool, NativeResourcePool>();
            serviceCollection.AddSingleton<INativeResourceFactory, NativeResourceFactory>();

            startup.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Continue setup

            // while (!Debugger.IsAttached)
            //     Thread.Sleep(1000);

            var resource = serviceProvider.GetRequiredService<IResource>();
            var server = serviceProvider.GetRequiredService<IServer>();
            var module = serviceProvider.GetRequiredService<IServerModule>();
            var nativeResource = serviceProvider.GetRequiredService<INativeResource>();

            ModuleWrapper.Initialize(resource, module, server, nativeResource, assemblyLoadContext);
            
            // TODO: Under construction
            var scriptServices = AssemblyLoader.FindAllTypes<IScriptService>(assemblyLoadContext.Assemblies);
            foreach (var scriptService in scriptServices)
            {
                scriptService.Initialize();
            }
        }
        
        private class EmptyResource : Resource
        {
            public override void OnStart()
            {
            }

            public override void OnStop()
            {
            }
        }

        
    }
}