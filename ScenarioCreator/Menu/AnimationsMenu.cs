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
using System.IO;

namespace ScenarioCreatorClient
{
    class AnimationList 
    {
        public List<string> Dances { get; set; }
        public List<string> Emotes { get; set; }
        public List<string> PropEmotes { get; set; }
        public List<string> Expressions { get; set; }
        public List<string> Walks { get; set; }

        public AnimationList()
        {

        }
    }
    internal class AnimationsMenu : Menu
    {
        #region Variables;
        private readonly SceneScript _script;
        List<SceneList> sceneList = new List<SceneList>() { };
        EntityPed _currentPed;
        AnimationList _items;
        string _currentAnimNameSelected;

        #endregion

        internal AnimationsMenu(SceneScript script, EntityPed currentPed, string name = Globals.ScriptName, string subtitle = "Animation Select Menu Menu") : base(name, subtitle)
        {
            _script = script;
            _currentPed = currentPed;

            this.InstructionalButtons.Add(Control.PhoneRight, "Play Anim");
            Update();
        }

        
        internal void Update()
        {

            LoadJson();

            foreach (var anim in _items.Dances)
            {
                var item = new MenuItem(anim);
                
                item.Description = "Press enter to change";
                item.ItemData = anim;
                this.AddMenuItem(item);
            }

            foreach (var anim in _items.Emotes)
            {
                var item = new MenuItem(anim);
                
                item.Description = "Press enter to change";
                item.ItemData = anim;
                this.AddMenuItem(item);
            }

            foreach (var anim in _items.PropEmotes)
            {
                var item = new MenuItem(anim);
                
                item.Description = "Press enter to change";
                item.ItemData = anim;
                this.AddMenuItem(item);
            }

            this.ButtonPressHandlers.Add(
                new Menu.ButtonPressHandler(
                    Control.PhoneRight,
                    Menu.ControlPressCheckType.JUST_PRESSED,
                    new Action<Menu, Control>((m, c) =>
                    {
                        BaseScript.TriggerEvent("dpemotes:executeEmote", _currentAnimNameSelected, _currentPed.localEntityId);
                        // create new
                    }), true
                )
            );


            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                // _script.HandleEntityList();
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                _currentAnimNameSelected = newItem.ItemData;
            };

            // when the player chooses a model
            this.OnItemSelect += async (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                _currentAnimNameSelected = menuItem.ItemData;

                _currentPed.AddAnimName( _currentAnimNameSelected );
                _currentPed.AddAnimDict( null );

                ClearPedTasks( _currentPed.localEntityId );
            };

            MenuController.AddSubmenu(_script.GetPedEditMenu(), this);
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
        }

        internal void LoadJson(string fileName = "data/animations.json")
        {
            try
            {
                string strings = LoadResourceFile(GetCurrentResourceName(), fileName);
                _items = JsonConvert.DeserializeObject<AnimationList>(strings);

                Debug.WriteLine($"{nameof(MainScript)}: Loaded config from {fileName}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(MainScript)}: Impossible to load {fileName}", e.Message);
                Debug.WriteLine(e.StackTrace);

                _items = new AnimationList();
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
