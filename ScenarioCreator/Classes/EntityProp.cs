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
    public class EntityProp : EntityBase
    {
        public override int Id  { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override int localEntityId { get; set; }
        public override int netEntityId { get; set; }

        public int AttachedToPedId  { get; }
        public dynamic AttachedMetadata  { get; }
        public EntityProp(
            int id,
            string model,
            Vector3 position,
            Vector3 rotation,
            int attachedToPedId = -1,
            dynamic attachedMetadata = null
        )
        {
            Id = id;
            Model = model;
            Position = position;
            Rotation = rotation;
            AttachedToPedId = attachedToPedId;
            AttachedMetadata = attachedMetadata;
        }
        public override async void BeforeInitialization()
        {
            int modelHash = GetHashKey( Model ); 
            await Utils.LoadEntityModel( (uint)modelHash );

            var locEntId = CreateObject( modelHash, Position.X, Position.Y, Position.Z, true, true, true);
            SetEntityRotation( locEntId, Rotation.X, Rotation.Y, Rotation.Z, 2, false);

            localEntityId = locEntId;
            netEntityId = NetworkGetNetworkIdFromEntity( localEntityId );

            while (NetworkGetNetworkIdFromEntity( localEntityId ) == 0) {
                await BaseScript.Delay(100);
            }
        }

        // public override void BeforeDestroy()
        // {
        //     if ( localEntity != null && localEntity.Handle != null ) {
        //         if ( DoesEntityExist( localEntity.Handle ) ) {
        //             DeleteEntity( localEntity?.Handle );
        //         }
        //     }
        // }
        
        public override void DrawOnWorld()
        {

        }
    }
}