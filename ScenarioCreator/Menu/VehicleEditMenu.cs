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
using ScenarioCreatorClient.Classes;

namespace ScenarioCreatorClient
{

    internal class VehicleEditMenu : Menu
    {

        private readonly SceneScript _script;
        #region Variables;

        List<SceneList> sceneList = new List<SceneList>() { };
        EntityVehicle _currentVehicle;

        #endregion

        internal VehicleEditMenu(SceneScript script, EntityVehicle currentVehicle, string name = Globals.ScriptName, string subtitle = "Vehicle Edit Menu") : base(name, subtitle)
        {
            _script = script;
            _currentVehicle = currentVehicle;

            Update();
        }

        internal void Update()
        {
            this.ClearMenuItems();
            sceneList = new List<SceneList>() { }; 
            
            sceneList.Add(new SceneList(1, $"Ped driver {_currentVehicle.PedDriver}", _script.DefinePedDriver));
            sceneList.Add(new SceneList(2, $"Drive Style {_currentVehicle.PedDriverMetadata.DriverStyle}", _script.DefineDriveStyle));
            sceneList.Add(new SceneList(3, $"Max Speed {_currentVehicle.PedDriverMetadata.MaxSpeed}", _script.DefineMaxSpeed));
            sceneList.Add(new SceneList(4, $"To Position {_currentVehicle.PedDriverMetadata.ToPosition}", _script.DefineToPosition));
            
            sceneList.Add(new SceneList(5, "Confirm Edit", _script.ConfirmEditsVehicle));
            
            int i = 1;
            foreach (var scene in sceneList)
            {
                var item = new MenuItem(scene.Name);
                
                item.Description = "Press enter to change";
                item.ItemData = scene.Handle;
                this.AddMenuItem(item);
                i++;
            }

            bool inSelection = true;

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                // _script.HandleEntityList();
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                
            };

            // when the player chooses a model
            this.OnItemSelect += async (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                var menuHandleResponse = await menuItem.ItemData();
            };

            MenuController.AddSubmenu(_script.GetSceneMenu(), this);
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
