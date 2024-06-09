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

            Vehicles = new List<EntityVehicle>() { };
            Props = new List<EntityProp>() { };
            Peds = new List<EntityPed>() { };
            Entities = new Dictionary<int, int>() { };

            if ( peds != null ) {
                InstantiatePeds( peds );
            }

            if ( props != null ) {
                InstantiateProps( props );
            }
            
            if ( vehicles != null ) {
                InstantiateVehicles( vehicles );
            }
        }

        private void InstantiateProps( List<ScenarioProp> props )
        {
            foreach (var prop in props)
            {
                var pp = new EntityProp( 
                    prop.Id == -1 ? Props.Count + 1 : prop.Id,
                    prop.Model,
                    prop.Position,
                    prop.Rotation,
                    prop.AttachedToPedId,
                    prop.AttachedMetadata
                );

                pp.BeforeInitialization();
                
                Props.Add( pp );
                Entities[pp.localEntityId]= pp.Id;
                entitiesCount += 1;
            }
        }
        private void InstantiateVehicles( List<ScenarioVehicle> vehicles )
        {
            foreach (var vehicle in vehicles)
            {
                var veh = new EntityVehicle( 
                    vehicle.Id == -1 ? Vehicles.Count + 1 : vehicle.Id,
                    vehicle.Model,
                    vehicle.Position,
                    vehicle.Rotation,
                    null,
                    vehicle.Plate,
                    vehicle.PedDriver,
                    vehicle.PedDriverMetadata
                );

                veh.BeforeInitialization();

                Vehicles.Add( veh );
                Entities[veh.localEntityId]= veh.Id;
                entitiesCount += 1;
            }
        }
        private void InstantiatePeds( List<ScenarioPed> peds )
        {
            foreach (var ped in peds)
            {
                var pd = new EntityPed( 
                    ped.Id == -1 ? Peds.Count + 1 : ped.Id,
                    ped.Model,
                    ped.Position,
                    ped.Rotation,
                    ped.OutfitVariation,
                    ped.WeaponModel,
                    ped.Scenario,
                    ped.Anim,
                    ped.Dict,
                    ped.Flags,
                    ped.Relationship,
                    ped.IsFreezed,
                    ped.IsInvincible
                );

                pd.BeforeInitialization();

                Peds.Add( pd );
                Entities[pd.localEntityId]= pd.Id;
                entitiesCount += 1;
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
        public void AddVehicleToScene( EntityVehicle veh, int localId ) 
        {
            _addVehicleToScene ( veh, localId );
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

            Debug.WriteLine(" BeforeDestroy :: ");
            
            foreach (var entity in Entities)
            {
                Debug.WriteLine($" entity.Key :: {entity.Key}");
                EntityBase _entity = GetEntityInstanceFromHandleId( entity.Key );
                _entity.BeforeDestroy();
                entitiesCount -= 1;
            }
        }
    }

}