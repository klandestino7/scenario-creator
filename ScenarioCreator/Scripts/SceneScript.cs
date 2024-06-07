using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using ScenarioCreatorClient.Classes;
using ScenarioCreatorClient.Scripts;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient
{
    public class SceneScript : BaseScript
    {
        private EntityListMenu _entityListMenu;
        private EntityMenu _entityMenu;
        private SceneMenu _sceneMenu;
        private Scene _currentScene;

        public bool editModeEnabled = false;
        public bool isSpawnEntityMode = false;

        #region Variables
        public static bool DebugMode = true; // GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true";
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneScript()
        {
            RegisterEventMethods();

        
            RegisterCommand("sceneMenu", new Action<int, List<object>>((source, args) =>
            {
                OpenMainSceneMenu();
            }), false);
        }

        private void RegisterEventMethods() 
        {
            // EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);
            EventHandlers["scenarioCreator:entitySpawnedOnScene"] += new Action<int>(OnEntitySpawnedOnScene);
        }

        private void OnClientResourceStop(string resourceName)
        {
            if ( _currentScene != null ) {
                _currentScene.BeforeDestroy();
            }
        }

        private void OnEntitySpawnedOnScene( int ent )
        {
            Entity newEntity = Entity.FromHandle( ent );

            var entityType = GetEntityType( newEntity.Handle );

            Debug.WriteLine(" OnEntitySpawnedOnScene :: ");

            switch ( (eEntityTypeToClass)entityType ) 
            {
                case eEntityTypeToClass.EntityPed:
                    var p = new EntityPed(
                        -1,
                        "player_zero",
                        newEntity.Position,
                        newEntity.Rotation,
                        1
                    );
                    _currentScene.AddPedToScene( p, newEntity.Handle);
                break;

                case eEntityTypeToClass.EntityProp:
                    var pp = new EntityProp(
                        -1,
                        "player_zero",
                        newEntity.Position,
                        newEntity.Rotation
                    );
                    _currentScene.AddPropToScene( pp, newEntity.Handle);
                break;

                case eEntityTypeToClass.EntityVehicle:
                    var vehicleplate = GetVehicleNumberPlateText( newEntity.Handle );
                    var vv = new EntityVehicle(
                        -1,
                        "cypher",
                        newEntity.Position,
                        newEntity.Rotation,
                        null,
                        vehicleplate
                    );

                    _currentScene.AddVehicleToScene( vv, newEntity.Handle );
                break;
            } 
        }

        public void OpenMainSceneMenu()
        {
            if ( _sceneMenu == null ) {
                _sceneMenu = new SceneMenu(this);
            }

            if ( _currentScene == null ) {
                _currentScene = new Scene(1, "Cena Principal", null, null, null);
            }

            _sceneMenu.OpenMenu();
        }

        public async Task<bool> HandleStartScene( ) 
        {
            return false;
        }
        public async Task<bool> HandleAddNewEntity( ) 
        {
            var result = await CommonFunctions.GetUserInput(windowTitle: "Enter Vehicle Name");
            // If the result was not invalid.
            if (!string.IsNullOrEmpty(result))
            {
                EntityCreation.SpawnEntity(result, Game.PlayerPed.Position);
                isSpawnEntityMode = true;
                return true;
            }
            // Result was invalid.
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
                return false;
            }

        }
        public async Task<bool> HandleEntityList( ) 
        {
            Debug.WriteLine(" HandleEntityList ");
            if (_entityListMenu == null) {
                _entityListMenu = new EntityListMenu(this);
            }

            _entityListMenu.OpenMenu();

            return true;
        }
        public async Task<bool> HandleToggleEditMode( bool result ) 
        {
            Debug.WriteLine($" HandleToggleEditMode {editModeEnabled} {result}");
            editModeEnabled = result;
            return false;
        }
        public async Task<bool> HandleStopScene( ) 
        {
            return false;
        }
        public async Task<bool> HandleRestartScene( ) 
        {
            return false;
        }
        public async Task<bool> HandleEditEntity( ) 
        {
            return false;
        }       
        public async Task<bool> HandleSaveScene( ) 
        {
            return false;
        }

        public void OpenEntityMenu()
        {
            if (_entityMenu == null) {
                _entityMenu = new EntityMenu(this);
            }
            
            _entityMenu.OpenMenu();
        }
    
        /// <summary>
        /// Main tick method for class
        /// </summary>

        int lastEntity;
        [Tick]
        internal async Task OnTickMethod()
        {

            if ( editModeEnabled ) 
            {
                var res = Utils.GetPlayerRayCastResult();
                
                if ( res.HitEntity != null && res.HitEntity.Handle != lastEntity ) {
                    SetEntityDrawOutline( res.HitEntity.Handle , true );
                    SetEntityDrawOutlineColor( 255, 20, 20, 255 );
                    SetEntityDrawOutlineShader( 0 );

                    SetEntityDrawOutline( lastEntity, false );
                    lastEntity = res.HitEntity.Handle;

                } else {
                    SetEntityDrawOutline( lastEntity, false );
                    lastEntity = 0;
                }
            }
            else {
                if (DoesEntityExist( lastEntity )) 
                    SetEntityDrawOutline( lastEntity, false );
                    lastEntity = 0;
            }


            await Task.FromResult(0);
        }
    }


}
