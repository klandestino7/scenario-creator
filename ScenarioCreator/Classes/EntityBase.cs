using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
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
        public abstract void BeforeInitialization();
        // public abstract void BeforeDestroy();
        public abstract void DrawOnWorld();

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
    }
}