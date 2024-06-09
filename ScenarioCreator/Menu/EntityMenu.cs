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
using CitizenFX.Core.Native;

namespace ScenarioCreatorClient
{

    internal class EntityMenu : Menu
    {
        private readonly SceneScript _script;
        #region Variables;

        List<SceneList> sceneList = new List<SceneList>() { };

        #endregion

        internal EntityMenu(SceneScript script, string name = Globals.ScriptName, string subtitle = "Scene Selected") : base(name, subtitle)
        {
            _script = script;
            // this.InstructionalButtons.Remove(Control.FrontendCancel);
            // this.InstructionalButtons.Add(Control.FrontendX, "Variation");
            // this.InstructionalButtons.Add(Control.FrontendY, "Accessory");

            sceneList.Add(new SceneList(1, "Edit Entity", _script.HandleEditEntity));
            sceneList.Add(new SceneList(2, "Delete Entity", _script.HandleAddNewEntity));
            sceneList.Add(new SceneList(3, "Reset Entity", _script.HandleEntityList));
            sceneList.Add(new SceneList(4, "Rename Entity", _script.HandleStartScene));

            Update();
        }

        internal void Update()
        {
            
            int i = 1;
            foreach (var scene in sceneList)
            {
                var item = new MenuItem(scene.Name);
                
                item.ItemData = scene.Handle;
                this.AddMenuItem(item);
                i++;
            }

            bool inSelection = true;

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                _script.HandleEntityList();
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                
            };

            // when the player chooses a model
            this.OnItemSelect += (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                var menuHandleResponse = menuItem.ItemData();

                if (menuHandleResponse) {
                    inSelection = false;
                    m.Visible = false;
                    m.CloseMenu();
                }
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
