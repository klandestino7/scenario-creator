using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using ScenarioCreatorShared;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient.Classes
{
    public abstract class EntityBase
    {
        public abstract int Id  { get; set; }
        public abstract string Model  { get; set; }
        public abstract Vector3 Position  { get; set; }
        public abstract Vector3 Rotation  { get; set; }
        public abstract int localEntityId { get; set; }
        private Entity localEntity { get; set; }
        public abstract int netEntityId { get; set; }
        public abstract Task<bool> BeforeInitialization();
        public abstract void StartAct();
        public abstract void StopAct();
        // public abstract void BeforeDestroy();


        public Entity GetLocalEntity() {
            var pedIdFromNet = NetworkGetEntityFromNetworkId( netEntityId );
            localEntity = Entity.FromHandle(pedIdFromNet);

            return localEntity;
        }

        public void BeforeDestroy()
        {
            var lEntity = GetLocalEntity();
            
            if ( lEntity.Handle != 0 ) {
                if ( DoesEntityExist( lEntity.Handle ) ) {
                    lEntity.Delete();
                }
            }
        }

        public void UpdateWorldOrientation()
        {
            var lEntity = GetLocalEntity();
            Position = lEntity.Position;
            Rotation = lEntity.Rotation;
        }

        public void ResetEntity()
        {
            var lEntity = GetLocalEntity();
            SetEntityCoords( lEntity.Handle, Position.X, Position.Y, Position.Z - 0.98f, true, false, false, false);
            SetEntityRotation( lEntity.Handle, Rotation.X, Rotation.Y, Rotation.Z, 2, false );
        }

        public void PermanentDelete()
        {
            var lEntity = GetLocalEntity();

            Func<bool, string> CallbackFunction = (res) =>
            {
                if ( res )
                {
                    if ( lEntity.Handle != 0 ) {
                        if ( DoesEntityExist( lEntity.Handle ) ) {
                            lEntity.Delete();
                        }
                    }
                }
                return "";
            };

            var entityType = GetEntityType( lEntity.Handle );
            var entityStringType = (Globals.eEntityTypeToClass)entityType;

            BaseScript.TriggerServerEvent("scenarioCreator:deleteEntity", Id, entityStringType, CallbackFunction);
        }
    }
}