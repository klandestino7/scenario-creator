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
        public abstract CitizenFX.Core.Entity localEntity { get; set; }
        public abstract int netEntityId { get; set; }
        public abstract void BeforeInitialization();
        // public abstract void BeforeDestroy();
        public abstract void DrawOnWorld();

        // public EntityBase(int id, string model, Vector3 position, Vector3 rotation)
        // {
        //     Id = id;
        //     Model = model;
        //     Position = position;
        //     Rotation = rotation;
        // }

        public Entity GetEntity() {
            if ( DoesEntityExist( localEntity.Handle ) )
            {
                var pedIdFromNet = NetworkGetNetworkIdFromEntity( localEntity.Handle );

                if ( pedIdFromNet != 0 && localEntity.Handle != pedIdFromNet )
                {
                    localEntity = Entity.FromHandle(pedIdFromNet);
                }
            }

            return localEntity;
        }

        public void BeforeDestroy()
        {
            var lEntity = GetEntity();
            if ( lEntity.Handle != 0 ) {
                if ( DoesEntityExist( lEntity.Handle ) ) {
                    lEntity.Delete();
                }
            }
        }

        // public void SetLocalEntity( Entity locEnt ) 
        // {
        //     localEntity = locEnt;
        // } 

        // public void BeforeDestroy()
        // {
        //     if ( localEntity != null && localEntity.Handle != null ) {
        //         if ( DoesEntityExist( localEntity.Handle ) ) {
        //             DeleteEntity( localEntity?.Handle );
        //         }
        //     }
        // }

        // ~EntityBase()
        // { 
        //     Debug.WriteLine("EntityBase's finalizer is called.");
        // }
    }
}