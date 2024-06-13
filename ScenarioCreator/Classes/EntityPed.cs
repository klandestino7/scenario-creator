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
    public class EntityPed : EntityBase
    {
        public override int Id  { get; set; }
        public override string Model  { get; set; }
        public override Vector3 Position  { get; set; }
        public override Vector3 Rotation  { get; set; }
        public override int localEntityId { get; set; }
        public override int netEntityId { get; set; }

        public int OutfitVariation  { get; }
        public string WeaponModel  { get; set; }
        public string Scenario  { get; set; }
        public string Anim  { get; set; }
        public string Dict  { get; set; }
        public int Flags  { get; set; }
        public string Relationship  { get; set; }
        public bool IsFreezed  { get; set; }
        public bool IsInvincible  { get; set; }

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
            int flags = 0,
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

        public override async Task<bool> BeforeInitialization()
        {
            int modelHash = GetHashKey( Model ); 
            await Utils.LoadEntityModel( (uint)modelHash );

            var locEntId = CreatePed(4, (uint)modelHash, Position.X, Position.Y, Position.Z - 0.98f, Rotation.Z, true, true);

            SetEntityRotation( locEntId, Rotation.X, Rotation.Y, Rotation.Z, 2, false );

            localEntityId = locEntId;

            while (NetworkGetNetworkIdFromEntity( locEntId ) == 0) {
                await BaseScript.Delay(100);
            }
            
            netEntityId = NetworkGetNetworkIdFromEntity( locEntId );

            AddWeaponToPed( locEntId );
            AddToRelationship( locEntId );

            return true;
        }

        public void ReloadPedConfig()
        {
            var lEntity = GetLocalEntity();
            AddWeaponToPed( lEntity.Handle );
            AddToRelationship( lEntity.Handle );
        }

        public void AddToRelationship( int locEntId)
        {
            if ( Relationship != "") 
            {
                var gHash = (uint)GetHashKey(Relationship);
                AddRelationshipGroup(Relationship, ref gHash);
                SetPedRelationshipGroupHash( locEntId ,gHash);

                var playerHash = (uint) GetHashKey("PLAYER");
                SetRelationshipBetweenGroups(3, playerHash, gHash);
                SetRelationshipBetweenGroups(3, gHash, playerHash);
            }
        }
        public void AddWeaponToPed( int locEntId )
        {
            // Debug.WriteLine($" WeaponModel :: {WeaponModel }");
            if ( WeaponModel != "WEAPON_UNARMED") 
            {
                var weaponModelHash = (uint)GetHashKey(WeaponModel );

                if ( !IsWeaponValid( weaponModelHash )) {
                    return;
                }

                // Debug.WriteLine($" GiveWeaponToPed :: {weaponModelHash }");
                GiveWeaponToPed( locEntId, weaponModelHash, 300, false, true );
                SetCurrentPedWeapon( locEntId, weaponModelHash, true);
            }
        }

        public async void PlayPedActions() 
        {
            var lEntity = GetLocalEntity();
            var isUsingScenario = Scenario != "";

            if ( isUsingScenario ) {
                TaskStartScenarioInPlace( lEntity.Handle, Scenario, 0 , false );
            }
            else
            {
                if ( Anim != "")
                {
                    BaseScript.TriggerEvent("dpemotes:executeEmote", Anim, lEntity.Handle);
                    // await Utils.LoadAnimDict( Dict );  
                    // TaskPlayAnim( lEntity.Handle, Dict, Anim, 8.0f, 8.0f, -1, (int)Flags, 1, false, false, false);
                }
            }
        }
        public void AddScenario(string scenario) {
            Scenario = scenario;
        }
        public void AddAnimDict(string animDict) {
            Dict = animDict;
        }
        public void AddAnimName(string anim) {
            Anim = anim;
        }
        public void AddFlag(int flag) {
            Flags = flag;
        }
        public void SetWeapon(string weapon) {
            WeaponModel = weapon;
        }
        public void SetFreezed(bool freezed) {
            IsFreezed = freezed;
            var lEntity = GetLocalEntity();
            FreezeEntityPosition( lEntity.Handle , freezed );
        }
        public void SetInvincible(bool invincible) {
            IsInvincible = invincible;
            var lEntity = GetLocalEntity();
            SetEntityInvincible( lEntity.Handle , invincible );
        }
        public override void StartAct()
        {
            PlayPedActions();
        }
        public override void StopAct()
        {
            var lEntity = GetLocalEntity();
            ClearPedTasks( lEntity.Handle );
        }
    }
}