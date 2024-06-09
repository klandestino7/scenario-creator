using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient
{
    public class MainScript : BaseScript
    {
        public int CurrentSceneSelectedId;

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
            
            OpenMainMenu();
            RegisterEventMethods();
            RegisterCommands();
        }

        private async void RegisterEventMethods() 
        {
            EventHandlers["scenarioCreator:openMainMenu"] += new Action(OpenMainMenu);
        }

        public async void OpenMainMenu()
        {
            GetAllScenes();
        }

        private void RegisterCommands()
        {
        }

        public void SelectScene(int sceneId)
        {
           SceneScript.InitializeSceneFromId( sceneId );
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
                Func<int, string> CallbackFunction = (res) =>
                {
                    if ( res != null ) 
                    {
                        SceneScript.InitializeSceneFromId( res );
                    }
                    return "";
                };
                TriggerServerEvent("scenarioCreator:createScene", result, CallbackFunction);
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
                    Debug.WriteLine($" SCENE :: {scenario.id}");
                    _scenarios.Add(scenario);
                }

                new MainMenu(this).OpenMenu();
                return "";
            };
            BaseScript.TriggerServerEvent("scenarioCreator:getAllScenes", 1, CallbackFunction);
        }
    }
}
