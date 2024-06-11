using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;

using ScenarioCreatorShared;

using static CitizenFX.Core.Native.API;


namespace ScenarioCreatorServer
{
    public class MainServer : BaseScript
    {
        private ScenarioRepository _repository;
        public MainServer()
        {
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
            EventHandlers["scenarioCreator:isPlayerHasPermission"] += new Action<int, string, NetworkCallbackDelegate>(OnIsPlayerHasPermission);
            EventHandlers["scenarioCreator:requestDeleteScene"] += new Action<int, NetworkCallbackDelegate>(OnRequestDeleteScene);
            EventHandlers["scenarioCreator:getAllScenes"] += new Action<int, NetworkCallbackDelegate>(OnGetAllScenes);
            EventHandlers["scenarioCreator:getSceneDataFromDb"] += new Action<int, int, NetworkCallbackDelegate>(OnGetSceneFromDB);
            EventHandlers["scenarioCreator:createScene"] += new Action<string, string, NetworkCallbackDelegate>(OnCreateScene);

            EventHandlers["scenarioCreator:addVehicleToScene"] += new Action<int, string, NetworkCallbackDelegate>(OnAddVehicleToScene);
            EventHandlers["scenarioCreator:addPropToScene"] += new Action<int, string, NetworkCallbackDelegate>(OnAddPropToScene);
            EventHandlers["scenarioCreator:addPedToScene"] += new Action<int, string, NetworkCallbackDelegate>(OnAddPedToScene);
            EventHandlers["scenarioCreator:saveVehiclesInScene"] += new Action<int, List<ScenarioVehicle>>(OnSaveVehiclesOnScene);
            EventHandlers["scenarioCreator:updateVehicle"] += new Action<int, string>(OnUpdateVehicleFromScene);
            EventHandlers["scenarioCreator:updatePed"] += new Action<int, string>(OnUpdatePedFromScene);
            EventHandlers["scenarioCreator:updateProp"] += new Action<int, string>(OnUpdatePropFromScene);
            // EventHandlers["scenarioCreator:deleteVehicle"] += new Action<int>(OnDeleteVehicle);
            EventHandlers["scenarioCreator:deleteEntity"] += new Action<int, int, NetworkCallbackDelegate>(OnDeleteEntity);

            EventHandlers["scenarioCreator:updateEntityWorldPosition"] += new Action<int, Globals.eEntityTypeToClass, string, string>(OnUpdateEntityWorldPosition);

            _repository = new ScenarioRepository();
        }

        public void OnResourceStop(string resName) 
        {
            if (resName == GetCurrentResourceName())
            {
                ScenarioRepository.StopMySQL();
            }
        }

        public void OnRequestDeleteScene( int sceneId, NetworkCallbackDelegate cbFunction ) 
        {
            ScenarioRepository.DeleteSceneFromDB( sceneId );
        }

        public void OnCreateScene(string sceneName, string position, NetworkCallbackDelegate cbFunction)
        {
            int res = ScenarioRepository.CreateScene( sceneName, position );
            cbFunction( res );
        }

        public void OnIsPlayerHasPermission( int playerId, string permission,  NetworkCallbackDelegate cbFunction)
        {
            cbFunction( IsPlayerAceAllowed(playerId.ToString(), permission) );
        }

        public void OnDeleteEntity(int vehicleId, int entityType, NetworkCallbackDelegate cbFunction) 
        {
            // Debug.WriteLine($"OnDeleteEntity :: {vehicleId} {entityType}");

            switch( entityType ) 
            {
                case (int)Globals.eEntityTypeToClass.EntityPed:
                    ScenarioRepository.DeletePedFromDBScene( vehicleId );
                break;
                case (int)Globals.eEntityTypeToClass.EntityVehicle:
                    ScenarioRepository.DeleteVehicleFromDBScene( vehicleId );
                break;
                case (int)Globals.eEntityTypeToClass.EntityProp:
                    ScenarioRepository.DeletePropFromDBScene( vehicleId );
                break;
            }

            cbFunction(true);
        }
        public void OnAddPropToScene(int sceneId, string propJson, NetworkCallbackDelegate cbFunction) 
        {
            ScenarioProp prop = JsonConvert.DeserializeObject<ScenarioProp>(propJson);
            int id = ScenarioRepository.AddPropOnDBScene( sceneId, prop );
            cbFunction( id );
        }
        public void OnAddPedToScene(int sceneId, string pedJson, NetworkCallbackDelegate cbFunction) 
        {
            ScenarioPed ped = JsonConvert.DeserializeObject<ScenarioPed>(pedJson);
            int id = ScenarioRepository.AddPedOnDBScene( sceneId, ped );
            cbFunction( id );
        }

        public void OnAddVehicleToScene(int sceneId, string vehicleJson, NetworkCallbackDelegate cbFunction) 
        {
            ScenarioVehicle vehicle = JsonConvert.DeserializeObject<ScenarioVehicle>(vehicleJson);
            int id = ScenarioRepository.AddVehicleOnDBScene( sceneId, vehicle );
            cbFunction( id );
        }
        public void OnUpdateVehicleFromScene(int vehicleId, string vehicle ) 
        {
            ScenarioRepository.UpdateVehicleFromDBScene( vehicleId, JsonConvert.DeserializeObject<ScenarioVehicle>(vehicle) );
        }
        public void OnUpdatePedFromScene(int pedId, string ped) 
        {
            ScenarioRepository.UpdatePedFromDBScene( pedId, JsonConvert.DeserializeObject<ScenarioPed>(ped) );
        }
        public void OnUpdatePropFromScene(int propId, string prop) 
        {
            ScenarioRepository.UpdatePropFromDBScene( propId, JsonConvert.DeserializeObject<ScenarioProp>(prop) );
        }
        public void OnSaveVehiclesOnScene(int scenarioId, List<ScenarioVehicle> vehicles) 
        {
            ScenarioRepository.AddVehiclesOnDBScene( scenarioId, vehicles );
        }

        public void OnGetSceneFromDB(int playerId, int sceneId, NetworkCallbackDelegate cbFunction) 
        {
            Scenario _scenario = ScenarioRepository.GetScenarioFromId( (int)sceneId  );
            List<ScenarioPed> _peds = ScenarioRepository.GetAllPedsFromScenario( (int)sceneId );
            List<ScenarioProp> _props = ScenarioRepository.GetAllPropsFromScenario( (int)sceneId  );
            List<ScenarioVehicle> _vehicles = ScenarioRepository.GetAllVehiclesFromScenario( (int)sceneId  );
            cbFunction( JsonConvert.SerializeObject(_scenario), JsonConvert.SerializeObject(_peds), JsonConvert.SerializeObject(_props), JsonConvert.SerializeObject(_vehicles) );
        }

        public void OnGetAllScenes(int playerId, NetworkCallbackDelegate cbFunction) 
        {
            List<Scenario> _scenarios = ScenarioRepository.GetAllScenes();
            cbFunction(JsonConvert.SerializeObject(_scenarios));
        }

        public void OnUpdateEntityWorldPosition(int entityId, Globals.eEntityTypeToClass entityType, string position, string rotation)
        {
            string entityDbTable = "empty_table";

            switch ( entityType ) {
                case Globals.eEntityTypeToClass.EntityPed:
                    entityDbTable = "scenario_peds";
                break;
                case Globals.eEntityTypeToClass.EntityProp:
                    entityDbTable = "scenario_props";
                break;
                case Globals.eEntityTypeToClass.EntityVehicle:
                    entityDbTable = "scenario_vehicles";
                break;
            }

            ScenarioRepository.UpdateEntityWorldPosition( entityId, entityDbTable, position, rotation);
        }
    }
}
