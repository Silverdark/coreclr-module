using AltV.Net.Async.Elements.Pools;
using AltV.Net.Elements.Entities;

namespace AltV.Net.Async
{
    public abstract class AsyncResource : Resource
    {
        private readonly AltVAsync altVAsync;

        public AsyncResource() : this(new DefaultTickSchedulerFactory())
        {
        }

        public AsyncResource(ITickSchedulerFactory tickSchedulerFactory)
        {
            altVAsync = new AltVAsync(tickSchedulerFactory);
        }

        public override void OnTick()
        {
            altVAsync.Tick();
        }

        public override IBaseEntityPool GetBaseEntityPool(IEntityPool<IPlayer> playerPool,
            IEntityPool<IVehicle> vehiclePool)
        {
            return new AsyncBaseBaseObjectPool(playerPool, vehiclePool);
        }

        public override IEntityPool<IPlayer> GetPlayerPool(IEntityFactory<IPlayer> playerFactory)
        {
            return new AsyncPlayerPool(playerFactory);
        }

        public override IEntityPool<IVehicle> GetVehiclePool(IEntityFactory<IVehicle> vehicleFactory)
        {
            return new AsyncVehiclePool(vehicleFactory);
        }

        public override Module GetModule(IServer server, CSharpNativeResource cSharpNativeResource,
            IBaseBaseObjectPool baseBaseObjectPool,
            IBaseEntityPool baseEntityPool,
            IEntityPool<IPlayer> playerPool,
            IEntityPool<IVehicle> vehiclePool,
            IBaseObjectPool<IBlip> blipPool,
            IBaseObjectPool<ICheckpoint> checkpointPool,
            IBaseObjectPool<IVoiceChannel> voiceChannelPool)
        {
            return new AsyncModule(server, cSharpNativeResource, baseBaseObjectPool, baseEntityPool, playerPool,
                vehiclePool, blipPool, checkpointPool, voiceChannelPool);
        }
    }
}