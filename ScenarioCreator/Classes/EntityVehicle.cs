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
        public override Entity localEntity { get; set; }
        public override int netEntityId { get; set; }
        public Dictionary<string, int> Props  { get; }
        public string Plate  { get; }
        public int PedDriver  { get; }
        public dynamic PedDriverMetadata  { get; }

        public EntityVehicle(
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
        }
        public override async void BeforeInitialization()
        {
            int modelHash = GetHashKey( Model ); 
            await Utils.LoadEntityModel( (uint)modelHash );

            var localEntityId = await CommonFunctions.SpawnVehicle( (uint)modelHash, false, false, skipLoad: false, vehicleInfo: new CommonFunctions.VehicleInfo(), saveName: null, Position.X, Position.Y, Position.Z, Game.PlayerPed.Heading);
            localEntity = Entity.FromHandle(localEntityId);
            netEntityId = NetworkGetNetworkIdFromEntity( localEntityId );
        }
        
        public override void DrawOnWorld()
        {

        }
    }
}