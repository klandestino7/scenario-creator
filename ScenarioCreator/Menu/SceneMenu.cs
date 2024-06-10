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

    public class SceneList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Func<Task<bool>> Handle;

        public SceneList(int id, string name, Func<Task<bool>> handle)
        {
            Id = id;
            Name = name;
            Handle = handle;
        }
    }

    public class SceneMenu : Menu
    {
        private readonly SceneScript _script;
        #region Variables;

        List<SceneList> sceneList = new List<SceneList>() { };

        #endregion

        internal SceneMenu(SceneScript script, string name = Globals.ScriptName, string subtitle = "Scene Selected") : base(name, subtitle)
        {
            _script = script;
            // this.InstructionalButtons.Remove(Control.FrontendCancel);
            // this.InstructionalButtons.Add(Control.FrontendX, "Variation");
            // this.InstructionalButtons.Add(Control.FrontendY, "Accessory");

            // checkedValue: when this is set to true, the checkbox will be 'checked' by default.

            sceneList.Add(new SceneList(1, "Add New Entity", _script.HandleAddNewEntity));
            sceneList.Add(new SceneList(2, "Entity List", _script.HandleEntityList));
            sceneList.Add(new SceneList(3, "Start Scene", _script.HandleStartScene));
            sceneList.Add(new SceneList(4, "Stop Scene", _script.HandleStopScene));
            sceneList.Add(new SceneList(5, "Restart Scene", _script.HandleRestartScene));
            sceneList.Add(new SceneList(6, "Close Scene", _script.HandleCloseScene));

            Update();
        }

        internal void Update()
        {
            
            MenuCheckboxItem menuCheckBox = new MenuCheckboxItem("Edit Mode Bool", "When its be enabled you can edit select entity just looking for.", _script.editModeEnabled);
            // Add a menu item to a menu:

            this.AddMenuItem(menuCheckBox);

            int i = 1;
            foreach (var scene in sceneList)
            {
                var item = new MenuItem(scene.Name);
                
                item.ItemData = scene.Handle;
                this.AddMenuItem(item);
                i++;
            }

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                
            };

            this.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                _script.HandleToggleEditMode(_checked); 
            };

            // when the player chooses a model
            this.OnItemSelect +=  async (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                var menuHandleResponse = await menuItem.ItemData();

                if ( menuHandleResponse ) {
                    m.Visible = false;
                    m.CloseMenu();
                }
            };

            MenuController.AddSubmenu(_script.GetMainMenu(), this);
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
        }

        
        private void CloseCurrentMenu() 
        {
            this.Visible = false;
            this.CloseMenu();
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
