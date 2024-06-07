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

        #region Variables
        public static bool DebugMode = true; // GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true";
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainScript()
        {
            RegisterEventMethods();
            RegisterCommands();
        }

        private void RegisterEventMethods() 
        {
            // EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        }

        private void RegisterCommands()
        {
            RegisterCommand("openMenu", new Action<int, List<object>>((source, args) =>
            {
                new MainMenu(this).OpenMenu();
            }), false);
        }
    }
}
