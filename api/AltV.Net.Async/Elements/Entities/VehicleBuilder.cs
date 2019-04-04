using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Native;

namespace AltV.Net.Async.Elements.Entities
{
    public class VehicleBuilder : IVehicleBuilder
    {
        private readonly uint model;

        private readonly Position position;

        private readonly float heading;

        private byte? primaryColor;

        private IntPtr numberPlate = IntPtr.Zero;

        public VehicleBuilder(uint model, Position position, float heading)
        {
            this.model = model;
            this.position = position;
            this.heading = heading;
        }

        public IVehicleBuilder PrimaryColor(byte value)
        {
            primaryColor = value;
            return this;
        }

        public IVehicleBuilder NumberPlate(string value)
        {
            numberPlate = AltNative.StringUtils.StringToHGlobalUtf8(value);
            return this;
        }

        public async Task<IVehicle> Build()
        {
            ushort id = default;
            var vehiclePtr = await AltAsync.AltVAsync.Schedule(() =>
            {
                var ptr = AltNative.Server.Server_CreateVehicle(((Server) Alt.Server).NativePointer, model,
                    position, heading,
                    ref id);
                if (primaryColor.HasValue)
                {
                    AltNative.Vehicle.Vehicle_SetPrimaryColor(ptr, primaryColor.Value);
                }

                if (numberPlate != IntPtr.Zero)
                {
                    AltNative.Vehicle.Vehicle_SetNumberplateText(ptr, numberPlate);
                }

                return ptr;
            });
            Dispose();
            Alt.Module.VehiclePool.Create(vehiclePtr, id, out var vehicle);
            return vehicle;
        }

        // Call Dispose when you don't wanna continue building the vehicle anymore to cleanup memory
        public void Dispose()
        {
            if (numberPlate != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(numberPlate);
            }
        }
    }
}