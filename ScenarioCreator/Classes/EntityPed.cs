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
    class EntityPed : EntityBase
    {
        public override int Id  { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override Entity localEntity { get; set; }
        public override int netEntityId { get; set; }

        public int OutfitVariation  { get; }
        public string WeaponModel  { get; }
        public string Scenario  { get; }
        public string Anim  { get; }
        public string Dict  { get; }
        public uint Flags  { get; }
        public string Relationship  { get; }
        public bool IsFreezed  { get; }
        public bool IsInvincible  { get; }

        public EntityPed(
            int id,
            string model,
            Vector3 position,
            Vector3 rotation,
            int outfitVariation,
            string weaponModel = "WEAPON_UNARMED",
            string scenario = "",
            string anim = "",
            string dict = "",
            uint flags = 0,
            string relationship = "PLAYER",
            bool isFreezed = false,
            bool isInvincible = false
        )
        {
            Id = id;
            Model = model;
            Position = position;
            Rotation = rotation;
            OutfitVariation = outfitVariation;
            WeaponModel = weaponModel;
            Scenario = scenario;
            Anim = anim;
            Dict = dict;
            Flags = flags;
            Relationship = relationship;
            IsFreezed = isFreezed;
            IsInvincible = isInvincible;
        }

        public override async void BeforeInitialization()
        {
            int modelHash = GetHashKey( Model ); 
            await Utils.LoadEntityModel( (uint)modelHash );

            var localEntityId = CreatePed(4, (uint)modelHash, Position.X, Position.Y, Position.Z, Rotation.Z, true, true);

            SetEntityRotation( localEntityId, Rotation.X, Rotation.Y, Rotation.Z, 2, false );

            localEntity = Entity.FromHandle(localEntityId);
            netEntityId = NetworkGetNetworkIdFromEntity( localEntityId );
        }

        public void StopPedActions()
        {
            var lEntity = GetEntity();
            ClearPedTasksImmediately( lEntity.Handle );
            FreezeEntityPosition( lEntity.Handle, true );
        }

        public void ResetPed()
        {
            var lEntity = GetEntity();
            SetEntityCoords( lEntity.Handle, Position.X, Position.Y, Position.Z, true, false, false, false);
            SetEntityRotation( lEntity.Handle, Rotation.X, Rotation.Y, Rotation.Z, 2, false );
        }

        public void PlayPedActions() 
        {
            var lEntity = GetEntity();
            
        }

        public override void DrawOnWorld()
        {

        }
    }
}