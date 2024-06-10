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

        private async void InstantiateProps( List<ScenarioProp> props )
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

                await pp.BeforeInitialization();
                
                Props.Add( pp );
                Entities[pp.localEntityId]= pp.Id;
                entitiesCount += 1;
            }
        }
        private async void InstantiateVehicles( List<ScenarioVehicle> vehicles )
        {
            foreach (var vehicle in vehicles)
            {

                Debug.WriteLine($" vehicle.PedDriverMetadata :: {vehicle.PedDriverMetadata}");
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

                await veh.BeforeInitialization();

                AddPedIntoVehicle( veh.localEntityId, vehicle.PedDriver );

                Vehicles.Add( veh );
                Entities[veh.localEntityId]= veh.Id;
                entitiesCount += 1;
            }
        }
        private async void InstantiatePeds( List<ScenarioPed> peds )
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

                await pd.BeforeInitialization();

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
        async void _addPedToScene( EntityPed p, int localId = -1 )
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

        public void RemoveEntityFromScene( int localId )
        {
            var entityType = GetEntityType( localId );
                // Debug.WriteLine($" RemoveEntityFromScene :: {localId} {entityType}");

              switch( entityType ) {
                case (int)Globals.eEntityTypeToClass.EntityPed:
                    EntityPed foundPed = Peds.Find(p => p.localEntityId == localId);
                    foundPed.PermanentDelete();
                    Peds.Remove( foundPed );
                break;

                case (int)Globals.eEntityTypeToClass.EntityVehicle:
                    EntityVehicle foundVehicle = Vehicles.Find(p => p.localEntityId == localId);
                    foundVehicle.PermanentDelete();
                    Vehicles.Remove( foundVehicle );
                break;
                
                case (int)Globals.eEntityTypeToClass.EntityProp:
                    EntityProp foundObj = Props.Find(p => p.localEntityId == localId);
                    foundObj.PermanentDelete();
                    Props.Remove( foundObj );
                break;
            }

            Entities.Remove( localId );
        }

        public int GetIdEntityFromInternalId( int id ) 
        {
            foreach (var entity in Entities) {
                if (entity.Value == id )
                {
                    return entity.Key;
                }
            }

            return -1;
        }

        public EntityBase GetEntityInstanceFromHandleId(int entityId)
        {
            Entities.TryGetValue(entityId, out int classId);
            var entityType = GetEntityType( entityId );

            var idToFind = classId;

            // Debug.WriteLine($" GetEntityInstanceFromHandleId :: idToFind {idToFind}");

            EntityBase entityFound = Peds.Find(p => p.Id == idToFind);

            switch( entityType ) {
                case (int)Globals.eEntityTypeToClass.EntityPed:
                    EntityPed foundPed = Peds.Find(p => p.Id == idToFind);
                    entityFound = foundPed;
                break;

                case (int)Globals.eEntityTypeToClass.EntityVehicle:
                    EntityVehicle foundVehicle = Vehicles.Find(p => p.Id == idToFind);
                    entityFound = foundVehicle;
                break;
                
                case (int)Globals.eEntityTypeToClass.EntityProp:
                    EntityProp foundObj = Props.Find(p => p.Id == idToFind);
                    entityFound = foundObj;
                break;
            }

            return entityFound;
        }

        public EntityVehicle GetVehicleInstanceFromEntityHandle( int entityId ) 
        {
            Debug.WriteLine($"GetVehicleInstanceFromEntityHandle :: {entityId}");
            return Vehicles.Find(p => p.localEntityId == entityId);
        }
        public EntityPed GetPedInstanceFromEntityHandle( int entityId ) 
        {
            return Peds.Find(p => p.localEntityId == entityId);
        }
        public EntityProp GetPropInstanceFromEntityHandle( int entityId ) 
        {
            return Props.Find(p => p.localEntityId == entityId);
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

        public void AddPedIntoVehicle( int locEntId, int pedDriver )
        {
            // Debug.WriteLine($" AddPedIntoVehicle {locEntId} - {pedDriver}");
            if ( pedDriver != -1) 
            {
                int localPedId = GetIdEntityFromInternalId( pedDriver );

                if ( localPedId != -1 && DoesEntityExist( localPedId ))
                {
                    TaskEnterVehicle( localPedId, locEntId, 10000, -1, 1.0f, 1, 0);
                }
            }
        }

        public void DeleteEntityFromHandleId(int entityId)
        {

        }
        public void RestartScene()
        {
            StopScene();

            foreach (var item in Entities)
            {
                EntityBase foundObj = GetEntityInstanceFromHandleId( item.Key );
                foundObj.ResetEntity();
            }

            foreach ( var vehicle in Vehicles )
            {
                if ( vehicle.PedDriver != -1 )
                {
                    AddPedIntoVehicle( vehicle.localEntityId, vehicle.PedDriver );
                }
            }
        }

        public void StartScene()
        {
            foreach (var item in Entities)
            {
                EntityBase foundObj = GetEntityInstanceFromHandleId( item.Key );
                foundObj.StartAct();
            }
        }

        public void StopScene()
        {

            foreach (var item in Entities)
            {
                EntityBase foundObj = GetEntityInstanceFromHandleId( item.Key );
                foundObj.StopAct();
            }
        }

        public void ForceSaveScene() 
        {
            BaseScript.TriggerLatentServerEvent("scenarioCreator:forceSaveScene", 1024, Id, Vehicles);
        }

        public void PermanentDeleteEntityFromHandle( int entityId )
        {   
            EntityBase _entity = GetEntityInstanceFromHandleId( entityId );

            _entity.BeforeDestroy();
        }

        public void BeforeDestroy()
        {
            StopScene();

            // Debug.WriteLine(" BeforeDestroy :: ");
            
            foreach (var entity in Entities)
            {
                // Debug.WriteLine($" entity.Key :: {entity.Key}");
                EntityBase _entity = GetEntityInstanceFromHandleId( entity.Key );
                _entity.BeforeDestroy();
                entitiesCount -= 1;
            }
        }
    }

}