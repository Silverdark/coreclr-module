using AltV.Net.Data;
using AltV.Net.Elements.Args;
using AltV.Net.Elements.Entities;
using AltV.Net.FunctionParser;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace AltV.Net
{
    public interface IServerModule : IDisposable
    {
        bool IsMainThread();
        Assembly LoadAssemblyFromName(AssemblyName assemblyName);
        Assembly LoadAssemblyFromStream(Stream stream);
        Assembly LoadAssemblyFromStream(Stream stream, Stream assemblySymbols);
        Assembly LoadAssemblyFromPath(string path);
        Assembly LoadAssemblyFromNativeImagePath(string nativeImagePath, string assemblyPath);
        WeakReference<AssemblyLoadContext> GetAssemblyLoadContext();
        void OnClient(string eventName, Function function);
        void OffClient(string eventName, Function function);
        void OnServer(string eventName, Function function);
        void OffServer(string eventName, Function function);
        void On<TFunc>(string eventName, TFunc func, ClientEventParser<TFunc> parser) where TFunc : Delegate;
        void Off<TFunc>(string eventName, TFunc func, ClientEventParser<TFunc> parser) where TFunc : Delegate;
        void On<TFunc>(string eventName, TFunc func, ServerEventParser<TFunc> parser) where TFunc : Delegate;
        void Off<TFunc>(string eventName, TFunc func, ServerEventParser<TFunc> parser) where TFunc : Delegate;

        void OnCheckpoint(IntPtr checkpointPointer, IntPtr entityPointer, BaseObjectType baseObjectType,
                          bool state);

        void OnCheckPointEvent(ICheckpoint checkpoint, IEntity entity, bool state);
        void OnPlayerConnect(IntPtr playerPointer, ushort playerId, string reason);
        void OnResourceStart(IntPtr resourcePointer);
        void OnResourceStartEvent(INativeResource resource);
        void OnResourceStop(IntPtr resourcePointer);
        void OnResourceStopEvent(INativeResource resource);
        void OnResourceError(IntPtr resourcePointer);
        void OnResourceErrorEvent(INativeResource resource);
        void OnPlayerConnectEvent(IPlayer player, string reason);

        void OnPlayerDamage(IntPtr playerPointer, IntPtr attackerEntityPointer,
                            BaseObjectType attackerBaseObjectType,
                            ushort attackerEntityId, uint weapon, ushort damage);

        void OnPlayerDamageEvent(IPlayer player, IEntity attacker, uint weapon, ushort damage);

        void OnPlayerDeath(IntPtr playerPointer, IntPtr killerEntityPointer, BaseObjectType killerBaseObjectType,
                           uint weapon);

        void OnPlayerDeathEvent(IPlayer player, IEntity killer, uint weapon);

        void OnExplosion(IntPtr eventPointer, IntPtr playerPointer, ExplosionType explosionType,
                         Position position, uint explosionFx, IntPtr targetEntityPointer, BaseObjectType targetEntityType);

        void OnExplosionEvent(IntPtr eventPointer, IPlayer sourcePlayer, ExplosionType explosionType,
                              Position position,
                              uint explosionFx, IEntity targetEntity);

        void OnWeaponDamage(IntPtr eventPointer, IntPtr playerPointer, IntPtr entityPointer,
                            BaseObjectType entityType, uint weapon,
                            ushort damage, Position shotOffset, BodyPart bodyPart);

        void OnWeaponDamageEvent(IntPtr eventPointer, IPlayer sourcePlayer, IEntity targetEntity,
                                 uint weapon, ushort damage,
                                 Position shotOffset, BodyPart bodyPart);

        void OnPlayerChangeVehicleSeat(IntPtr vehiclePointer, IntPtr playerPointer, byte oldSeat,
                                       byte newSeat);

        void OnPlayerChangeVehicleSeatEvent(IVehicle vehicle, IPlayer player, byte oldSeat, byte newSeat);
        void OnPlayerEnterVehicle(IntPtr vehiclePointer, IntPtr playerPointer, byte seat);
        void OnPlayerEnterVehicleEvent(IVehicle vehicle, IPlayer player, byte seat);
        void OnPlayerEnteringVehicle(IntPtr vehiclePointer, IntPtr playerPointer, byte seat);
        void OnPlayerEnteringVehicleEvent(IVehicle vehicle, IPlayer player, byte seat);
        void OnPlayerLeaveVehicle(IntPtr vehiclePointer, IntPtr playerPointer, byte seat);
        void OnPlayerLeaveVehicleEvent(IVehicle vehicle, IPlayer player, byte seat);
        void OnPlayerDisconnect(IntPtr playerPointer, string reason);
        void OnPlayerDisconnectEvent(IPlayer player, string reason);
        void OnPlayerRemove(IntPtr playerPointer);
        void OnPlayerRemoveEvent(IPlayer player);
        void OnVehicleRemove(IntPtr vehiclePointer);
        void OnVehicleRemoveEvent(IVehicle vehicle);
        void OnClientEvent(IntPtr playerPointer, string name, IntPtr[] args);

        void OnClientEventEvent(IPlayer player, string name, IntPtr[] args, MValueConst[] mValues,
                                object[] objects);

        void OnServerEvent(string name, IntPtr[] args);
        void OnServerEventEvent(string name, IntPtr[] args, MValueConst[] mValues, object[] objects);
        void OnCreatePlayer(IntPtr playerPointer, ushort playerId);
        void OnRemovePlayer(IntPtr playerPointer);
        void OnCreateVehicle(IntPtr vehiclePointer, ushort vehicleId);
        void OnCreateVoiceChannel(IntPtr channelPointer);
        void OnCreateColShape(IntPtr colShapePointer);
        void OnRemoveVehicle(IntPtr vehiclePointer);
        void OnCreateBlip(IntPtr blipPointer);
        void OnRemoveBlip(IntPtr blipPointer);
        void OnCreateCheckpoint(IntPtr checkpointPointer);
        void OnRemoveCheckpoint(IntPtr checkpointPointer);
        void OnRemoveVoiceChannel(IntPtr channelPointer);
        void OnRemoveColShape(IntPtr colShapePointer);
        void OnConsoleCommand(string name, string[] args);
        void OnConsoleCommandEvent(string name, string[] args);

        void OnMetaDataChange(IntPtr entityPointer, BaseObjectType entityType, string key,
                              IntPtr value);

        void OnMetaDataChangeEvent(IEntity entity, string key, object value);

        void OnSyncedMetaDataChange(IntPtr entityPointer, BaseObjectType entityType, string key,
                                    IntPtr value);

        void OnSyncedMetaDataChangeEvent(IEntity entity, string key, object value);

        void OnColShape(IntPtr colShapePointer, IntPtr targetEntityPointer, BaseObjectType entityType,
                        bool state);

        void OnColShapeEvent(IColShape colShape, IEntity entity, bool state);
        void OnVehicleDestroy(IntPtr vehiclePointer);
        void OnVehicleDestroyEvent(IVehicle vehicle);
        void OnFire(IntPtr eventPointer, IntPtr playerPointer, FireInfo[] fires);
        void OnFireEvent(IntPtr eventPointer, IPlayer player, FireInfo[] fires);
        void OnStartProjectile(IntPtr eventPointer, IntPtr sourcePlayerPointer, Position startPosition, Position direction, uint ammoHash, uint weaponHash);
        void OnStartProjectileEvent(IntPtr eventPointer, IPlayer player, Position startPosition, Position direction, uint ammoHash, uint weaponHash);
        void OnPlayerWeaponChange(IntPtr eventPointer, IntPtr targetPlayerPointer, uint oldWeapon, uint newWeapon);
        void OnPlayerWeaponChangeEvent(IntPtr eventPointer, IPlayer player, uint oldWeapon, uint newWeapon);
        void OnNetOwnerChange(IntPtr eventPointer, IntPtr targetEntityPointer, BaseObjectType targetEntityType, IntPtr oldNetOwnerPointer, IntPtr newNetOwnerPointer);
        void OnNetOwnerChangeEvent(IEntity targetEntity, IPlayer oldPlayer, IPlayer newPlayer);
        void OnVehicleAttach(IntPtr eventPointer, IntPtr targetPointer, IntPtr attachedPointer);
        void OnVehicleAttachEvent(IVehicle targetVehicle, IVehicle attachedVehicle);
        void OnVehicleDetach(IntPtr eventPointer, IntPtr targetPointer, IntPtr detachedPointer);
        void OnVehicleDetachEvent(IVehicle targetVehicle, IVehicle detachedVehicle);
        void OnScriptsLoaded(IScript[] scripts);
        void OnScriptLoaded(IScript script);
        void OnModulesLoaded(IModule[] modules);
        void OnModuleLoaded(IModule module);
        void SetExport(string key, Function function);
    }
}