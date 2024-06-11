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
    public class SceneScript
    {
        #region Variables
        private MainScript _mainScript;
        public EntityPed _currentPedEditMode;
        public EntityProp _currentPropEditMode;
        public EntityVehicle _currentVehicleEditMode;
        private EntityListMenu _entityListMenu;
        private WorldPositionEditMenu _worldPositionEditMenu;
        private EntityMenu _entityMenu;
        private PedEditMenu _pedEditMenu;
        private VehicleEditMenu _vehicleEditMenu;
        private PropEditMenu _propEditMenu;
        private static SceneMenu _sceneMenu;
        public Scene _currentScene = null;
        private static Scenario _currentSceneData = null; 
        public bool editModeEnabled = false;
        public bool isSpawnEntityMode = false;
        private string _currentEntitySpawnModel;
        public static bool DebugMode = true; // GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true";
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneScript(MainScript mainScript)
        {
            _mainScript = mainScript;
            RegisterEventMethods();

            RegisterCommand("sceneMenu", new Action<int, List<object>>((source, args) =>
            {
                OpenMainSceneMenu();
            }), false);
        }

        private void RegisterEventMethods() 
        {
            RegisterKeyMapping("+start_scene", "Start the current scene", "KEYBOARD", CommonFunctions.GetSettingsString(Setting.smenu_start_scene));
            RegisterKeyMapping("+stop_scene", "Stop the current scene", "KEYBOARD", CommonFunctions.GetSettingsString(Setting.smenu_stop_scene));
            RegisterKeyMapping("+reset_scene", "Reset the current scene", "KEYBOARD", CommonFunctions.GetSettingsString(Setting.smenu_reset_scene));

            RegisterCommand("+start_scene", new Action<int, List<object>>((source, args) =>
            {
                if ( _currentScene == null )
                {
                    Notify.Error(CommonErrors.CurrentSceneInvalid);
                    return;
                }

                HandleStartScene();
            }), false);
            
            RegisterCommand("+stop_scene", new Action<int, List<object>>((source, args) =>
            {
                if ( _currentScene == null )
                {
                    Notify.Error(CommonErrors.CurrentSceneInvalid);
                    return;
                }

                HandleStopScene();
            }), false);

            RegisterCommand("+reset_scene", new Action<int, List<object>>((source, args) =>
            {
                if ( _currentScene == null )
                {
                    Notify.Error(CommonErrors.CurrentSceneInvalid);
                    return;
                }

                HandleRestartScene();
            }), false);
        }

        public MainMenu GetMainMenu()
        {
            return _mainScript._mainMenu;
        }

        public void OnClientResourceStop(string resourceName)
        {
            if ( resourceName == GetCurrentResourceName() )
            {
                // Debug.WriteLine($" OnClientResourceStop :: {resourceName}"); 

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

        public void OnUpdateEnitityPosition( int ent )
        {
            EntityBase entitySelected = _currentScene.GetEntityInstanceFromHandleId( ent );
            entitySelected.UpdateWorldOrientation();
        }

        public void OnEntitySpawnedOnScene( int ent )
        {
            Entity newEntity = Entity.FromHandle( ent );

            var entityType = GetEntityType( newEntity.Handle );

            // Debug.WriteLine($" OnEntitySpawnedOnScene :: ee {ent}");


            // Debug.WriteLine($" _currentScene :: {_currentScene}");

            switch ( (Globals.eEntityTypeToClass)entityType ) 
            {
                case Globals.eEntityTypeToClass.EntityPed:
                    var ped = new EntityPed(
                        _currentScene.entitiesCount + 1,
                        _currentEntitySpawnModel,
                        newEntity.Position,
                        newEntity.Rotation,
                        1
                    );
                    _currentScene.AddPedToScene( ped, ent );
                break;

                case Globals.eEntityTypeToClass.EntityProp:
                    var prop = new EntityProp(
                        _currentScene.entitiesCount + 1,
                        _currentEntitySpawnModel,
                        newEntity.Position,
                        newEntity.Rotation
                    );
                    _currentScene.AddPropToScene( prop, ent );
                break;

                case Globals.eEntityTypeToClass.EntityVehicle:
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

        public async void InitializeSceneFromId( int sceneId )
        {
            // Debug.WriteLine($" InitializeSceneFromId :: {sceneId}");
            
            if ( _currentScene != null ) 
            {
                _currentScene.BeforeDestroy();
                _currentScene = null;
            }

            GetSceneDataFromServer( sceneId );
        }

         private async void GetSceneDataFromServer(int sceneId)
        {
            var promise = new TaskCompletionSource<bool>();
            Func<string, string, string, string, string> CallbackFunction = (scenario, peds, props, vehicles) =>
            {
                Scenario _scenario = JsonConvert.DeserializeObject<Scenario>(scenario);
                List<ScenarioPed> _peds = JsonConvert.DeserializeObject<List<ScenarioPed>>(peds);
                List<ScenarioProp> _props = JsonConvert.DeserializeObject<List<ScenarioProp>>(props);
                List<ScenarioVehicle> _vehicles = JsonConvert.DeserializeObject<List<ScenarioVehicle>>(vehicles);

                _currentSceneData = _scenario;

                // Debug.WriteLine(" RODEI ESSA MERDA 1 :: ");

                ///// ADD HERE TO SET PLAYER POSITION WHEN IS 
                if ( true ) 
                {
                    if ( _scenario.DefaultPosition.Z != 0.0f)
                    {
                        Game.PlayerPed.Position = _scenario.DefaultPosition;
                    }
                }

                _currentScene = new Scene(
                    _scenario.Id,
                    _scenario.Name,
                    _vehicles,
                    _peds,
                    _props
                );
                // Debug.WriteLine($" RODEI ESSA MERDA 2 :: {_currentScene}");

                // TriggerEvent("scenarioCreator:openMainSceneMenu");
                promise.TrySetResult(true);
                return "";
            };
            BaseScript.TriggerServerEvent("scenarioCreator:getSceneDataFromDb", Game.Player.ServerId, sceneId, CallbackFunction);

            await promise.Task;

            OpenMainSceneMenu();
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

        public SceneMenu GetSceneMenu()
        {
            return _sceneMenu;
        }

        #region Menu Handles
        public async Task<bool> HandleStartScene( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Scene.Start");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            _currentScene.StartScene();
            return true;
        }
        public async Task<bool> HandleAddNewEntity( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Entity.Add");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

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
            // Debug.WriteLine($" HandleToggleEditMode {editModeEnabled} {result}");
            editModeEnabled = result;
            return false;
        }
        public async Task<bool> HandleStopScene( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Scene.Stop");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            _currentScene.StopScene();
            return false;
        }
        public async Task<bool> HandleRestartScene( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Scene.Restart");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }
            _currentScene.RestartScene();
            return false;
        }
        public async Task<bool> HandleDeleteEntity( ) 
        {
            // Debug.WriteLine($" HandleDeleteEntity :: {selectedEntity}");

            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Entity.Delete");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            if ( DoesEntityExist( selectedEntity ))
            {
                _currentScene.RemoveEntityFromScene( selectedEntity );
                return true;
            }

            return false;
        }       
        public async Task<bool> HandleEditEntity( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Entity.Edit");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            if ( DoesEntityExist( selectedEntity ) )
            {
                var entityType = GetEntityType( selectedEntity );

                // Debug.WriteLine($"entityType :: {entityType}");

                switch ( (Globals.eEntityTypeToClass)entityType ) 
                {
                    case Globals.eEntityTypeToClass.EntityPed:
                        OpenPedEditMenu();
                    break;

                    case Globals.eEntityTypeToClass.EntityProp:
                        OpenPropEditMenu();
                    break;

                    case Globals.eEntityTypeToClass.EntityVehicle:
                        OpenVehicleEditMenu( );
                    break;
                } 
                return true;
            }
            return false;
        }        
        public async Task<bool> HandleEditEntityPosition( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Entity.Edit");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            if ( DoesEntityExist( selectedEntity ) )
            {
                OpenMenuToEditWorldPosition();
                return true;
            }
            return false;
        }    

        public async Task<bool> HandleResetEntity( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Entity.Reset");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            if ( DoesEntityExist( selectedEntity ))
            {
                _currentScene.GetEntityInstanceFromHandleId( selectedEntity ).ResetEntity();
                return true;
            }
            return false;
        }
        public async Task<bool> HandleCloseScene( ) 
        {
            var res = await CommonFunctions.IsPlayerHasPermission("smenu.Scene.Close");

            if (!res)
            {
                Notify.Error(CommonErrors.NotAllowed);
                return false;
            }

            _currentScene.BeforeDestroy();
            _currentScene = null;

            _mainScript._mainMenu.OpenMenu();
            return true;
        }

        #endregion

        #region OpenMenu
        public void OpenEntityMenu( int entityId )
        {
            Debug.WriteLine($" OpenEntityMenu :: { entityId }");

            if (_entityMenu == null)
            {
                _entityMenu = new EntityMenu(this);
            }

            selectedEntity = entityId;
            
            _entityMenu.OpenMenu();
        }

        private void OpenPedEditMenu() 
        {
            Debug.WriteLine($" OpenPedEditMenu ");

            _currentPedEditMode = _currentScene.GetPedInstanceFromEntityHandle( selectedEntity );
            
            if ( _entityListMenu != null )
            {
                _entityListMenu.CloseMenu();
            }

            if (_pedEditMenu == null)
            {
                _pedEditMenu = new PedEditMenu(this, _currentPedEditMode);
            }

            Debug.WriteLine($" OpenPedEditMenu :: {_pedEditMenu} {_currentPedEditMode}");

            _pedEditMenu.OpenMenu();
        }
        private void OpenPropEditMenu() 
        {
            
            _currentPropEditMode = _currentScene.GetPropInstanceFromEntityHandle( selectedEntity );
            if ( _entityListMenu != null )
            {
                _entityListMenu.CloseMenu();
            }
            if (_propEditMenu == null)
            {
                _propEditMenu = new PropEditMenu(this, _currentPropEditMode);
            }

            _propEditMenu.OpenMenu();
        }
        private void OpenVehicleEditMenu( ) 
        {
            Debug.WriteLine(" OpenVehicleEditMenu ");
            if ( _entityListMenu != null )
            {
                _entityListMenu.CloseMenu();
            }

            
            _currentVehicleEditMode = _currentScene.GetVehicleInstanceFromEntityHandle( selectedEntity );
            Debug.WriteLine($"_currentVehicleEditMode :: {_currentVehicleEditMode}");

            if (_vehicleEditMenu == null)
            {
                _vehicleEditMenu = new VehicleEditMenu(this, _currentVehicleEditMode);
            }

            Debug.WriteLine($" OpenVehicleEditMenu :: {_vehicleEditMenu} {_currentVehicleEditMode}");

            _vehicleEditMenu.OpenMenu();
        }

        private void OpenMenuToEditWorldPosition()
        {
            var _currentEntityToChangePosition = _currentScene.GetEntityInstanceFromHandleId( selectedEntity );
            
            if ( _entityListMenu != null )
            {
                _entityListMenu.CloseMenu();
            }
            if (_worldPositionEditMenu == null)
            {
                _worldPositionEditMenu = new WorldPositionEditMenu(this, _currentEntityToChangePosition);
            }

            _worldPositionEditMenu.OpenMenu();
        }
        #endregion

        private async Task<string> InputUserRequest(string title) 
        {
            var result = await CommonFunctions.GetUserInput(windowTitle: "Enter scene Name");
             // If the result was not invalid.
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            return "";
        }
        

        #region Ped Edit Methods

        public async Task<bool> DefineScenarioToPed()
        {
            var scenario = await InputUserRequest("Add Ped Scenario");

            if (scenario != "") {
                _currentPedEditMode.AddScenario(scenario);
                return true;
            }

            return false;
        }
          public async Task<bool> DefineAnimationToPed()
        {
            var anim = await InputUserRequest("Add Anim name");
            if ( anim != "" ) {
                _currentPedEditMode.AddAnimName(anim);
                return true;
            }
            return false;
        }
        public async Task<bool> DefineAnimationDictToPed()
        {
            var animDict = await InputUserRequest("Add Anim Dict");
            if ( animDict != "" ) {
                _currentPedEditMode.AddAnimDict(animDict);
                return true;
            }
            return false;
        }
        public async Task<bool> DefineFlagsToPed()
        {
         var flag = Int32.Parse(await InputUserRequest("Define anim Flags (NUMBER)"));
          
            if ( flag >= 0 ) {
                _currentPedEditMode.AddFlag(flag);
                return true;
            }
            return false;
        }
        public async Task<bool> DefineWeapon()
        {
            var weaponName = await InputUserRequest("Set weapon");
            if ( weaponName != "" ) {
                _currentPedEditMode.SetWeapon(weaponName);
                return true;
            }
            return false;
        }
        public async Task<bool> DefineRelationShip()
        {
            var weaponName = await InputUserRequest("Set Relatioship Group (PLAYER OR ENEMY)");
            if ( weaponName != "" ) {
                _currentPedEditMode.SetWeapon(weaponName);
                return true;
            }
            return false;
        }
        public void DefineFreezed()
        {
            _currentPedEditMode.SetFreezed( !_currentPedEditMode.IsFreezed );
        }
        public void DefineInvincible()
        {
            _currentPedEditMode.SetInvincible( !_currentPedEditMode.IsInvincible );
        }

        public async Task<bool> ConfirmEditsPed() 
        {
            ScenarioPed _ped = new ScenarioPed(
                _currentPedEditMode.Id,
                _currentScene.Id,
                _currentPedEditMode.Model,
                _currentPedEditMode.Position,
                _currentPedEditMode.Rotation,
                _currentPedEditMode.OutfitVariation,
                _currentPedEditMode.IsFreezed,
                _currentPedEditMode.IsInvincible,
                _currentPedEditMode.Scenario,
                _currentPedEditMode.Anim,
                _currentPedEditMode.Dict,
                _currentPedEditMode.Flags,
                _currentPedEditMode.Relationship,
                _currentPedEditMode.WeaponModel
            );
            BaseScript.TriggerLatentServerEvent("scenarioCreator:updatePed", 1024, _currentPedEditMode.Id, JsonConvert.SerializeObject(_ped));
            Notify.Success("Ped updated");
            _pedEditMenu.Update();
            return true;
        }

        #region Vehicle Edit Methods
        public async Task<bool> DefinePedDriver()
        {
            var pedDriverId = await InputUserRequest("Set Ped Driver by Entity ID (NUMBER)");
            if ( pedDriverId != "" ) {
                _currentVehicleEditMode.SetPedDriver(Int32.Parse(pedDriverId));
                return true;
            }
            return false;
        }
        public async Task<bool> DefineDriveStyle()
        {
            var driverStyle = await InputUserRequest("Driver Style");
            if ( driverStyle != "" ) {
                _currentVehicleEditMode.SetDriveStyle(Int32.Parse(driverStyle));
                return true;
            }
            return false;
        }

        public async Task<bool> DefineToPosition()
        {
            var position = await InputUserRequest("Destination coords (0.0, 0.0, 0.0)");

            if ( position != "" ) {
                string[] values = position.Split(new[] { ", " }, StringSplitOptions.None);

                var toPosition = new Vector3();

                toPosition.X = float.Parse(values[0]);
                toPosition.Y = float.Parse(values[1]);
                toPosition.Z = float.Parse(values[2]);
        
                _currentVehicleEditMode.SetToPosition(toPosition);
                return true;
            }
            return false;
        }
        public async Task<bool> DefineMaxSpeed()
        {
            var maxSpeed = await InputUserRequest("Set Max Vehicle Speed");
            if ( maxSpeed != "" ) {
                _currentVehicleEditMode.SetMaxSpeed(Int32.Parse(maxSpeed));
                return true;
            }
            return false;
        }
        public async Task<bool> ConfirmEditsVehicle() 
        {
            ScenarioVehicle _vehicle = new ScenarioVehicle(
                _currentVehicleEditMode.Id,
                _currentScene.Id,
                _currentVehicleEditMode.Model,
                _currentVehicleEditMode.Position,
                _currentVehicleEditMode.Rotation,
                null,
                _currentVehicleEditMode.Plate,
                _currentVehicleEditMode.PedDriver,
                _currentVehicleEditMode.PedDriverMetadata
            );
            BaseScript.TriggerLatentServerEvent("scenarioCreator:updateVehicle", 1024, _currentVehicleEditMode.Id, JsonConvert.SerializeObject(_vehicle));
            Notify.Success("Ped updated");
            return true;
        }
        #endregion

        #endregion
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
                        SetEntityAlpha( res.HitEntity.Handle, 100, 0 );
                        ResetEntityAlpha( lastEntity );
                        lastEntity = res.HitEntity.Handle;
                    }
                } 
                else
                {
                    ResetEntityAlpha( lastEntity );
                    lastEntity = 0;
                }
            }
            else
            {
                if (DoesEntityExist( lastEntity )) 
                {
                    ResetEntityAlpha( lastEntity );
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
