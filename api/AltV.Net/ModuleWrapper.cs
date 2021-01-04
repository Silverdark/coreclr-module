using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.ResourceLoaders;

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: InternalsVisibleTo("AltV.Net.Mock")]
[assembly: InternalsVisibleTo("AltV.Net.DependencyInjection")]

namespace AltV.Net
{
    internal static class ModuleWrapper
    {
        private static IServerModule _module;

        private static IResource _resource;

        private static IScript[] _scripts;

        private static IModule[] _modules;

        public static void OnStartResource(IntPtr serverPointer, IntPtr resourcePointer, string resourceName,
            string entryPoint)
        {
            foreach (var module in _modules)
            {
                module.OnScriptsStarted(_scripts);
            }

            _resource.OnStart();
        }

        public static void MainWithAssembly(IntPtr serverPointer, IntPtr resourcePointer,
            AssemblyLoadContext assemblyLoadContext)
        {
            if (!AssemblyLoader.FindType(assemblyLoadContext.Assemblies, out IResource resource))
            {
                return;
            }

            var wrapperContext = new WrapperContext
            {
                ResourcePointer = resourcePointer,
                ServerPointer = serverPointer,
                AssemblyLoadContext = assemblyLoadContext
            };
            
            var playerFactory = resource.GetPlayerFactory();
            var vehicleFactory = resource.GetVehicleFactory();
            var blipFactory = resource.GetBlipFactory();
            var checkpointFactory = resource.GetCheckpointFactory();
            var voiceChannelFactory = resource.GetVoiceChannelFactory();
            var colShapeFactory = resource.GetColShapeFactory();
            var nativeResourceFactory = resource.GetNativeResourceFactory();
            var playerPool = resource.GetPlayerPool(playerFactory);
            var vehiclePool = resource.GetVehiclePool(vehicleFactory);
            var blipPool = resource.GetBlipPool(blipFactory);
            var checkpointPool = resource.GetCheckpointPool(checkpointFactory);
            var voiceChannelPool = resource.GetVoiceChannelPool(voiceChannelFactory);
            var colShapePool = resource.GetColShapePool(colShapeFactory);
            var nativeResourcePool = resource.GetNativeResourcePool(nativeResourceFactory);
            var entityPool = resource.GetBaseEntityPool(playerPool, vehiclePool);
            var baseObjectPool =
                resource.GetBaseBaseObjectPool(playerPool, vehiclePool, blipPool, checkpointPool, voiceChannelPool,
                    colShapePool);
            nativeResourcePool.GetOrCreate(serverPointer, resourcePointer, out var csharpResource);
            var server = new Server(wrapperContext, csharpResource, baseObjectPool, entityPool, playerPool, vehiclePool,
                blipPool,
                checkpointPool, voiceChannelPool, colShapePool, nativeResourcePool);
            
            var module = resource.GetModule(server, wrapperContext, csharpResource, baseObjectPool, entityPool,
                playerPool, vehiclePool,
                blipPool, checkpointPool, voiceChannelPool, colShapePool, nativeResourcePool);
            
            Initialize(resource, module, server, csharpResource, assemblyLoadContext);
        }

        // TODO: Refactor
        internal static void Initialize(IResource resource,
                                        IServerModule module,
                                        IServer server,
                                        INativeResource csharpResource,
                                        AssemblyLoadContext assemblyLoadContext)
        {
            _resource = resource;
            _module = module;
            _scripts = AssemblyLoader.FindAllTypes<IScript>(assemblyLoadContext.Assemblies);
            
            foreach (var unused in server.GetPlayers())
            {
            }

            foreach (var unused in server.GetVehicles())
            {
            }

            csharpResource.CSharpResourceImpl.SetDelegates();
            
            foreach (var script in _scripts)
            {
                if (script.GetType().GetInterfaces().Contains(typeof(IResource)))
                {
                    throw new InvalidOperationException(
                        "IScript must not be a IResource for type:" + script.GetType());
                }
            }
            
            _module.OnScriptsLoaded(_scripts);
            // TODO: Provide from outside
            _modules = AssemblyLoader.FindAllTypes<IModule>(assemblyLoadContext.Assemblies);
            _module.OnModulesLoaded(_modules);

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Alt.Log(
                $"< ==== UNHANDLED EXCEPTION ==== > {Environment.NewLine} Received an unhandled exception from {sender?.GetType()}: " +
                (Exception) e.ExceptionObject);
        }

        public static void OnStop()
        {
            _resource.OnStop();
            foreach (var module in _modules)
            {
                module.OnStop();
            }

            Alt.Server.Resource.CSharpResourceImpl.Dispose();

            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

            _module.Dispose();

            _modules = new IModule[0];
            _scripts = new IScript[0];
            _module = null;
            _resource = null;
        }

        public static void OnTick()
        {
            _resource.OnTick();
        }

        public static void OnCheckpoint(IntPtr checkpointPointer, IntPtr entityPointer, BaseObjectType baseObjectType,
            bool state)
        {
            _module.OnCheckpoint(checkpointPointer, entityPointer, baseObjectType, state);
        }

        public static void OnPlayerConnect(IntPtr playerPointer, ushort playerId, string reason)
        {
            _module.OnPlayerConnect(playerPointer, playerId, reason);
        }

        public static void OnResourceStart(IntPtr resourcePointer)
        {
            _module.OnResourceStart(resourcePointer);
        }

        public static void OnResourceStop(IntPtr resourcePointer)
        {
            _module.OnResourceStop(resourcePointer);
        }

        public static void OnResourceError(IntPtr resourcePointer)
        {
            _module.OnResourceError(resourcePointer);
        }

        public static void OnPlayerDamage(IntPtr playerPointer, IntPtr attackerEntityPointer,
            BaseObjectType attackerBaseObjectType,
            ushort attackerEntityId, uint weapon, ushort damage)
        {
            _module.OnPlayerDamage(playerPointer, attackerEntityPointer, attackerBaseObjectType, attackerEntityId,
                weapon, damage);
        }

        public static void OnPlayerDeath(IntPtr playerPointer, IntPtr killerEntityPointer,
            BaseObjectType killerBaseObjectType, uint weapon)
        {
            _module.OnPlayerDeath(playerPointer, killerEntityPointer, killerBaseObjectType, weapon);
        }

        public static void OnExplosion(IntPtr eventPointer, IntPtr playerPointer, ExplosionType explosionType,
            Position position, uint explosionFx, IntPtr targetEntityPointer, BaseObjectType targetEntityType)
        {
            _module.OnExplosion(eventPointer, playerPointer, explosionType, position, explosionFx, targetEntityPointer, targetEntityType);
        }

        public static void OnWeaponDamage(IntPtr eventPointer, IntPtr playerPointer, IntPtr entityPointer,
            BaseObjectType entityType, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart)
        {
            _module.OnWeaponDamage(eventPointer, playerPointer, entityPointer, entityType, weapon, damage, shotOffset, bodyPart);
        }

        public static void OnPlayerChangeVehicleSeat(IntPtr vehiclePointer, IntPtr playerPointer, byte oldSeat,
            byte newSeat)
        {
            _module.OnPlayerChangeVehicleSeat(vehiclePointer, playerPointer, oldSeat, newSeat);
        }

        public static void OnPlayerEnterVehicle(IntPtr vehiclePointer, IntPtr playerPointer, byte seat)
        {
            _module.OnPlayerEnterVehicle(vehiclePointer, playerPointer, seat);
        }

        public static void OnPlayerEnteringVehicle(IntPtr vehiclePointer, IntPtr playerPointer, byte seat)
        {
            _module.OnPlayerEnteringVehicle(vehiclePointer, playerPointer, seat);
        }

        public static void OnPlayerLeaveVehicle(IntPtr vehiclePointer, IntPtr playerPointer, byte seat)
        {
            _module.OnPlayerLeaveVehicle(vehiclePointer, playerPointer, seat);
        }

        public static void OnPlayerDisconnect(IntPtr playerPointer, string reason)
        {
            _module.OnPlayerDisconnect(playerPointer, reason);
        }

        public static void OnClientEvent(IntPtr playerPointer, string name, IntPtr pointer, ulong size)
        {
            var args = new IntPtr[size];
            if (pointer != IntPtr.Zero)
            {
                Marshal.Copy(pointer, args, 0, (int) size);
            }

            _module.OnClientEvent(playerPointer, name, args);
        }

        public static void OnServerEvent(string name, IntPtr pointer, ulong size)
        {
            var args = new IntPtr[size];
            if (pointer != IntPtr.Zero)
            {
                Marshal.Copy(pointer, args, 0, (int) size);
            }

            _module.OnServerEvent(name, args);
        }

        public static void OnCreatePlayer(IntPtr playerPointer, ushort playerId)
        {
            _module.OnCreatePlayer(playerPointer, playerId);
        }

        public static void OnRemovePlayer(IntPtr playerPointer)
        {
            _module.OnRemovePlayer(playerPointer);
        }

        public static void OnCreateVehicle(IntPtr vehiclePointer, ushort vehicleId)
        {
            _module.OnCreateVehicle(vehiclePointer, vehicleId);
        }

        public static void OnRemoveVehicle(IntPtr vehiclePointer)
        {
            _module.OnRemoveVehicle(vehiclePointer);
        }

        public static void OnCreateBlip(IntPtr blipPointer)
        {
            _module.OnCreateBlip(blipPointer);
        }

        public static void OnCreateVoiceChannel(IntPtr channelPointer)
        {
            _module.OnCreateVoiceChannel(channelPointer);
        }

        public static void OnCreateColShape(IntPtr colShapePointer)
        {
            _module.OnCreateColShape(colShapePointer);
        }

        public static void OnRemoveBlip(IntPtr blipPointer)
        {
            _module.OnRemoveBlip(blipPointer);
        }

        public static void OnCreateCheckpoint(IntPtr checkpointPointer)
        {
            _module.OnCreateCheckpoint(checkpointPointer);
        }

        public static void OnRemoveCheckpoint(IntPtr checkpointPointer)
        {
            _module.OnRemoveCheckpoint(checkpointPointer);
        }

        public static void OnRemoveVoiceChannel(IntPtr channelPointer)
        {
            _module.OnRemoveVoiceChannel(channelPointer);
        }

        public static void OnRemoveColShape(IntPtr colShapePointer)
        {
            _module.OnRemoveColShape(colShapePointer);
        }

        public static void OnPlayerRemove(IntPtr playerPointer)
        {
            _module.OnPlayerRemove(playerPointer);
        }

        public static void OnVehicleRemove(IntPtr vehiclePointer)
        {
            _module.OnVehicleRemove(vehiclePointer);
        }

        public static void OnConsoleCommand(string name,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            string[] args, int argsSize)
        {
            args ??= new string[0];
            _module.OnConsoleCommand(name, args);
        }

        public static void OnMetaDataChange(IntPtr entityPointer, BaseObjectType entityType, string key,
            IntPtr value)
        {
            _module.OnMetaDataChange(entityPointer, entityType, key, value);
        }

        public static void OnSyncedMetaDataChange(IntPtr entityPointer, BaseObjectType entityType, string key,
            IntPtr value)
        {
            _module.OnSyncedMetaDataChange(entityPointer, entityType, key, value);
        }

        public static void OnColShape(IntPtr colShapePointer, IntPtr targetEntityPointer, BaseObjectType entityType,
            bool state)
        {
            _module.OnColShape(colShapePointer, targetEntityPointer, entityType, state);
        }
        
        public static void OnVehicleDestroy(IntPtr vehiclePointer)
        {
            _module.OnVehicleDestroy(vehiclePointer);
        }
        
        public static void OnFire(IntPtr eventPointer, IntPtr playerPointer,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            FireInfo[] fires, int length)
        {
            fires ??= new FireInfo[0];
            _module.OnFire(eventPointer, playerPointer, fires);
        }
        
        public static void OnStartProjectile(IntPtr eventPointer, IntPtr sourcePlayerPointer, Position startPosition, Position direction, uint ammoHash, uint weaponHash)
        {
            _module.OnStartProjectile(eventPointer, sourcePlayerPointer, startPosition, direction, ammoHash, weaponHash);
        }
        
        public static void OnPlayerWeaponChange(IntPtr eventPointer, IntPtr targetPlayerPointer, uint oldWeapon, uint newWeapon)
        {
            _module.OnPlayerWeaponChange(eventPointer, targetPlayerPointer, oldWeapon, newWeapon);
        }

        public static void OnNetOwnerChange(IntPtr eventPointer, IntPtr targetEntityPointer, BaseObjectType targetEntityType, IntPtr oldNetOwnerPointer, IntPtr newNetOwnerPointer)
        {
            _module.OnNetOwnerChange(eventPointer, targetEntityPointer, targetEntityType, oldNetOwnerPointer, newNetOwnerPointer);
        }

        public static void OnVehicleAttach(IntPtr eventPointer, IntPtr targetPointer, IntPtr attachedPointer)
        {
            _module.OnVehicleAttach(eventPointer, targetPointer, attachedPointer);
        }

        public static void OnVehicleDetach(IntPtr eventPointer, IntPtr targetPointer, IntPtr detachedPointer)
        {
            _module.OnVehicleDetach(eventPointer, targetPointer, detachedPointer);
        }
    }
}
