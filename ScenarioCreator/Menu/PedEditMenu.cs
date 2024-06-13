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

    public class PedEditMenu : Menu
    {
        enum eMenuItem {
            IsFreezed = 0,
            IsInvincible = 1,
            Scenario = 2,
            Anim = 3,
            AnimDict = 4,
            Flags = 5,
            Relationship = 6,
            Weapon = 7,
        }
        private readonly SceneScript _script;
        private readonly EntityPed _currentPed;
        #region Variables;

        List<SceneList> sceneList = new List<SceneList>() { };

        #endregion

        internal PedEditMenu(SceneScript script, EntityPed currentPed, string name = Globals.ScriptName, string subtitle = "Ped Edit Menu") : base(name, subtitle)
        {
            _script = script;
            _currentPed = currentPed;

            Update();
        }

        internal void Update()
        {
            this.ClearMenuItems();
            sceneList = new List<SceneList>() { }; 
            
            MenuCheckboxItem isFreezedCheckbox = new MenuCheckboxItem($"Is Freezed {_currentPed.IsFreezed}", "", _currentPed.IsFreezed);
            this.AddMenuItem(isFreezedCheckbox);

            MenuCheckboxItem isInvincibleCheckbox = new MenuCheckboxItem($"Is Invincible {_currentPed.IsInvincible}", "", _currentPed.IsInvincible);
            this.AddMenuItem(isInvincibleCheckbox);

            sceneList.Add(new SceneList(2, $"Scenario {_currentPed.Scenario}", _script.DefineScenarioToPed));
            sceneList.Add(new SceneList(3, $"Anim {_currentPed.Anim}", _script.OpenAnimationMenu));
            // sceneList.Add(new SceneList(4, $"Anim Dict {_currentPed.Dict}", _script.DefineAnimationDictToPed));
            sceneList.Add(new SceneList(5, $"Flags {_currentPed.Flags}", _script.DefineFlagsToPed));
            sceneList.Add(new SceneList(6, $"Relationship {_currentPed.Relationship}", _script.DefineRelationShip));
            sceneList.Add(new SceneList(7, $"Weapon {_currentPed.WeaponModel}", _script.OpenWeaponMenu));

            sceneList.Add(new SceneList(8, "Confirm Edit", _script.ConfirmEditsPed));
            
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

            this.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                if ( itemIndex == (int)eMenuItem.IsFreezed )
                {
                    _script.DefineFreezed();
                }
                if ( itemIndex == (int)eMenuItem.IsInvincible )
                {
                    _script.DefineInvincible();
                }
            };

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

                this.Update();
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
