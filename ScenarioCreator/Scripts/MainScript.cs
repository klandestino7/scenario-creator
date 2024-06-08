using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using ScenarioCreatorClient.Scripts;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient
{
    public class MainScript : BaseScript
    {
        public int CurrentSceneSelectedId;

        public SceneScript sceneScript;

        #region Variables
        public static bool DebugMode = true; // GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true";
        public List<ScenarioList> _scenarios;
        #endregion

        
        public class ScenarioList {
            public int id { get; set; }
            public string name { get; set; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainScript()
        {
            _scenarios = new List<ScenarioList>() { };
            RegisterEventMethods();
            RegisterCommands();
        }

        private async void RegisterEventMethods() 
        {
        
            await Delay(3000);

            GetAllScenes();
            await Delay(1500);
            new MainMenu(this).OpenMenu();
        }

        private void RegisterCommands()
        {
        }

        public void SelectScene(int sceneId)
        {
            sceneScript.InitializeSceneFromId( sceneId );
        }
        public void RequestDeleteScene( int sceneId ) 
        {

            BaseScript.TriggerServerEvent("scenarioCreator:requestDeleteScene", sceneId);
        }
        public async void RequestCreateNewScene( ) 
        {
            var result = await CommonFunctions.GetUserInput(windowTitle: "Enter scene Name");
            // If the result was not invalid.
            if (!string.IsNullOrEmpty(result))
            {
                TriggerServerEvent("scenarioCreator:createScene", result);
            }
            // Result was invalid.
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
                return;
            }
        }

        public void GetAllScenes()
        {
            Func<string, string> CallbackFunction = (data) =>
            {
                var scenarios = JsonConvert.DeserializeObject<List<ScenarioList>>(data);

                foreach (var scenario in scenarios)
                {
                    _scenarios.Add(scenario);
                }

                return "";
            };
            BaseScript.TriggerServerEvent("scenarioCreator:getAllScenes", 1, CallbackFunction);
        }
    }
}
