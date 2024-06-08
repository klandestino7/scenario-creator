using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json.Serialization;

using ScenarioCreatorClient.Scripts;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient.Classes
{
    public class EntityVehicle : EntityBase
    {
        public override int Id  { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override int localEntityId { get; set; }
        public override int netEntityId { get; set; }
        public Dictionary<string, int> Props  { get; }
        public string Plate  { get; }
        public int PedDriver  { get; }
        public dynamic PedDriverMetadata  { get; }

        public EntityVehicle(
            bool createEntity,
            int id,
            string model,
            Vector3 position,
            Vector3 rotation,
            Dictionary<string, int> props,
            string plate,
            int pedDriver = -1,
            dynamic pedDriverMetadata = null
        )
        {
            Id = id;
            Model = model;
            Position = position;
            Rotation = rotation;
            Props = props;
            Plate = plate;
            PedDriver = pedDriver;
            PedDriverMetadata = pedDriverMetadata;

            if ( createEntity ) 
            {
                // BeforeInitialization();
            }
        }

        public override async void BeforeInitialization()
        {
            await Task.Delay(3000);
            Debug.WriteLine($" BeforeInitialization :: ");
            int modelHash = GetHashKey( Model ); 

            if (!IsModelValid((uint)modelHash))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return;
            }

            await Utils.LoadEntityModel( (uint)modelHash );

            Debug.WriteLine($" BeforeInitialization :: LoadEntityModel ");

            var locEntId = await CommonFunctions.SpawnVehicle( (uint)modelHash, false, false, skipLoad: false, vehicleInfo: new CommonFunctions.VehicleInfo(), saveName: null, Position.X, Position.Y, Position.Z, Rotation.Z);

            Debug.WriteLine($" BeforeInitialization :: SpawnVehicle {locEntId}");
            localEntityId = locEntId;
            netEntityId = NetworkGetNetworkIdFromEntity( locEntId );
            
            while (NetworkGetNetworkIdFromEntity( locEntId ) == 0) {
                await Task.Delay(100);
            }
        }
        
        public override void DrawOnWorld()
        {

        }
    }
}