
using System.Collections.Generic;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using MenuAPI;

using ScenarioCreatorShared;
using ScenarioCreatorClient.Classes;
using System;

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

            Update();
        }

        internal void Update()
        {
            bool nextMenu = false;

            Func<string> DrawMenuListAgain = () =>
            {
                int i = 1;
                this.ClearMenuItems();
                _entities = _script.GetEntitiesScene();
                
                foreach (var s in _entities)
                {
                    // Debug.WriteLine($" ESSE ENT :: {s.Model}");
                    var item = new MenuItem($"[{s.Id}] {s.Model}");
                    
                    item.ItemData = s.localEntityId;
                    this.AddMenuItem(item);
                    i++;
                }
                
                return "";
            };

            DrawMenuListAgain();
            
            int lastEntity = 0;

            this.OnMenuOpen += ( Menu m ) =>
            {
                nextMenu = false;
                DrawMenuListAgain();
            };

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                if ( !nextMenu ) 
                {
                    _script.OpenMainSceneMenu();
                }

                if ( lastEntity != 0 && DoesEntityExist( lastEntity )) 
                {
                    ResetEntityAlpha( lastEntity );
                }
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                int newEntity = newItem.ItemData;
                
                if ( lastEntity != 0 && DoesEntityExist( lastEntity )) 
                {
                    ResetEntityAlpha( lastEntity );
                }
            
                if (newEntity != null && DoesEntityExist( newEntity ) )
                {
                    SetEntityAlpha( newEntity, 100, 0 );
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

            MenuController.AddSubmenu(_script.GetMainMenu(), this);
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
