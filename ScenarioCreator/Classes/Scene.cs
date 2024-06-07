using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient.Classes
{
    public enum eEntityTypeToClass  {
        EntityPed = 1,
        EntityVehicle = 2,
        EntityProp = 3
    }
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
            
            if ( props != null ) {
                InstantiateProps( props );
            }
            
            if ( vehicles != null ) {
                InstantiateVehicles( vehicles );
            }
            
            if ( peds != null ) {
                InstantiatePeds( peds );
            }

            Vehicles = new List<EntityVehicle>() { };
            Props = new List<EntityProp>() { };
            Peds = new List<EntityPed>() { };
            Entities = new List<KeyValuePair<int, int>>() { };
        }

        private void InstantiateProps( dynamic props )
        {
            foreach (var prop in props)
            {
                _addPropToScene( new EntityProp( 
                    prop["id"] == -1 ? Props.Count + 1 : prop["id"],
                    prop["model"],
                    prop["position"],
                    prop["rotation"],
                    prop["attachedToPedId"],
                    prop["attachedMetadata"]
                ));
            }
        }
        private void InstantiateVehicles( dynamic vehicles )
        {
            foreach (var vehicle in vehicles)
            {
                _addVehicleToScene( new EntityVehicle( 
                    vehicle["id"] == -1 ? Props.Count + 1 : vehicle["id"],
                    vehicle["model"],
                    vehicle["position"],
                    vehicle["rotation"],
                    vehicle["props"],
                    vehicle["plate"],
                    vehicle["pedDriver"],
                    vehicle["pedDriverMetadata"]
                ));
            }
        }
        private void InstantiatePeds( dynamic peds )
        {
            foreach (var ped in peds)
            {
                _addPedToScene( new EntityPed( 
                    ped["id"] == -1 ? Props.Count + 1 : ped["id"],
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
                ));
            }
        }

        void _addPropToScene( EntityProp p, int localId = -1 ) 
        {
            Props.Add( p );
            Entities.Add( new KeyValuePair<int, int>(localId, p.Id) );
        }
        void _addPedToScene( EntityPed p, int localId = -1 )
        {
            Peds.Add( p );
            Entities.Add( new KeyValuePair<int, int>(localId, p.Id) );
        }

        void _addVehicleToScene( EntityVehicle v, int localId = -1 )
        {
            Vehicles.Add( v );
            Entities.Add( new KeyValuePair<int, int>(localId, v.Id) );
        }

        public void AddPropToScene( EntityProp v, int localId ) 
        {
            _addPropToScene( v, localId );
        }
        
        public void AddPedToScene( EntityPed v, int localId ) 
        {
            _addPedToScene( v,localId );
        }
        public void AddVehicleToScene( EntityVehicle v, int localId ) 
        {
            _addVehicleToScene ( v, localId );
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