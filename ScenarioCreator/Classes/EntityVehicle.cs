using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json.Serialization;


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
        public Dictionary<string, dynamic> Props  { get; }
        public string Plate  { get; }
        public int PedDriver  { get; }
        public dynamic PedDriverMetadata  { get; }

        public EntityVehicle(
            int id,
            string model,
            Vector3 position,
            Vector3 rotation,
            Dictionary<string, dynamic> props,
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
            Debug.WriteLine($" BeforeInitialization :: ");
            uint modelHash = (uint)GetHashKey( Model ); 

            if (!IsModelValid(modelHash))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return;
            }

            await Utils.LoadEntityModel( modelHash );

            Debug.WriteLine($" BeforeInitialization :: LoadEntityModel ");

            var locEntId = CreateVehicle(modelHash, Position.X, Position.Y, Position.Z, Rotation.Z, true, false);

            Debug.WriteLine($" BeforeInitialization :: SpawnVehicle {locEntId}");
            localEntityId = locEntId;
            
            while (NetworkGetNetworkIdFromEntity( locEntId ) == 0) {
                await BaseScript.Delay(100);
            }
        
            netEntityId = NetworkGetNetworkIdFromEntity( locEntId );
        }

        public override void DrawOnWorld()
        {

        }
    }
}