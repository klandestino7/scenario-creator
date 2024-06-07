using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

using MenuAPI;

using Newtonsoft.Json;

using ScenarioCreatorShared;
using ScenarioCreatorClient.Scripts;
using CitizenFX.Core.Native;

namespace ScenarioCreatorClient
{

    internal class EntityListMenu : Menu
    {
        private readonly SceneScript _script;
        #region Variables;

        List<string> skins = new List<string>() {
            "player_zero",
            "a_f_m_fatcult_01",
            "a_m_m_trampbeac_01"
        };

        #endregion

        internal EntityListMenu(SceneScript script, string name = Globals.ScriptName, string subtitle = "Scene Selected") : base(name, subtitle)
        {
            _script = script;
            // this.InstructionalButtons.Remove(Control.FrontendCancel);
            // this.InstructionalButtons.Add(Control.FrontendX, "Variation");
            // this.InstructionalButtons.Add(Control.FrontendY, "Accessory");

            Update();
        }

        internal void Update()
        {
            
           int i = 1;
            foreach (var s in skins)
            {
                var item = new MenuItem(s ?? $"Character #{i}");
                
                item.ItemData = s;
                this.AddMenuItem(item);
                i++;
            }

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                _script.OpenMainSceneMenu();
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                
            };

            // when the player chooses a model
            this.OnItemSelect += (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                m.Visible = false;
                m.CloseMenu();

                _script.OpenEntityMenu();
            };

            MenuController.AddMenu(this);
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
        }

        internal bool HideMenu
        {
            get => MenuController.DontOpenAnyMenu;
            set
            {
                MenuController.DontOpenAnyMenu = value;
            }
        }
    }
}
