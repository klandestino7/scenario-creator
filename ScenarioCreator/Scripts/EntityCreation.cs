using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient.Scripts
{
    internal class EntityCreation : BaseScript
    {
        #region Variables
        public static bool Active { get; private set; } = false;
        public static Entity CurrentEntity { get; private set; } = null;
        private int scaleform = 0;
        private readonly float rotateSpeed = 20f;
        #endregion


        public EntityCreation()
        {
            RegisterCommandsMethods();
        }

        public void RegisterCommandsMethods()
        {
            RegisterCommand("testEntity", new Action<int, List<object>>((source, args) =>
            {
                var entityModel = (string)args[0];
                SpawnEntity(entityModel, Game.PlayerPed.Position);
            }), false);

            RegisterCommand("endTest", new Action(() =>
            {
                FinishPlacement();
            }), false);
        }

        /// <summary>
        /// Method for spawning entity with EntitySpawner. After entity is spawned you will be able to change
        /// position of entity with your mouse.
        /// </summary>
        /// <param name="model">model of entity as string</param>
        /// <param name="coords">initial coords for the entity</param>
        /// <returns>true spawn was succesful</returns>
        public static void SpawnEntity(string model, Vector3 coords)
        {
            SpawnEntity((uint)GetHashKey(model), coords);
        }

        /// <summary>
        /// Method for spawning entity with EntitySpawner. After entity is spawned you will be able to change
        /// position of entity with your mouse.
        /// </summary>
        /// <param name="model">model of entity as hash</param>
        /// <param name="coords">initial coords for the entity</param>
        /// <returns>true spawn was succesful</returns>
        public static async void SpawnEntity(uint model, Vector3 coords, bool enableModificationPos = false)
        {
            if (!IsModelValid(model))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return;
            }

            if (CurrentEntity != null)
            {
                Notify.Error("One entity is currently being processed.");
                return;
            }

            int handle;
            
            await Utils.LoadEntityModel( model ); 

            if (IsModelAPed(model))
            {
                handle = CreatePed(4, model, coords.X, coords.Y, coords.Z, Game.PlayerPed.Heading, true, true);
            }
            else if (IsModelAVehicle(model))
            {
                handle = await CommonFunctions.SpawnVehicle(model, false, false, skipLoad: false, vehicleInfo: new CommonFunctions.VehicleInfo(), saveName: null, coords.X, coords.Y, coords.Z, Game.PlayerPed.Heading);
            }
            else
            {
                handle = CreateObject((int)model, coords.X, coords.Y, coords.Z, true, true, true);
            }

            CurrentEntity = Entity.FromHandle(handle);

            if (!CurrentEntity.Exists())
            {
                Notify.Error("Failed to create entity");
                return;
            }

            SetEntityAsMissionEntity(handle, true, true); // Set As mission to prevent despawning

            Active = enableModificationPos;
        }

        public static async void UpdateEntityPosition()
        {
            
        }

        public static void CancelEntityPlacement() 
        {
            Active = false;

            CurrentEntity.Delete();
            CurrentEntity = null;
        }

        /// <summary>
        /// Method used to confirm location of prop and finish placement
        /// </summary>
        public static async void FinishPlacement(bool duplicate = false)
        {
            var cachedEntity = CurrentEntity;
            if (duplicate)
            {
                var hash = CurrentEntity.Model.Hash;
                var position = CurrentEntity.Position;
                CurrentEntity = null;
                await Delay(1); // Mandatory
                SpawnEntity((uint)hash, position);
            }
            else
            {
                Active = false;
                CurrentEntity = null;
            }

            TriggerEvent("scenarioCreator:entitySpawnedOnScene", cachedEntity);
            cachedEntity = null;
        }

        #region InternalMethods

        /// <summary>
        /// Used internally for drawing of help text
        /// </summary>
        private void DrawButtons() // TODO: Right keys
        {
            BeginScaleformMovieMethod(scaleform, "CLEAR_ALL");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(0);
            PushScaleformMovieMethodParameterString("~INPUT_VEH_FLY_ROLL_LR~");
            PushScaleformMovieMethodParameterString("Rotate Object");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(1);
            PushScaleformMovieMethodParameterString("~INPUT_ATTACK~");
            PushScaleformMovieMethodParameterString("Place Entity");
            EndScaleformMovieMethod();
                        
            BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(2);
            PushScaleformMovieMethodParameterString("~INPUT_AIM~");
            PushScaleformMovieMethodParameterString("Cancelar");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(3);
            PushScaleformMovieMethodParameterString("~INPUT_SPRINT~");
            PushScaleformMovieMethodParameterString("~INPUT_ATTACK~");
            PushScaleformMovieMethodParameterString("Place Entity ++");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "DRAW_INSTRUCTIONAL_BUTTONS");
            ScaleformMovieMethodAddParamInt(0);
            EndScaleformMovieMethod();
    


            DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);
        }

        /// <summary>
        /// Main tick method for class
        /// </summary>
        [Tick]
        internal async Task MoveHandler()
        {
            if (Active)
            {
                scaleform = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
                while (!HasScaleformMovieLoaded(scaleform))
                {
                    await Delay(0);
                }

                DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 0, 0);
            }
            else
            {
                if (scaleform != 0)
                {
                    SetScaleformMovieAsNoLongerNeeded(ref scaleform); // Unload scaleform if there is no need to draw it
                    scaleform = 0;
                }
            }

            var headingOffset = 0f;

            while (Active)
            {
                if (CurrentEntity == null || !CurrentEntity.Exists())
                {
                    Active = false;
                    CurrentEntity = null;
                    break;
                }
                var handle = CurrentEntity.Handle;

                DrawButtons();

                FreezeEntityPosition(handle, true);
                SetEntityInvincible(handle, true);
                SetEntityCollision(handle, false, false);
                SetEntityAlpha(handle, (int)(255 * 0.4), 0);
                CurrentEntity.Heading = (GetGameplayCamRot(0).Z + headingOffset) % 360f;

                var newPosition = Utils.GetCoordsPlayerIsLookingAt();

                CurrentEntity.Position = newPosition;
                if (CurrentEntity.HeightAboveGround < 3.0f)
                {
                    if (CurrentEntity.Model.IsVehicle)
                    {
                        SetVehicleOnGroundProperly(CurrentEntity.Handle);
                    }
                    else
                    {
                        PlaceObjectOnGroundProperly(CurrentEntity.Handle);
                    }
                }

                // Controls
                if (Game.IsControlPressed(0, Control.VehicleFlyRollLeftOnly))
                {
                    headingOffset += rotateSpeed * Game.LastFrameTime;
                }
                else if (Game.IsControlPressed(0, Control.VehicleFlyRollRightOnly))
                {
                    headingOffset -= rotateSpeed * Game.LastFrameTime;
                } 
                else if (Game.IsControlJustPressed(0, Control.Attack) && !Game.IsControlPressed(0, Control.Sprint))
                {
                    FinishPlacement();
                }
                else if (Game.IsControlPressed(0, Control.Sprint) && Game.IsControlJustReleased(0, Control.Attack))
                {
                    FinishPlacement( true );
                }
                else if (Game.IsControlJustPressed(0, Control.Aim))
                {
                    CancelEntityPlacement();
                }

                await Delay(0);

                FreezeEntityPosition(handle, false);
                SetEntityInvincible(handle, false);
                SetEntityCollision(handle, true, true);
                ResetEntityAlpha(handle);
            }

            await Task.FromResult(0);
        }

        #endregion
    }
}
