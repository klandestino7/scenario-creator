using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient
{
    internal class MainMenu : Menu
    {
        private readonly MainScript _script;
        #region Variables

        #endregion

        internal MainMenu(MainScript script, string name = Globals.ScriptName, string subtitle = "Main Menu") : base(name, subtitle)
        {

        }
    }
}
