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
    class EntityProp : EntityBase
    {
        public override int Id  { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override Entity localEntity { get; set; }
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

            var localEntityId = CreateObject( modelHash, Position.X, Position.Y, Position.Z, true, true, true);
            localEntity = Entity.FromHandle(localEntityId);
            netEntityId = NetworkGetNetworkIdFromEntity( localEntityId );
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