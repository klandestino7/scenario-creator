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

        private Dictionary<string, dynamic> _currentSceneData; 

        public bool editModeEnabled = false;
        public bool isSpawnEntityMode = false;
        private string _currentEntitySpawnModel;

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
            EventHandlers["onResourceStop"] += new Action<string>(OnClientResourceStop);
            EventHandlers["scenarioCreator:entitySpawnedOnScene"] += new Action<int>(OnEntitySpawnedOnScene);
            EventHandlers["scenarioCreator:updateEntityPosition"] += new Action<int>(OnUpdateEnitityPosition);
        }

        private void OnClientResourceStop(string resourceName)
        {
            Debug.WriteLine($" OnClientResourceStop :: {resourceName}"); 

            if ( _currentScene != null )
            {
                _currentScene.BeforeDestroy();
            }
        }

        private void OnUpdateEnitityPosition( int ent )
        {
            EntityBase entitySelected = _currentScene.GetEntityInstanceFromHandleId( ent );
            entitySelected.UpdateWorldOrientation();
        }

        private void OnEntitySpawnedOnScene( int ent )
        {
            Entity newEntity = Entity.FromHandle( ent );

            var entityType = GetEntityType( newEntity.Handle );

            Debug.WriteLine(" OnEntitySpawnedOnScene :: ");

            switch ( (eEntityTypeToClass)entityType ) 
            {
                case eEntityTypeToClass.EntityPed:
                    var ped = new EntityPed(
                        false,
                        _currentScene.entitiesCount + 1,
                        _currentEntitySpawnModel,
                        newEntity.Position,
                        newEntity.Rotation,
                        1
                    );
                    _currentScene.AddPedToScene( ped, ent);
                break;

                case eEntityTypeToClass.EntityProp:
                    var prop = new EntityProp(
                        false,
                        _currentScene.entitiesCount + 1,
                        _currentEntitySpawnModel,
                        newEntity.Position,
                        newEntity.Rotation
                    );
                    _currentScene.AddPropToScene( prop, ent);
                break;

                case eEntityTypeToClass.EntityVehicle:
                    var vehicleplate = GetVehicleNumberPlateText( ent );
                    var veh = new EntityVehicle(
                        true,
                        _currentScene.entitiesCount + 1,
                        _currentEntitySpawnModel,
                        newEntity.Position,
                        newEntity.Rotation,
                        null,
                        vehicleplate
                    );

                    _currentScene.AddVehicleToScene( veh, ent );
                break;
            } 
        }

        public void InitializeSceneFromId( int sceneId )
        {
            GetSceneDataFromServer( sceneId );

            if ( _currentScene != null ) 
            {
                _currentScene.BeforeDestroy();
                _currentScene = null;
            }

            _currentScene = new Scene(
                _currentSceneData["id"],
                _currentSceneData["name"],
                _currentSceneData["vehicles"],
                _currentSceneData["peds"],
                _currentSceneData["props"]
            );
        }

         private void GetSceneDataFromServer(int sceneId)
        {
            Func<string, string> CallbackFunction = (data) =>
            {
                _currentSceneData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
                return "";
            };
            BaseScript.TriggerServerEvent("scenarioCreator:getSceneDataFromDb", 1, sceneId, CallbackFunction);
        }

        public void OpenMainSceneMenu()
        {
            if ( _sceneMenu == null )
            {
                _sceneMenu = new SceneMenu(this);
            }

            if ( _currentScene == null )
            {
                _currentScene = new Scene(1, "Cena Principal", null, null, null);
            }

            _sceneMenu.OpenMenu();
        }

        #region Menu Handles
        public async Task<bool> HandleStartScene( ) 
        {
            _currentScene.StartScene();
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
                _currentEntitySpawnModel = result;
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
            if (_entityListMenu == null)
            {
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
            _currentScene.StopScene();
            return false;
        }
        public async Task<bool> HandleRestartScene( ) 
        {
            _currentScene.StopScene();
            _currentScene.StartScene();
            return false;
        }
        public async Task<bool> HandleEditEntity( ) 
        {
            return false;
        }       
        public async Task<bool> HandleSaveScene( ) 
        {
            _currentScene.ForceSaveScene();
            return false;
        }

        #endregion

        public void OpenEntityMenu()
        {
            if (_entityMenu == null)
            {
                _entityMenu = new EntityMenu(this);
            }
            
            _entityMenu.OpenMenu();
        }
    
        /// <summary>
        /// Main tick method for class
        /// </summary>

        #region  Ticks
        int lastEntity;
        int selectedEntity;
        [Tick]
        internal async Task OnTickSelectEntity()
        {
            if ( editModeEnabled ) 
            {
                var res = Utils.GetPlayerRayCastResult();
                
                if ( res.HitEntity != null )
                {
                    if ( res.HitEntity.Handle != lastEntity )
                    {
                        SetEntityDrawOutline( res.HitEntity.Handle , true );
                        SetEntityDrawOutlineColor( 255, 20, 20, 255 );
                        SetEntityDrawOutlineShader( 0 );

                        SetEntityDrawOutline( lastEntity, false );
                        lastEntity = res.HitEntity.Handle;
                    }
                } 
                else
                {
                    SetEntityDrawOutline( lastEntity, false );
                    lastEntity = 0;
                }
            }
            else
            {
                if (DoesEntityExist( lastEntity )) 
                {
                    SetEntityDrawOutline( lastEntity, false );
                    lastEntity = 0;
                }
            }
            await Task.FromResult(0);
        }

          [Tick]
        internal async Task OnTickEditEntity()
        {
            if ( editModeEnabled && lastEntity != 0 && DoesEntityExist(lastEntity) ) 
            {
                if ( Game.IsControlJustPressed(0, Control.FrontendAccept) )
                {
                    EntityBase entitySelected = _currentScene.GetEntityInstanceFromHandleId( lastEntity );

                    if ( entitySelected == null || entitySelected.Id <= 0 )
                    {
                        Notify.Error(CommonErrors.InvalidEntity);
                        lastEntity = 0;
                        return;
                    }

                    if ( entitySelected.localEntity == null || !DoesEntityExist( entitySelected.localEntity.Handle ))
                    {
                        Notify.Error(CommonErrors.InvalidEntity);
                        lastEntity = 0;
                        return;
                    }


                    selectedEntity = entitySelected.localEntity.Handle;
                    SetEntityDrawOutlineColor( 20, 255, 20, 255 );

                    Debug.WriteLine(" ENTREI UMA VEZ :: ");

                    EntityCreation.SetCurrentEntity( entitySelected.localEntity );
                    EntityCreation.SetHandleMoveStatus( true );
                }
            }

            await Task.FromResult(0);
        }

        #endregion
    }


}
