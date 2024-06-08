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
        private readonly MainScript _script;
        #region Variables

        #endregion


        internal MainMenu(MainScript script, string name = Globals.ScriptName, string subtitle = "Select a scene") : base(name, subtitle)
        {
            _script = script;

            this.InstructionalButtons.Remove(Control.FrontendCancel);

            this.InstructionalButtons.Add(Control.SaveReplayClip, "Delete");
            this.InstructionalButtons.Add(Control.ReplayStartStopRecordingSecondary, "Create New");

            Update();
        }

        internal void Update()
        {
            if ( _script._scenarios.Count > 0 )
            {
                int i = 1;
                foreach (var s in _script._scenarios)
                {
                    var item = new MenuItem(s.name);
                    
                    item.ItemData = s.id;
                    this.AddMenuItem(item);
                    i++;
                }
            }
            else
            {
                var item = new MenuItem("Theres no scene created");
                item.Enabled = false;
                this.AddMenuItem(item);
            }

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {

            };

            int currentMenuItemSceneId = 0;

            this.ButtonPressHandlers.Add(
                new Menu.ButtonPressHandler(
                    Control.ReplayStartStopRecordingSecondary,
                    Menu.ControlPressCheckType.JUST_PRESSED,
                    new Action<Menu, Control>((m, c) =>
                    {
                        _script.RequestCreateNewScene(  );
                        // create new
                    }), true
                )
            );

            this.ButtonPressHandlers.Add(
                new Menu.ButtonPressHandler(
                    Control.SaveReplayClip,
                    Menu.ControlPressCheckType.JUST_PRESSED,
                    new Action<Menu, Control>((m, c) =>
                    {
                        if ( currentMenuItemSceneId > 0 )
                            _script.RequestDeleteScene( currentMenuItemSceneId );
                        // delete
                    }), true
                )
            );

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                currentMenuItemSceneId = newItem.ItemData;
            };

            this.OnItemSelect += (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                currentMenuItemSceneId = menuItem.ItemData;

                m.Visible = false;
                m.CloseMenu();
                MenuController.CloseAllMenus();

                _script.SelectScene( currentMenuItemSceneId );
            };

            MenuController.AddMenu(this);
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            MenuController.MenuToggleKey = Control.ReplayStartStopRecording;
            this.Visible = false;
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
