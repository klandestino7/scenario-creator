using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;
using ScenarioCreatorShared;

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
        Dictionary<int, int> Entities;

        public int entitiesCount = 0;

        public Scene(int id, string name, List<ScenarioVehicle> vehicles, List<ScenarioPed> peds, List<ScenarioProp> props)
        {
            Id = id;
            Name = name;

            if ( peds != null ) {
                InstantiatePeds( peds );
            }

            if ( props != null ) {
                InstantiateProps( props );
            }
            
            if ( vehicles != null ) {
                InstantiateVehicles( vehicles );
            }
            
            Vehicles = new List<EntityVehicle>() { };
            Props = new List<EntityProp>() { };
            Peds = new List<EntityPed>() { };
            Entities = new Dictionary<int, int>() { };
        }

        private void InstantiateProps( dynamic props )
        {
            foreach (var prop in props)
            {
                _addPropToScene( new EntityProp( 
                    true,
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
                    true,
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
                    true,
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
            p.localEntityId = localId;
            p.netEntityId = NetworkGetNetworkIdFromEntity( localId );
            Props.Add( p );
            Entities[localId]= p.Id;
            entitiesCount += 1;

            BaseScript.TriggerLatentServerEvent("scenarioCreator:addPropToScene", 1024, Id, JsonConvert.SerializeObject(p));
        }
        void _addPedToScene( EntityPed p, int localId = -1 )
        {
            p.localEntityId = localId;
            p.netEntityId = NetworkGetNetworkIdFromEntity( localId );
            Peds.Add( p );
            Entities[localId]= p.Id;
            entitiesCount += 1;

            BaseScript.TriggerLatentServerEvent("scenarioCreator:addPedToScene", 1024, Id, JsonConvert.SerializeObject(p));
        }

        void _addVehicleToScene( EntityVehicle v, int localId = -1 )
        {
            v.localEntityId = localId;
            v.netEntityId = NetworkGetNetworkIdFromEntity( localId );
            Vehicles.Add( v );
            Entities[localId]= v.Id;
            entitiesCount += 1;

            BaseScript.TriggerLatentServerEvent("scenarioCreator:addVehicleToScene", 1024, Id, JsonConvert.SerializeObject(v));
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

        public EntityBase GetEntityInstanceFromHandleId(int entityId)
        {
            Entities.TryGetValue(entityId, out int classId);
            var entityType = GetEntityType( entityId );

            var idToFind = classId;

            Debug.WriteLine($" GetEntityInstanceFromHandleId :: idToFind {idToFind}");

            EntityBase entityFound = Peds.Find(p => p.Id == idToFind);

            switch( entityType ) {
                case (int)eEntityTypeToClass.EntityPed:
                    EntityPed foundPed = Peds.Find(p => p.Id == idToFind);
                    entityFound = foundPed;
                break;

                case (int)eEntityTypeToClass.EntityVehicle:
                    EntityVehicle foundVehicle = Vehicles.Find(p => p.Id == idToFind);
                    entityFound = foundVehicle;
                break;
                
                case (int)eEntityTypeToClass.EntityProp:
                    EntityProp foundObj = Props.Find(p => p.Id == idToFind);
                    entityFound = foundObj;
                break;
            }

            return entityFound;
        }

        public List<EntityBase> GetEntities()
        {
            List<EntityBase> _entities = new List<EntityBase>( ) {};

            foreach (var item in Entities)
            {
                EntityBase foundObj = GetEntityInstanceFromHandleId( item.Key );
                _entities.Add( foundObj );
            }

            return _entities;
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
            BaseScript.TriggerLatentServerEvent("scenarioCreator:forceSaveScene", 1024, Id, Vehicles);
        }

        public void BeforeDestroy()
        {
            StopScene();
            
            foreach (var entity in Entities)
            {
                EntityBase _entity = GetEntityInstanceFromHandleId( entity.Key );
                _entity.BeforeDestroy();
                entitiesCount -= 1;
            }
        }
    }

}