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
using ScenarioCreatorClient.Classes;

namespace ScenarioCreatorClient
{

    internal class EntityListMenu : Menu
    {
        private readonly SceneScript _script;
        #region Variables;

        List<EntityBase> _entities;

        #endregion

        internal EntityListMenu(SceneScript script, string name = Globals.ScriptName, string subtitle = "Scene Selected") : base(name, subtitle)
        {
            _script = script;
            // this.InstructionalButtons.Remove(Control.FrontendCancel);
            // this.InstructionalButtons.Add(Control.FrontendX, "Variation");
            // this.InstructionalButtons.Add(Control.FrontendY, "Accessory");

            _entities = _script.GetEntitiesScene();

            Update();
        }

        internal void Update()
        {
            bool nextMenu = false;
            int i = 1;
            foreach (var s in _entities)
            {
                Debug.WriteLine($" ESSE ENT :: {s.Model}");
                var item = new MenuItem(s.Model ?? $"Character #{i}");
                
                item.ItemData = s.localEntityId;
                this.AddMenuItem(item);
                i++;
            }

            int lastEntity = 0;

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                if ( !nextMenu ) 
                {
                    _script.OpenMainSceneMenu();
                }

                if ( lastEntity != 0 && DoesEntityExist( lastEntity )) 
                {
                    SetEntityDrawOutline( lastEntity , false );
                }
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                int newEntity = newItem.ItemData;
                
                if ( lastEntity != 0 && DoesEntityExist( lastEntity )) 
                {
                    SetEntityDrawOutline( lastEntity , false );
                }
            
                if (newEntity != null && DoesEntityExist( newEntity ) )
                {
                    SetEntityDrawOutline( newEntity , true );
                    SetEntityDrawOutlineColor( 20, 255, 20, 255 );
                    SetEntityDrawOutlineShader( 0 );
                }

                lastEntity = newEntity;
            };

            // when the player chooses a model
            this.OnItemSelect += (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                m.Visible = false;
                nextMenu = true;
                m.CloseMenu();

                _script.OpenEntityMenu( menuItem.ItemData );
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
