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
        public MainServer()
        {
            Debug.WriteLine($"MainServer INITIS");
            EventHandlers["scenarioCreator:forceSaveScene"] += new Action<int, int, string, string, string>(OnSaveScene);
            EventHandlers["scenarioCreator:getAllScenes"] += new Action<int, NetworkCallbackDelegate>(OnGetAllScenes);
            EventHandlers["scenarioCreator:getSceneDataFromDb"] += new Action<int, int, NetworkCallbackDelegate>(OnGetSceneFromDB);
        }

        public void OnSaveScene(int playerId, int sceneId, string peds, string props, string vehicles) 
        {
            
        }

        public void OnGetSceneFromDB(int playerId, int sceneId, NetworkCallbackDelegate cbFunction) 
        {
            var res = "{\"id\":2,\"name\": \"Eu sou um senário mais ou menos\"}";
            cbFunction(res);
        }

        public void OnGetAllScenes(int playerId, NetworkCallbackDelegate cbFunction) 
        {
            var res = "[{\"id\":1,\"name\": \"Eu sou um senario legal\"}, {\"id\":2,\"name\": \"Eu sou um senário mais ou menos\"}]";
            cbFunction(res);
        }
    }
}
