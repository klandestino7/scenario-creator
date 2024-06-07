using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient.Classes
{

    public class Scene
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Func<Task<bool>> Handle;

        public bool IsActive = false;
        public bool IsPaused = false;
        List<EntityVehicle> Vehicles;
        List<EntityPed> Peds;
        List<EntityProp> Props;
        List<KeyValuePair<int, int>> Entities;

        public Scene(int id, string name, dynamic vehicles, dynamic peds, dynamic props)
        {
            Id = id;
            Name = name;
            
            InstantiateProps( props );
            InstantiateVehicles( vehicles );
            InstantiatePeds( peds );
        }

        private void InstantiateProps( dynamic props )
        {
            foreach (var prop in props)
            {
                var p = new EntityProp( 
                    prop["id"],
                    prop["model"],
                    prop["position"],
                    prop["rotation"],
                    prop["attachedToPedId"],
                    prop["attachedMetadata"]
                );

                Props.Add( p );
                Entities.Add( new KeyValuePair<int, int>(p.localEntity.Handle, p.Id) );
            }
        }
        private void InstantiateVehicles( dynamic vehicles )
        {
            foreach (var vehicle in vehicles)
            {
                var v =  new EntityVehicle( 
                    vehicle["id"],
                    vehicle["model"],
                    vehicle["position"],
                    vehicle["rotation"],
                    vehicle["props"],
                    vehicle["plate"],
                    vehicle["pedDriver"],
                    vehicle["pedDriverMetadata"]
                );

                Vehicles.Add( v );
                Entities.Add( new KeyValuePair<int, int>(v.localEntity.Handle, v.Id) );
            }
        }
        private void InstantiatePeds( dynamic peds )
        {
            foreach (var ped in peds)
            {
                var p = new EntityPed( 
                    ped["id"],
                    ped["model"],
                    ped["position"],
                    ped["rotation"],
                    ped["outfitVariation"],
                    ped["weaponModel"],
                    ped["scenario"],
                    ped["anim"],
                    ped["dict"],
                    ped["flags"],
                    ped["relationship"],
                    ped["isFreezed"],
                    ped["isInvincible"]
                );

                Peds.Add( p );
                Entities.Add( new KeyValuePair<int, int>(p.localEntity.Handle, p.Id) );
            }
        }

        EntityBase GetEntityInstanceFromHandleId(int entityId)
        {
             var ent = Entities[entityId];
            var entityType = GetEntityType( ent.Value );
            var idToFind = ent.Key;

            EntityBase entityFound = Peds.Find(p => p.Id == idToFind);

            switch( entityType ) {
                case 1:
                    EntityPed foundPed = Peds.Find(p => p.Id == idToFind);
                    entityFound = foundPed;
                break;

                case 2:
                    EntityVehicle foundVehicle = Vehicles.Find(p => p.Id == idToFind);
                    entityFound = foundVehicle;
                break;
                
                case 3:
                    EntityProp foundObj = Props.Find(p => p.Id == idToFind);
                    entityFound = foundObj;
                break;
            }

            return entityFound;
        }

        public void AddVehicleToScene() {

        }
        
        public void AddPedToScene() {
            
        }

        public void AddPropToScene() {
            
        }

        public void DeleteEntityFromHandleId(int entityId)
        {
            EntityBase _entity = GetEntityInstanceFromHandleId( entityId );
            _entity.BeforeDestroy();
        }

        public void StartScene()
        {

        }

        public void PauseScene()
        {

        }

        public void StopScene()
        {

        }

        public void ForceSaveScene() 
        {
            
        }

        public void BeforeDestroy()
        {
            StopScene();
            
            foreach (var entity in Entities)
            {
                EntityBase _entity = GetEntityInstanceFromHandleId( entity.Key );
                _entity.BeforeDestroy();
            }
        }
    }

}