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

    internal class WeaponsMenu : Menu
    {
        #region Variables;
        private readonly SceneScript _script;
        List<SceneList> sceneList = new List<SceneList>() { };
        EntityPed _currentPed;
        string _currentWeaponSelected;
        List<string> _weapons;
        #endregion

        internal WeaponsMenu(SceneScript script, EntityPed currentPed, string name = Globals.ScriptName, string subtitle = "Weapons Menu") : base(name, subtitle)
        {
            _script = script;
            _currentPed = currentPed;

            Update();
        }

        internal void Update()
        {

            LoadJson();

            foreach (var weapon in _weapons)
            {
                var item = new MenuItem(weapon);
                
                item.Description = "Press enter to change";
                item.ItemData = weapon;
                this.AddMenuItem(item);
            }

            
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
                _currentWeaponSelected = newItem.ItemData;
            };

            // when the player chooses a model
            this.OnItemSelect += async (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                _currentWeaponSelected = menuItem.ItemData;

                _currentPed.SetWeapon( _currentWeaponSelected );

                var lEntity = _currentPed.GetLocalEntity();
                _currentPed.AddWeaponToPed( lEntity.Handle );
            };

            MenuController.AddSubmenu(_script.GetPedEditMenu(), this);
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
        }

        internal void LoadJson(string fileName = "data/weapons.json")
        {
            try
            {
                string strings = LoadResourceFile(GetCurrentResourceName(), fileName);
                _weapons = JsonConvert.DeserializeObject<List<string>>(strings);

                Debug.WriteLine($"{nameof(MainScript)}: Loaded config from {fileName}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(MainScript)}: Impossible to load {fileName}", e.Message);
                Debug.WriteLine(e.StackTrace);

                _weapons = new List<string>() { };
            }
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
