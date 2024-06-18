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
    internal enum ePedOrientation {
        Factor = 0,
        PositionX,
        PositionY,
        PositionZ,
        RotationX,
        RotationY,
        RotationZ
    }
    internal class SlideChangerItem 
    {
        public string title {get;}
        public string description {get;}
        public string currentValue {get; set;}
        public ePedOrientation orientation;

        public SlideChangerItem(string _title, string _description, string _currentValue, ePedOrientation _orientation)
        {
            title = _title;
            description = _description;
            currentValue = _currentValue;
            orientation = _orientation;
        }
    }

    internal class WorldPositionEditMenu : Menu
    {
        #region Variables;
        private readonly SceneScript _script;
        private EntityBase _currentEntity;
        List<SlideChangerItem> menuList = new List<SlideChangerItem>() { };
        #endregion

        internal WorldPositionEditMenu(SceneScript script, EntityBase currentEntity, string name = Globals.ScriptName, string subtitle = "World Position Edit Menu") : base(name, subtitle)
        {
            _script = script;
            _currentEntity = currentEntity;

            menuList.Add(new SlideChangerItem("Factor",  "", "0.1", ePedOrientation.Factor));

            menuList.Add(new SlideChangerItem("Position X",  "", _currentEntity.Position.X.ToString(), ePedOrientation.PositionX));
            menuList.Add(new SlideChangerItem("Position Y",  "", _currentEntity.Position.Y.ToString(), ePedOrientation.PositionY));
            menuList.Add(new SlideChangerItem("Position Z",  "", _currentEntity.Position.Z.ToString(), ePedOrientation.PositionZ));
            
            menuList.Add(new SlideChangerItem("Rotation X",  "", _currentEntity.Rotation.X.ToString(), ePedOrientation.RotationX));
            menuList.Add(new SlideChangerItem("Rotation Y",  "", _currentEntity.Rotation.Y.ToString(), ePedOrientation.RotationY));
            menuList.Add(new SlideChangerItem("Rotation Z",  "", _currentEntity.Rotation.Z.ToString(), ePedOrientation.RotationZ));

            Update();
        }


        private void UpdateEntityPositionFromMenu( float newValue, ePedOrientation orientation )
        {
            Debug.WriteLine($" orientation : {orientation} {ePedOrientation.PositionX}"); 
            switch( orientation )
            {
                case ePedOrientation.PositionX:
                    _currentEntity.Position = new Vector3( newValue, _currentEntity.Position.Y, _currentEntity.Position.Z );
                break;
                case ePedOrientation.PositionY:
                    _currentEntity.Position = new Vector3( _currentEntity.Position.X, newValue, _currentEntity.Position.Z );
                break;
                case ePedOrientation.PositionZ:
                    _currentEntity.Position = new Vector3( _currentEntity.Position.X, _currentEntity.Position.Y, newValue );
                break;
                case ePedOrientation.RotationX:
                    _currentEntity.Rotation = new Vector3( newValue, _currentEntity.Rotation.Y, _currentEntity.Rotation.Z );
                break;
                case ePedOrientation.RotationY:
                    _currentEntity.Rotation = new Vector3( _currentEntity.Rotation.X, newValue, _currentEntity.Rotation.Z );
                break;
                case ePedOrientation.RotationZ:
                    _currentEntity.Rotation = new Vector3( _currentEntity.Rotation.X, _currentEntity.Rotation.Y, newValue );
                break;
            }

            _currentEntity.ResetEntity();
        }

        internal void Update()
        {
            int currentMenuIndex = 0;

            string ChangeCallback(MenuDynamicListItem item, bool left)
            {
                var factorValue = menuList[(int)ePedOrientation.Factor];
                var menuListItem = menuList[currentMenuIndex];

                var isChangeFactor = currentMenuIndex == 0;

                if ( isChangeFactor ) 
                {
                    if (left)
                    {
                        factorValue.currentValue = (float.Parse(item.CurrentItem) - 0.1).ToString();
                        return factorValue.currentValue;
                    }
                    else 
                    {
                        factorValue.currentValue = (float.Parse(item.CurrentItem) + 0.1).ToString();
                        return factorValue.currentValue;
                    }
                    
                }

                float factor = float.Parse(factorValue.currentValue);
                
                // Left will be true when the left arrow key was pressed
                // and false if the right arrow key was pressed.

                float newValue = float.Parse(item.CurrentItem);

                if ( menuListItem != null && !isChangeFactor)
                {
                    if (left) {
                        newValue -= factor;
                    } else {
                        newValue += factor;
                    }
                }

                UpdateEntityPositionFromMenu( newValue, (ePedOrientation)currentMenuIndex );
                return newValue.ToString();
            }


            // Use the ChangeCallback function from above to create a new callback delegate.
            MenuDynamicListItem.ChangeItemCallback callback = new MenuDynamicListItem.ChangeItemCallback(ChangeCallback);

            foreach ( var item in menuList) {
                var _menuItem = new MenuDynamicListItem(item.title, item.currentValue, callback, item.description);
                this.AddMenuItem(_menuItem);
            }

            // prevents player closing the menu
            this.OnMenuClose += (Menu m) =>
            {
                // _script.HandleEntityList();

                if ( _currentEntity != null ){
                    _currentEntity.SaveWorldPositionOnDB();
                }
            };

            this.OnIndexChange += async (Menu m, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex) =>
            {
                Debug.WriteLine($" CurrentMenu Index :: {newIndex}");
                currentMenuIndex = newIndex;
            };

            // when the player chooses a model
            this.OnItemSelect += async (Menu m, MenuItem menuItem, int itemIndex) =>
            {
                // sets selectModel to false, to allow exiting the method
                // var menuHandleResponse = await menuItem.ItemData();
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
