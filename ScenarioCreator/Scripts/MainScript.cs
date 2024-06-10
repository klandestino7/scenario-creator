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
        #region Variables
        public SceneScript _sceneScript;
        public static string MenuToggleKey { get; private set; } = "M"; // M by default
        public int CurrentSceneSelectedId;
        public MainMenu _mainMenu;
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
            RegisterCommands();

            if (!(CommonFunctions.GetSettingsString(Setting.smenu_menu_toggle_key) == null))
            {
                MenuToggleKey = CommonFunctions.GetSettingsString(Setting.smenu_menu_toggle_key);
            }

            _sceneScript = new SceneScript(this);
            
            RegisterEventMethods();
        
            _mainMenu = new MainMenu(this);
            MenuController.AddMenu(_mainMenu);
            MenuController.MainMenu = _mainMenu;
            MenuController.MenuToggleKey = (Control)Int32.Parse(MenuToggleKey);
        }


        public void OpenMainSceneMenu()
        {
            _sceneScript.OpenMainSceneMenu();
        }
        private void RegisterEventMethods() 
        {
            EventHandlers["scenarioCreator:openMainMenu"] += new Action(OpenMainMenu);

            EventHandlers["onClientResourceStop"] += new Action<string>(_sceneScript.OnClientResourceStop);
            EventHandlers["onResourceStop"] += new Action<string>(_sceneScript.OnClientResourceStop);
            EventHandlers["scenarioCreator:entitySpawnedOnScene"] += new Action<int>(_sceneScript.OnEntitySpawnedOnScene);
            EventHandlers["scenarioCreator:updateEntityPosition"] += new Action<int>(_sceneScript.OnUpdateEnitityPosition);
            EventHandlers["scenarioCreator:openMainSceneMenu"] += new Action(_sceneScript.OpenMainSceneMenu);
        }

        public async void OpenMainMenu()
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.OpenMenu");
            // Debug.WriteLine($" IsPlayerHasPermission :: {res}");

            if (!res)
            {
                MenuController.MainMenu = null;
                MenuController.DisableMenuButtons = true;
                MenuController.DontOpenAnyMenu = true;
                return;
            }

            GetAllScenes();

        }

        private void RegisterCommands()
        {
        }

        public void SelectScene(int sceneId)
        {
           _sceneScript.InitializeSceneFromId( sceneId );
        }
        public async void RequestDeleteScene( int sceneId ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Scene.Delete");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return;
            }

            Func<bool, bool> CallbackFunction = (res) =>
            {
                if ( res ) 
                {
                    GetAllScenes();
                }
                return true;
            };

            BaseScript.TriggerServerEvent("scenarioCreator:requestDeleteScene", sceneId, CallbackFunction);
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
                        _mainMenu.CloseMenu();
                        _sceneScript.InitializeSceneFromId( res );
                    }
                    return "";
                };

                Vector3 defaultPosition = Game.PlayerPed.Position;

                TriggerServerEvent("scenarioCreator:createScene", result, JsonConvert.SerializeObject(defaultPosition), CallbackFunction);
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
            _scenarios = new List<ScenarioList>() { };

            Func<string, bool> CallbackFunction = (data) =>
            {
                var scenarios = JsonConvert.DeserializeObject<List<ScenarioList>>(data);

                _mainMenu.ClearMenuItems();

                if ( scenarios.Count <= 0 )
                {
                    var item = new MenuItem("Theres no scene created");
                    item.Enabled = false;
                    _mainMenu.AddMenuItem(item);
                    return true;
                }

                foreach (var scenario in scenarios)
                {
                    // Debug.WriteLine($" SCENE :: {scenario.id}");
                    var item = new MenuItem(scenario.name);
                
                    item.ItemData = scenario.id;
                    _mainMenu.AddMenuItem(item);
                }

                return true;
            };
            BaseScript.TriggerServerEvent("scenarioCreator:getAllScenes", 1, CallbackFunction);
        }
    }
}
