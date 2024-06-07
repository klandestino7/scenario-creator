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

namespace ScenarioCreatorClient
{
    internal class MainMenu : Menu
    {
        //private readonly MainScript _script;
        #region Variables
        List<string> skins = new List<string>() {
            "player_zero",
            "a_f_m_fatcult_01",
            "a_m_m_trampbeac_01"
        };

        #endregion

        internal MainMenu(MainScript script, string name = Globals.ScriptName, string subtitle = "Main Menu") : base(name, subtitle)
        {
            this.InstructionalButtons.Remove(Control.FrontendCancel);
            this.InstructionalButtons.Add(Control.FrontendX, "Variation");
            this.InstructionalButtons.Add(Control.FrontendY, "Accessory");

            Update();
        }

        internal void Update()
        {
            
            int i = 1;
            foreach (var s in skins)
            {
                var item = new MenuItem(s ?? $"Character #{i}");
                    Debug.WriteLine($" nEW MODEL ::: {s}");
                
                item.ItemData = s;
                this.AddMenuItem(item);
                i++;
            }

            bool inSelection = true;

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                // if (inSelection)
                //     m.Visible = true;
            };

            // sets the requested model every time the player changes the selection
            int selectedPed = 0;
            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                if (newItem.ItemData is string skin)
                {
                    selectedPed = GetHashKey(skin);
    
                    var res = await Utils.LoadEntityModel( (uint)selectedPed );

                    if (!res ) {
                        Notify.Error(" Player MOdel Invalid ");
                        return;
                    }

                    Debug.WriteLine($" THIS A NEW MODEL ::: {selectedPed} - {skin}");
                    SetPlayerModel(PlayerId(), (uint)selectedPed);
                }
            };

            // when the secondary button is pressed, set a random component variation
            this.ButtonPressHandlers.Add(
                new Menu.ButtonPressHandler(
                    Control.FrontendX,
                    Menu.ControlPressCheckType.JUST_PRESSED,
                    new Action<Menu, Control>((m, c) =>
                    {
                        SetPedRandomComponentVariation(PlayerPedId(), false);
                    }), true
                )
            );

            // when the tertiary button is pressed, set a random prop
            this.ButtonPressHandlers.Add(
                new Menu.ButtonPressHandler(
                    Control.FrontendY,
                    Menu.ControlPressCheckType.JUST_PRESSED,
                    new Action<Menu, Control>((m, c) =>
                    {
                        SetPedRandomProps(PlayerPedId());
                    }), true
                )
            );

            // when the player chooses a model
            this.OnItemSelect += (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                inSelection = false;

                m.Visible = false;
                m.CloseMenu();
                MenuController.CloseAllMenus();
            };

            MenuController.AddMenu(this);
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            this.Visible = true;
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
