using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using ScenarioCreatorClient.Classes;

using ScenarioCreatorShared;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient
{
    public class SceneScript : BaseScript
    {
        private EntityListMenu _entityListMenu;
        private EntityMenu _entityMenu;
        private static SceneMenu _sceneMenu;
        public static Scene _currentScene = null;
        private static Scenario _currentSceneData = null; 
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
            EventHandlers["scenarioCreator:openMainSceneMenu"] += new Action(OpenMainSceneMenu);
        }

        private void OnClientResourceStop(string resourceName)
        {
            if ( resourceName == GetCurrentResourceName() )
            {
                Debug.WriteLine($" OnClientResourceStop :: {resourceName}"); 

                if ( _currentScene != null )
                {
                    _currentScene.BeforeDestroy();
                }
            }
        }

        public List<EntityBase> GetEntitiesScene()
        {
            return _currentScene.GetEntities();
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

            Debug.WriteLine($" OnEntitySpawnedOnScene :: ee {ent}");

            switch ( (eEntityTypeToClass)entityType ) 
            {
                case eEntityTypeToClass.EntityPed:
                    var ped = new EntityPed(
                        _currentScene.entitiesCount + 1,
                        _currentEntitySpawnModel,
                        newEntity.Position,
                        newEntity.Rotation,
                        1
                    );
                    _currentScene.AddPedToScene( ped, ent );
                break;

                case eEntityTypeToClass.EntityProp:
                    var prop = new EntityProp(
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

        public static async void InitializeSceneFromId( int sceneId )
        {
            Debug.WriteLine($" InitializeSceneFromId :: {sceneId}");
            
            if ( _currentScene != null ) 
            {
                _currentScene.BeforeDestroy();
                _currentScene = null;
            }

            SceneScript.GetSceneDataFromServer( sceneId );

        }

         private static void GetSceneDataFromServer(int sceneId)
        {
            
            Func<string, string, string, string, string> CallbackFunction = (scenario, peds, props, vehicles) =>
            {
                Scenario _scenario = JsonConvert.DeserializeObject<Scenario>(scenario);
                List<ScenarioPed> _peds = JsonConvert.DeserializeObject<List<ScenarioPed>>(peds);
                List<ScenarioProp> _props = JsonConvert.DeserializeObject<List<ScenarioProp>>(props);
                List<ScenarioVehicle> _vehicles = JsonConvert.DeserializeObject<List<ScenarioVehicle>>(vehicles);

                _currentSceneData = _scenario;

                Debug.WriteLine(" RODEI ESSA MERDA 1 :: ");

                _currentScene = new Scene(
                    _scenario.Id,
                    _scenario.Name,
                    _vehicles,
                    _peds,
                    _props
                );
                Debug.WriteLine($" RODEI ESSA MERDA 2 :: {_currentScene}");

                TriggerEvent("scenarioCreator:openMainSceneMenu");
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
                return;
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
            var result = await CommonFunctions.GetUserInput(windowTitle: "Enter Entity Name");
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
        public async Task<bool> HandleCloseScene( ) 
        {
            _currentScene.BeforeDestroy();
            _currentScene = null;

            TriggerEvent("scenarioCreator:openMainMenu");
            return true;
        }

        #endregion

        public void OpenEntityMenu( int entityId )
        {
            if (_entityMenu == null)
            {
                _entityMenu = new EntityMenu(this);
            }

            selectedEntity = entityId;
            
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

                    if ( entitySelected.localEntityId == null || !DoesEntityExist( entitySelected.localEntityId ))
                    {
                        Notify.Error(CommonErrors.InvalidEntity);
                        lastEntity = 0;
                        return;
                    }


                    selectedEntity = entitySelected.localEntityId;
                    SetEntityDrawOutlineColor( 20, 255, 20, 255 );

                    Debug.WriteLine(" ENTREI UMA VEZ :: ");

                    Entity locEnt = Entity.FromHandle( entitySelected.localEntityId ); 

                    EntityCreation.SetCurrentEntity( locEnt );
                    EntityCreation.SetHandleMoveStatus( true );
                }
            }

            await Task.FromResult(0);
        }

        #endregion
    }


}
