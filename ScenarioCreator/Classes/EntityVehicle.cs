using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json.Serialization;

using ScenarioCreatorShared;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient.Classes
{
    public class EntityVehicle : EntityBase
    {
        public override int Id  { get; set; }
        public override int localId { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override int localEntityId { get; set; }
        public override int netEntityId { get; set; }
        public Dictionary<string, dynamic> Props  { get; }
        public string Plate  { get; }
        public int PedDriver  { get; set; }
        public _PedDriverMetadata PedDriverMetadata  { get; }

        public EntityVehicle(
            int id,
            string model,
            Vector3 position,
            Vector3 rotation,
            Dictionary<string, dynamic> props,
            string plate,
            int pedDriver = -1,
            _PedDriverMetadata pedDriverMetadata = null
        )
        {
            Id = id;
            Model = model;
            Position = position;
            Rotation = rotation;
            Props = props;
            Plate = plate;
            PedDriver = pedDriver;
            PedDriverMetadata = new _PedDriverMetadata(pedDriverMetadata?.DriverStyle ?? 0, pedDriverMetadata?.ToPosition ?? new Vector3(), pedDriverMetadata?.MaxSpeed ?? 0);
        }

        public override async Task<bool> BeforeInitialization()
        {
            // Debug.WriteLine($" BeforeInitialization :: ");
            uint modelHash = (uint)GetHashKey( Model ); 

            if (!IsModelValid(modelHash))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return false;
            }

            await Utils.LoadEntityModel( modelHash );

            // Debug.WriteLine($" BeforeInitialization :: LoadEntityModel ");

            var locEntId = CreateVehicle(modelHash, Position.X, Position.Y, Position.Z, Rotation.Z, true, false);

            // Debug.WriteLine($" BeforeInitialization :: SpawnVehicle {locEntId}");
            localEntityId = locEntId;
            
            while (NetworkGetNetworkIdFromEntity( locEntId ) == 0) {
                await BaseScript.Delay(100);
            }
        
            netEntityId = NetworkGetNetworkIdFromEntity( locEntId );
            return true;
        }
        
        public override void StartAct()
        {
            if ( PedDriverMetadata != null ) 
            {
                var pedDriver = GetPedInVehicleSeat(localEntityId, -1);

                if ( DoesEntityExist( pedDriver ) )
                {

                    if ( !PedDriverMetadata.ToPosition.IsZero )
                    {
                        var toPosition = PedDriverMetadata.ToPosition;
                        TaskVehicleDriveToCoord(
                            pedDriver,
                            localEntityId,
                            toPosition.X,
                            toPosition.Y,
                            toPosition.Z,
                            PedDriverMetadata.MaxSpeed,
                            1,
                            (uint)GetEntityModel(localEntityId),
                            PedDriverMetadata.DriverStyle,
                            6.0f,
                            0.0f
                        );
                    }
                }
            }
        }
        public override void StopAct()
        {
            
        }

        public void SetPedDriver( int pedDriverId )
        {
            PedDriver = pedDriverId;
        }

        public void SetToPosition( Vector3 toPosition)
        {
            PedDriverMetadata.ToPosition = toPosition;
        }
        public void SetMaxSpeed(int maxSpeed)
        {
            PedDriverMetadata.MaxSpeed = maxSpeed;
        }
        public void SetDriveStyle(int driveStyle)
        {
            PedDriverMetadata.DriverStyle = driveStyle;
        }
    }
}