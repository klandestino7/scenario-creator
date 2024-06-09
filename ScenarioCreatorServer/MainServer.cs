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
            Debug.WriteLine($"MainServer INITIS");
            EventHandlers["scenarioCreator:getAllScenes"] += new Action<int, NetworkCallbackDelegate>(OnGetAllScenes);
            EventHandlers["scenarioCreator:getSceneDataFromDb"] += new Action<int, int, NetworkCallbackDelegate>(OnGetSceneFromDB);
            EventHandlers["scenarioCreator:createScene"] += new Action<string, NetworkCallbackDelegate>(OnCreateScene);

            EventHandlers["scenarioCreator:addVehicleToScene"] += new Action<int, string>(OnAddVehicleToScene);
            EventHandlers["scenarioCreator:addPropToScene"] += new Action<int, string>(OnAddPropToScene);
            EventHandlers["scenarioCreator:addPedToScene"] += new Action<int, string>(OnAddPedToScene);
            EventHandlers["scenarioCreator:saveVehiclesInScene"] += new Action<int, List<ScenarioVehicle>>(OnSaveVehiclesOnScene);
            EventHandlers["scenarioCreator:updateVehicle"] += new Action<int, ScenarioVehicle>(OnUpdateVehicleFromScene);
            EventHandlers["scenarioCreator:deleteVehicle"] += new Action<int>(OnDeleteVehicle);

            _repository = new ScenarioRepository();
        }

        public void OnResourceStop(string resName) 
        {
            if (resName == GetCurrentResourceName())
            {
                ScenarioRepository.StopMySQL();
            }
        }

        public void OnCreateScene(string sceneName, NetworkCallbackDelegate cbFunction)
        {
            int res = ScenarioRepository.CreateScene( sceneName );
            cbFunction( res );
        }

        public void OnDeleteVehicle(int vehicleId) 
        {
            ScenarioRepository.DeleteVehicleFromDBScene( vehicleId );
        }
        public void OnAddPropToScene(int sceneId, string propJson) 
        {
            ScenarioProp prop = JsonConvert.DeserializeObject<ScenarioProp>(propJson);
            ScenarioRepository.AddPropOnDBScene( sceneId, prop );
        }
        public void OnAddPedToScene(int sceneId, string pedJson) 
        {
            ScenarioPed ped = JsonConvert.DeserializeObject<ScenarioPed>(pedJson);
            ScenarioRepository.AddPedOnDBScene( sceneId, ped );
        }

        public void OnAddVehicleToScene(int sceneId, string vehicleJson) 
        {
            ScenarioVehicle vehicle = JsonConvert.DeserializeObject<ScenarioVehicle>(vehicleJson);
            ScenarioRepository.AddVehicleOnDBScene( sceneId, vehicle );
        }
        public void OnUpdateVehicleFromScene(int vehicleId, ScenarioVehicle vehicle) 
        {
            ScenarioRepository.UpdateVehicleFromDBScene( vehicleId, vehicle );
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

    }
}
