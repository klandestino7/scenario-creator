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
    public class EntityProp : EntityBase
    {
        public override int Id  { get; set; }
        public override int localId { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override int localEntityId { get; set; }
        public override int netEntityId { get; set; }

        public int AttachedToPedId  { get; }
        public _AttachedMetadata AttachedMetadata  { get; }
        public EntityProp(
            int id,
            string model,
            Vector3 position,
            Vector3 rotation,
            int attachedToPedId = -1,
            _AttachedMetadata attachedMetadata = null
        )
        {
            Id = id;
            Model = model;
            Position = position;
            Rotation = rotation;
            AttachedToPedId = attachedToPedId;
            AttachedMetadata = new _AttachedMetadata(attachedMetadata?.Offset ?? new Vector3(), attachedMetadata?.Rotation ?? new Vector3(), attachedMetadata?.HasCollision ?? false);
        }
        public override async Task<bool> BeforeInitialization()
        {
            int modelHash = GetHashKey( Model ); 
            await Utils.LoadEntityModel( (uint)modelHash );

            var locEntId = CreateObject( modelHash, Position.X, Position.Y, Position.Z, true, true, true);
            SetEntityRotation( locEntId, Rotation.X, Rotation.Y, Rotation.Z, 2, false);

            localEntityId = locEntId;

            while (NetworkGetNetworkIdFromEntity( locEntId ) == 0) {
                await BaseScript.Delay(100);
            }
            
            netEntityId = NetworkGetNetworkIdFromEntity( locEntId );

            return true;
        }

        // public override void BeforeDestroy()
        // {
        //     if ( localEntity != null && localEntity.Handle != null ) {
        //         if ( DoesEntityExist( localEntity.Handle ) ) {
        //             DeleteEntity( localEntity?.Handle );
        //         }
        //     }
        // }
    
        public override void StartAct()
        {
            
        }
        public override void StopAct()
        {
            
        }
    }
}