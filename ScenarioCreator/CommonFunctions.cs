using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;
using static ScenarioCreatorShared.PermissionsManager;

namespace ScenarioCreatorClient
{
    public static class CommonFunctions
    {
        #region Variables
        private static string _currentScenario = "";
        private static Vehicle _previousVehicle;

        internal static bool DriveToWpTaskActive = false;
        internal static bool DriveWanderTaskActive = false;
        #endregion

        #region some misc functions copied from base script
        /// <summary>
        /// Copy of <see cref="BaseScript.TriggerServerEvent(string, object[])"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public static void TriggerServerEvent(string eventName, params object[] args)
        {
            BaseScript.TriggerServerEvent(eventName, args);
        }

        /// <summary>
        /// Copy of <see cref="BaseScript.TriggerEvent(string, object[])"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public static void TriggerEvent(string eventName, params object[] args)
        {
            BaseScript.TriggerEvent(eventName, args);
        }

        /// <summary>
        /// Copy of <see cref="BaseScript.Delay(int)"/>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }
        #endregion

        #region menu position
        public static bool RightAlignMenus() => UserDefaults.MiscRightAlignMenu;
        #endregion

        #region DoesModelExist
        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelName">The model name</param>
        /// <returns></returns>
        public static bool DoesModelExist(string modelName) => DoesModelExist((uint)GetHashKey(modelName));

        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelHash">The model hash</param>
        /// <returns></returns>
        public static bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
        #endregion

        #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
        /// <summary>
        /// Returns the current or last vehicle of the current player.
        /// </summary>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the current or last vehicle of the selected ped.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return ped.LastVehicle;
            }
            else
            {
                if (ped.IsInVehicle())
                {
                    return ped.CurrentVehicle;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the current or last vehicle of the selected player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
        }
        #endregion

        #region GetVehicleModel (uint)(hash) from Entity/Vehicle (int)
        /// <summary>
        /// Get the vehicle model hash (as uint) from the specified (int) entity/vehicle.
        /// </summary>
        /// <param name="vehicle">Entity/vehicle.</param>
        /// <returns>Returns the (uint) model hash from a (vehicle) entity.</returns>
        public static uint GetVehicleModel(int vehicle) => (uint)GetHashKey(GetEntityModel(vehicle).ToString());
        #endregion

        #region Is ped pointing
        /// <summary>
        /// Is ped pointing function returns true if the ped is currently pointing their finger.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsPedPointing(int handle)
        {
            return N_0x921ce12c489c4c41(handle);
        }

        /// <summary>
        /// Gets the finger pointing camera pitch.
        /// </summary>
        /// <returns></returns>
        public static float GetPointingPitch()
        {
            var pitch = GetGameplayCamRelativePitch();
            if (pitch < -70f)
            {
                pitch = -70f;
            }
            if (pitch > 42f)
            {
                pitch = 42f;
            }
            pitch += 70f;
            pitch /= 112f;

            return pitch;
        }
        /// <summary>
        /// Gets the finger pointing camera heading.
        /// </summary>
        /// <returns></returns>
        public static float GetPointingHeading()
        {
            var heading = GetGameplayCamRelativeHeading();
            if (heading < -180f)
            {
                heading = -180f;
            }
            if (heading > 180f)
            {
                heading = 180f;
            }
            heading += 180f;
            heading /= 360f;
            heading *= -1f;
            heading += 1f;

            return heading;
        }
        /// <summary>
        /// Returns true if finger pointing is blocked by any obstacle.
        /// </summary>
        /// <returns></returns>
        public static bool GetPointingIsBlocked()
        {
            var hit = false;
            var rawHeading = GetGameplayCamRelativeHeading() / 90f;
            var heading = (float)MathUtil.Clamp(rawHeading, -180.0f, 180.0f);
            heading += 180.0f;
            heading /= 360.0f;
            var v1 = ((0.7f - 0.3f) * heading) + 0.3f;
            var pos0 = new Vector3(-0.2f, v1, 0.6f);
            var rot = new Vector3(0f, 0f, rawHeading);
            var vec1 = Vector3.Zero;

            // pos0, rot
            // ----
            var f0 = (float)Math.Cos(rot.X);
            var f1 = (float)Math.Sin(rot.X);
            vec1.X = pos0.X;
            vec1.Y = (f0 * pos0.Y) - (f1 * pos0.Z);
            vec1.Z = (f1 * pos0.Y) + (f0 * pos0.Z);
            pos0 = vec1;

            // ----
            f0 = (float)Math.Cos(rot.Y);
            f1 = (float)Math.Sin(rot.Y);
            vec1.X = (f0 * pos0.X) + (f1 * pos0.Z);
            vec1.Y = pos0.Y;
            vec1.Z = (f0 * pos0.Z) - (f1 * pos0.X);
            pos0 = vec1;

            // ----
            f0 = (float)Math.Cos(rot.Z);
            f1 = (float)Math.Sin(rot.Z);
            vec1.X = (f0 * pos0.X) - (f1 * pos0.Y);
            vec1.Y = (f1 * pos0.X) + (f0 * pos0.Y);
            vec1.Z = pos0.Z;
            pos0 = vec1;

            var pos1 = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, pos0.X, pos0.Y, pos0.Z);
            var handle = StartShapeTestCapsule(pos1.X, pos1.Y, pos1.Z - 0.2f, pos1.X, pos1.Y, pos1.Z + 0.2f, 0.4f, 95, Game.PlayerPed.Handle, 7);
            var outPos = Vector3.Zero;
            var surfaceNormal = Vector3.Zero;
            var entityHit = 0;
            GetShapeTestResult(handle, ref hit, ref outPos, ref surfaceNormal, ref entityHit);

            if (MainMenu.DebugMode)
            {
                if (hit)
                {
                    DrawMarker(28, pos1.X, pos1.Y, pos1.Z, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 0.4f, 0.4f, 123, 0, 0, 128, false, false, 0, false, null, null, false);
                }
                else
                {
                    DrawMarker(28, pos1.X, pos1.Y, pos1.Z, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 0.4f, 0.4f, 123, 53, 200, 128, false, false, 0, false, null, null, false);
                }

                DrawLine(pos1.X, pos1.Y, pos1.Z - 0.2f, pos1.X, pos1.Y, pos1.Z + 0.2f, 255, 0, 0, 255);
                DrawTextOnScreen("Blocking: " + hit.ToString(), 0f, 0f);
            }
            return hit;
        }
        #endregion

        #region Drive Tasks (WIP)
        /// <summary>
        /// Drives to waypoint
        /// </summary>
        public static void DriveToWp(int style = 0)
        {

            ClearPedTasks(Game.PlayerPed.Handle);
            DriveWanderTaskActive = false;
            DriveToWpTaskActive = true;

            var waypoint = World.WaypointPosition;

            var veh = GetVehicle();
            var model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveToCoordLongrange(Game.PlayerPed.Handle, veh.Handle, waypoint.X, waypoint.Y, waypoint.Z, GetVehicleModelMaxSpeed(model), style, 10f);
        }

        /// <summary>
        /// Drives around the area.
        /// </summary>
        public static void DriveWander(int style = 0)
        {
            ClearPedTasks(Game.PlayerPed.Handle);
            DriveWanderTaskActive = true;
            DriveToWpTaskActive = false;

            var veh = GetVehicle();
            var model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveWander(Game.PlayerPed.Handle, veh.Handle, GetVehicleModelMaxSpeed(model), style);
        }
        #endregion

        #region Quit session & Quit game
        /// <summary>
        /// Quit the current network session, but leaves you connected to the server so addons/resources are still streamed.
        /// </summary>
        public static void QuitSession() => NetworkSessionEnd(true, true);

        /// <summary>
        /// Quit the game after 5 seconds.
        /// </summary>
        public static async void QuitGame()
        {
            Notify.Info("The game will exit in 5 seconds.");
            Debug.WriteLine("Game will be terminated in 5 seconds, because the player used the Quit Game option in ScenarioCreator.");
            await BaseScript.Delay(5000);
            ForceSocialClubUpdate(); // bye bye
        }
        #endregion

        #region Spawn Vehicle
        #region Overload Spawn Vehicle Function
        /// <summary>
        /// Simple custom vehicle spawn function.
        /// </summary>
        /// <param name="vehicleName">Vehicle model name. If "custom" the user will be asked to enter a model name.</param>
        /// <param name="spawnInside">Warp the player inside the vehicle after spawning.</param>
        /// <param name="replacePrevious">Replace the previous vehicle of the player.</param>
        public static async Task<int> SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
        {
            if (vehicleName == "custom")
            {
                // Get the result.
                var result = await GetUserInput(windowTitle: "Enter Vehicle Name");
                // If the result was not invalid.
                if (!string.IsNullOrEmpty(result))
                {
                    // Convert it into a model hash.
                    var model = (uint)GetHashKey(result);
                    return await SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false, vehicleInfo: new VehicleInfo(),
                        saveName: null);
                }
                // Result was invalid.
                else
                {
                    Notify.Error(CommonErrors.InvalidInput);
                    return 0;
                }
            }
            return await SpawnVehicle(vehicleHash: (uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false,
                    vehicleInfo: new VehicleInfo(), saveName: null);
        }
        #endregion

        #region Main Spawn Vehicle Function
        /// <summary>
        /// Spawns a vehicle.
        /// </summary>
        /// <param name="vehicleHash">Model hash of the vehicle to spawn.</param>
        /// <param name="spawnInside">Teleports the player into the vehicle after spawning.</param>
        /// <param name="replacePrevious">Replaces the previous vehicle of the player with the new one.</param>
        /// <param name="skipLoad">Does not attempt to load the vehicle, but will spawn it right a way.</param>
        /// <param name="vehicleInfo">All information needed for a saved vehicle to re-apply all mods.</param>
        /// <param name="saveName">Used to get/set info about the saved vehicle data.</param>
        public static async Task<int> SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad, VehicleInfo vehicleInfo, string saveName = null, float x = 0f, float y = 0f, float z = 0f, float heading = -1f)
        {
            var speed = 0f;
            var rpm = 0f;
            if (Game.PlayerPed.IsInVehicle())
            {
                var tmpOldVehicle = GetVehicle();
                speed = GetEntitySpeedVector(tmpOldVehicle.Handle, true).Y; // get forward/backward speed only
                rpm = tmpOldVehicle.CurrentRPM;
            }

            var modelClass = GetVehicleClassFromName(vehicleHash);
            if (!VehicleSpawner.allowedCategories[modelClass])
            {
                Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
                return 0;
            }

            if (!skipLoad)
            {
                var successFull = await LoadModel(vehicleHash);
                if (!successFull || !IsModelAVehicle(vehicleHash))
                {
                    // Vehicle model is invalid.
                    Notify.Error(CommonErrors.InvalidModel);
                    return 0;
                }
            }

            Log("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");

            // Get the heading & position for where the vehicle should be spawned.
            var pos = new Vector3(x, y, z);
            if (pos.IsZero)
            {
                pos = spawnInside ? GetEntityCoords(Game.PlayerPed.Handle, true) : GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
                pos += new Vector3(0f, 0f, 1f);
            }

            heading = heading == -1 ? GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f) : heading;

            // If the previous vehicle exists...
            if (_previousVehicle != null)
            {
                // And it's actually a vehicle (rather than another random entity type)
                if (_previousVehicle.Exists() && _previousVehicle.PreviouslyOwnedByPlayer &&
                    (_previousVehicle.Occupants.Count() == 0 || _previousVehicle.Driver.Handle == Game.PlayerPed.Handle))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious))
                    {
                        // Delete it.
                        _previousVehicle.PreviouslyOwnedByPlayer = false;
                        SetEntityAsMissionEntity(_previousVehicle.Handle, true, true);
                        _previousVehicle.Delete();
                    }
                    // Otherwise
                    else
                    {
                        if (!ScenarioCreatorShared.ConfigManager.GetSettingsBool(ScenarioCreatorShared.ConfigManager.Setting.ScenarioCreator_keep_spawned_vehicles_persistent))
                        {
                            SetEntityAsMissionEntity(_previousVehicle.Handle, false, false);
                        }
                    }
                    _previousVehicle = null;
                }
            }

            if (Game.PlayerPed.IsInVehicle() && (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious)))
            {
                if (GetVehicle().Driver == Game.PlayerPed)
                {
                    var tmpveh = GetVehicle();
                    SetVehicleHasBeenOwnedByPlayer(tmpveh.Handle, false);
                    SetEntityAsMissionEntity(tmpveh.Handle, true, true);

                    if (_previousVehicle != null)
                    {
                        if (_previousVehicle.Handle == tmpveh.Handle)
                        {
                            _previousVehicle = null;
                        }
                    }
                    tmpveh.Delete();
                    Notify.Info("Your old car was removed to prevent your new car from glitching inside it. Next time, get out of your vehicle before spawning a new one if you want to keep your old one.");
                }
            }

            if (_previousVehicle != null)
            {
                _previousVehicle.PreviouslyOwnedByPlayer = false;
            }

            if (Game.PlayerPed.IsInVehicle() && x == 0f && y == 0f && z == 0f)
            {
                pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0, 8f, 0.1f) + new Vector3(0f, 0f, 1f);
            }

            // Create the new vehicle and remove the need to hotwire the car.
            var vehicle = new Vehicle(CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z, heading, true, false))
            {
                NeedsToBeHotwired = false,
                PreviouslyOwnedByPlayer = true,
                IsPersistent = true,
                IsStolen = false,
                IsWanted = false
            };

            Log($"New vehicle, hash:{vehicleHash}, handle:{vehicle.Handle}, force-re-save-name:{saveName ?? "NONE"}, created at x:{pos.X} y:{pos.Y} z:{pos.Z + 1f} " +
                $"heading:{heading}");

            // If spawnInside is true
            if (spawnInside)
            {
                // Set the vehicle's engine to be running.
                vehicle.IsEngineRunning = true;

                // Set the ped into the vehicle.
                new Ped(Game.PlayerPed.Handle).SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // If the vehicle is a helicopter and the player is in the air, set the blades to be full speed.
                if (vehicle.ClassType == VehicleClass.Helicopters && GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 10.0f)
                {
                    SetHeliBladesFullSpeed(vehicle.Handle);
                }
                // If it's not a helicopter or the player is not in the air, set the vehicle on the ground properly.
                else
                {
                    vehicle.PlaceOnGround();
                }
            }

            // If mod info about the vehicle was specified, check if it's not null.
            if (saveName != null)
            {
                ApplyVehicleModsDelayed(vehicle, vehicleInfo, 500);
            }

            // Set the previous vehicle to the new vehicle.
            _previousVehicle = vehicle;
            //vehicle.Speed = speed; // retarded feature that randomly breaks for no fucking reason
            if (!vehicle.Model.IsTrain) // to be extra fucking safe
            {
                // workaround of retarded feature above:
                SetVehicleForwardSpeed(vehicle.Handle, speed);
            }
            vehicle.CurrentRPM = rpm;

            await Delay(1); // Mandatory delay - without it radio station will not set properly

            // Set the radio station to default set by player in Vehicle Menu
            vehicle.RadioStation = (RadioStation)UserDefaults.VehicleDefaultRadio;

            // Discard the model.
            SetModelAsNoLongerNeeded(vehicleHash);

            return vehicle.Handle;
        }

        /// <summary>
        /// Waits for the given delay before applying the vehicle mods
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vehicleInfo"></param>
        private static async void ApplyVehicleModsDelayed(Vehicle vehicle, VehicleInfo vehicleInfo, int delay)
        {
            if (vehicle != null && vehicle.Exists())
            {
                vehicle.Mods.InstallModKit();
                // set the extras
                foreach (var extra in vehicleInfo.extras)
                {
                    if (DoesExtraExist(vehicle.Handle, extra.Key))
                    {
                        vehicle.ToggleExtra(extra.Key, extra.Value);
                    }
                }

                SetVehicleWheelType(vehicle.Handle, vehicleInfo.wheelType);
                SetVehicleMod(vehicle.Handle, 23, 0, vehicleInfo.customWheels);
                if (vehicle.Model.IsBike)
                {
                    SetVehicleMod(vehicle.Handle, 24, 0, vehicleInfo.customWheels);
                }
                ToggleVehicleMod(vehicle.Handle, 18, vehicleInfo.turbo);
                SetVehicleTyreSmokeColor(vehicle.Handle, vehicleInfo.colors["tyresmokeR"], vehicleInfo.colors["tyresmokeG"], vehicleInfo.colors["tyresmokeB"]);
                ToggleVehicleMod(vehicle.Handle, 20, vehicleInfo.tyreSmoke);
                ToggleVehicleMod(vehicle.Handle, 22, vehicleInfo.xenonHeadlights);
                SetVehicleLivery(vehicle.Handle, vehicleInfo.livery);

                SetVehicleColours(vehicle.Handle, vehicleInfo.colors["primary"], vehicleInfo.colors["secondary"]);
                SetVehicleInteriorColour(vehicle.Handle, vehicleInfo.colors["trim"]);
                SetVehicleDashboardColour(vehicle.Handle, vehicleInfo.colors["dash"]);

                SetVehicleExtraColours(vehicle.Handle, vehicleInfo.colors["pearlescent"], vehicleInfo.colors["wheels"]);

                SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo.plateText);
                SetVehicleNumberPlateTextIndex(vehicle.Handle, vehicleInfo.plateStyle);

                SetVehicleWindowTint(vehicle.Handle, vehicleInfo.windowTint);

                vehicle.CanTiresBurst = !vehicleInfo.bulletProofTires;

                SetVehicleEnveffScale(vehicle.Handle, vehicleInfo.enveffScale);

                VehicleOptions.SetHeadlightsColorForVehicle(vehicle, vehicleInfo.headlightColor);

                vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(red: vehicleInfo.colors["neonR"], green: vehicleInfo.colors["neonG"], blue: vehicleInfo.colors["neonB"]);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vehicleInfo.neonLeft);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vehicleInfo.neonRight);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vehicleInfo.neonFront);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vehicleInfo.neonBack);

                void DoMods()
                {
                    vehicleInfo.mods.ToList().ForEach(mod =>
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleMod(vehicle.Handle, mod.Key, mod.Value, vehicleInfo.customWheels);
                        }
                    });
                }

                DoMods();
                // Performance mods require a delay after setting the modkit,
                // so we just do it once first so all the visual mods load instantly,
                // and after a small delay we do it again to make sure all performance
                // mods have also loaded.
                await Delay(delay);
                DoMods();
            }
        }
        #endregion
        #endregion

        #region VehicleInfo struct
        /// <summary>
        /// Contains all information for a saved vehicle.
        /// </summary>
        public struct VehicleInfo
        {
            public Dictionary<string, int> colors;
            public bool customWheels;
            public Dictionary<int, bool> extras;
            public int livery;
            public uint model;
            public Dictionary<int, int> mods;
            public string name;
            public bool neonBack;
            public bool neonFront;
            public bool neonLeft;
            public bool neonRight;
            public string plateText;
            public int plateStyle;
            public bool turbo;
            public bool tyreSmoke;
            public int version;
            public int wheelType;
            public int windowTint;
            public bool xenonHeadlights;
            public bool bulletProofTires;
            public int headlightColor;
            public float enveffScale;
            public string Category;
        };
        #endregion

        #region Save Vehicle
        /// <summary>
        /// Saves the vehicle the player is currently in to the client's kvp storage.
        /// </summary>
        public static async void SaveVehicle(string updateExistingSavedVehicleName = null)
        {
            // Only continue if the player is in a vehicle.
            if (Game.PlayerPed.IsInVehicle())
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // Make sure the entity is actually a vehicle and it still exists, and it's not dead.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.IsDriveable)
                {
                    #region new saving method
                    var mods = new Dictionary<int, int>();

                    foreach (var mod in veh.Mods.GetAllMods())
                    {
                        mods.Add((int)mod.ModType, mod.Index);
                    }

                    #region colors
                    var colors = new Dictionary<string, int>();
                    var primaryColor = 0;
                    var secondaryColor = 0;
                    var pearlescentColor = 0;
                    var wheelColor = 0;
                    var dashColor = 0;
                    var trimColor = 0;
                    GetVehicleExtraColours(veh.Handle, ref pearlescentColor, ref wheelColor);
                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref trimColor);
                    colors.Add("primary", primaryColor);
                    colors.Add("secondary", secondaryColor);
                    colors.Add("pearlescent", pearlescentColor);
                    colors.Add("wheels", wheelColor);
                    colors.Add("dash", dashColor);
                    colors.Add("trim", trimColor);
                    var neonR = 255;
                    var neonG = 255;
                    var neonB = 255;
                    if (veh.Mods.HasNeonLights)
                    {
                        GetVehicleNeonLightsColour(veh.Handle, ref neonR, ref neonG, ref neonB);
                    }
                    colors.Add("neonR", neonR);
                    colors.Add("neonG", neonG);
                    colors.Add("neonB", neonB);
                    var tyresmokeR = 0;
                    var tyresmokeG = 0;
                    var tyresmokeB = 0;
                    GetVehicleTyreSmokeColor(veh.Handle, ref tyresmokeR, ref tyresmokeG, ref tyresmokeB);
                    colors.Add("tyresmokeR", tyresmokeR);
                    colors.Add("tyresmokeG", tyresmokeG);
                    colors.Add("tyresmokeB", tyresmokeB);
                    #endregion

                    var extras = new Dictionary<int, bool>();
                    for (var i = 0; i < 20; i++)
                    {
                        if (veh.ExtraExists(i))
                        {
                            extras.Add(i, veh.IsExtraOn(i));
                        }
                    }

                    var vi = new VehicleInfo()
                    {
                        colors = colors,
                        customWheels = GetVehicleModVariation(veh.Handle, 23),
                        extras = extras,
                        livery = GetVehicleLivery(veh.Handle),
                        model = (uint)GetEntityModel(veh.Handle),
                        mods = mods,
                        name = GetLabelText(GetDisplayNameFromVehicleModel((uint)GetEntityModel(veh.Handle))),
                        neonBack = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back),
                        neonFront = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front),
                        neonLeft = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left),
                        neonRight = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right),
                        plateText = veh.Mods.LicensePlate,
                        plateStyle = (int)veh.Mods.LicensePlateStyle,
                        turbo = IsToggleModOn(veh.Handle, 18),
                        tyreSmoke = IsToggleModOn(veh.Handle, 20),
                        version = 1,
                        wheelType = GetVehicleWheelType(veh.Handle),
                        windowTint = (int)veh.Mods.WindowTint,
                        xenonHeadlights = IsToggleModOn(veh.Handle, 22),
                        bulletProofTires = !veh.CanTiresBurst,
                        headlightColor = VehicleOptions.GetHeadlightsColorForVehicle(veh),
                        enveffScale = GetVehicleEnveffScale(veh.Handle),
                        Category = "Uncategorized"
                    };

                    #endregion

                    if (updateExistingSavedVehicleName == null)
                    {
                        // Ask the user for a save name (will be displayed to the user and will be used as unique identifier for this vehicle)
                        var saveName = await GetUserInput(windowTitle: "Enter a save name", maxInputLength: 30);
                        // If the name is not invalid.
                        if (!string.IsNullOrEmpty(saveName))
                        {
                            // Save everything from the dictionary into the client's kvp storage.
                            // If the save was successfull:
                            if (StorageManager.SaveVehicleInfo("veh_" + saveName, vi, false))
                            {
                                Notify.Success($"Vehicle {saveName} saved.");
                            }
                            // If the save was not successfull:
                            else
                            {
                                Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: "(" + saveName + ")");
                            }
                        }
                        // The user did not enter a valid name to use as a save name for this vehicle.
                        else
                        {
                            Notify.Error(CommonErrors.InvalidSaveName);
                        }
                    }
                    // We need to update an existing slot.
                    else
                    {
                        StorageManager.SaveVehicleInfo("veh_" + updateExistingSavedVehicleName, vi, true);
                    }

                }
                // The player is not inside a vehicle, or the vehicle is dead/not existing so we won't do anything. Only alert the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle, placeholderValue: "to save it");
                }
            }
            // The player is not inside a vehicle.
            else
            {
                Notify.Error(CommonErrors.NoVehicle);
            }

            // update the saved vehicles menu list to reflect the new saved car.
            MainMenu.SavedVehiclesMenu?.UpdateMenuAvailableCategories();

        }
        #endregion

        #region Get Saved Vehicles Dictionary
        /// <summary>
        /// Returns a collection of all saved vehicles, with their save name and saved vehicle info struct.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, VehicleInfo> GetSavedVehicles()
        {
            // Create a list to store all saved vehicle names in.
            var savedVehicleNames = new List<string>();
            // Start looking for kvps starting with veh_
            var findHandle = StartFindKvp("veh_");
            // Keep looking...
            while (true)
            {
                // Get the kvp string key.
                var vehString = FindKvp(findHandle);

                // If it exists then the key to the list.
                if (vehString is not "" and not null and not "NULL")
                {
                    //Debug.WriteLine(vehString);
                    savedVehicleNames.Add(vehString);
                }
                // Otherwise stop.
                else
                {
                    EndFindKvp(findHandle);
                    break;
                }
            }

            // Create a Dictionary to store all vehicle information in.
            //var vehiclesList = new Dictionary<string, Dictionary<string, string>>();
            var vehiclesList = new Dictionary<string, VehicleInfo>();
            // Loop through all save names (keys) from the list above, convert the string into a dictionary 
            // and add it to the dictionary above, with the vehicle save name as the key.
            foreach (var saveName in savedVehicleNames)
            {
                vehiclesList.Add(saveName, StorageManager.GetSavedVehicleInfo(saveName));
            }
            // Return the vehicle dictionary containing all vehicle save names (keys) linked to the correct vehicle
            // including all vehicle mods/customization parts.
            return vehiclesList;
        }
        #endregion

        #region Load Model
        /// <summary>
        /// Check and load a model.
        /// </summary>
        /// <param name="modelHash"></param>
        /// <returns>True if model is valid & loaded, false if model is invalid.</returns>
        private static async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (IsModelInCdimage(modelHash))
            {
                // Load the model.
                RequestModel(modelHash);
                // Wait until it's loaded.
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }
                // Model is loaded, return true.
                return true;
            }
            // Model is not valid or is not loaded correctly.
            else
            {
                // Return false.
                return false;
            }
        }
        #endregion

        #region GetUserInput
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                var keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return GetOnscreenKeyboardResult();
                    default:
                        await Delay(0);
                        break;
                }
            }
        }
        #endregion

        #region Set License Plate Text
        /// <summary>
        /// Set the license plate text using the player's custom input.
        /// </summary>
        public static async void SetLicensePlateCustomText()
        {
            // Get the vehicle.
            var veh = GetVehicle();
            // If it exists.
            if (veh != null && veh.Exists())
            {
                if (Game.PlayerPed == veh.Driver)
                {
                    // Get the input.
                    var text = await GetUserInput(windowTitle: "Enter License Plate", defaultText: veh.Mods.LicensePlate ?? "", maxInputLength: 8);
                    // If the input is valid.
                    if (!string.IsNullOrEmpty(text))
                    {
                        // Set the license plate.
                        SetVehicleNumberPlateText(veh.Handle, text);
                    }
                    // No valid text was given.
                    else
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                }
                else
                {
                    Notify.Error(CommonErrors.NeedToBeTheDriver);
                }



            }
            // If it doesn't exist, notify the user.
            else
            {
                Notify.Error(CommonErrors.NoVehicle);
            }


        }
        #endregion

        #region ToProperString()
        /// <summary>
        /// Converts a PascalCaseString to a Propper Case String.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Input string converted to a normal sentence.</returns>
        public static string ToProperString(string inputString)
        {
            var outputString = "";
            var prevUpper = true;
            foreach (var c in inputString)
            {
                if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
                {
                    if (prevUpper)
                    {
                        outputString += $"{c}";
                    }
                    else
                    {
                        outputString += $" {c}";
                    }
                    prevUpper = true;
                }
                else
                {
                    prevUpper = false;
                    outputString += c.ToString();
                }
            }
            while (outputString.IndexOf("  ") != -1)
            {
                outputString = outputString.Replace("  ", " ");
            }
            return outputString;
        }
        #endregion

        #region Play Scenarios
        /// <summary>
        /// Play the specified scenario name.
        /// If "forcestop" is specified, any currrently playing scenarios will be forcefully stopped.
        /// </summary>
        /// <param name="scenarioName"></param>
        public static void PlayScenario(string scenarioName)
        {
            // If there's currently no scenario playing, or the current scenario is not the same as the new scenario, then..
            if (_currentScenario == "" || _currentScenario != scenarioName)
            {
                // Set the current scenario.
                _currentScenario = scenarioName;
                // Clear all tasks to make sure the player is ready to play the scenario.
                ClearPedTasks(Game.PlayerPed.Handle);

                var canPlay = true;
                // Check if the player CAN play a scenario... 
                if (IsPedRunning(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are running.", true, false);
                    canPlay = false;
                }
                if (IsEntityDead(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are dead.", true, false);
                    canPlay = false;
                }
                if (IsPlayerInCutscene(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are in a cutscene.", true, false);
                    canPlay = false;
                }
                if (IsPedFalling(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are falling.", true, false);
                    canPlay = false;
                }
                if (IsPedRagdoll(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are currently in a ragdoll state.", true, false);
                    canPlay = false;
                }
                if (!IsPedOnFoot(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You must be on foot to start a scenario.", true, false);
                    canPlay = false;
                }
                if (NetworkIsInSpectatorMode())
                {
                    Notify.Alert("You can't start a scenario when you are currently spectating.", true, false);
                    canPlay = false;
                }
                if (GetEntitySpeed(Game.PlayerPed.Handle) > 5.0f)
                {
                    Notify.Alert("You can't start a scenario when you are moving too fast.", true, false);
                    canPlay = false;
                }

                if (canPlay)
                {
                    // If the scenario is a "sit" scenario, then the scenario needs to be played at a specific location.
                    if (PedScenarios.PositionBasedScenarios.Contains(scenarioName))
                    {
                        // Get the offset-position from the player. (0.5m behind the player, and 0.5m below the player seems fine for most scenarios)
                        var pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, -0.5f, -0.5f);
                        var heading = GetEntityHeading(Game.PlayerPed.Handle);
                        // Play the scenario at the specified location.
                        TaskStartScenarioAtPosition(Game.PlayerPed.Handle, scenarioName, pos.X, pos.Y, pos.Z, heading, -1, true, false);
                    }
                    // If it's not a sit scenario (or maybe it is, but using the above native causes other
                    // issues for some sit scenarios so those are not registered as "sit" scenarios), then play it at the current player's position.
                    else
                    {
                        TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenarioName, 0, true);
                    }
                }
            }
            // If the new scenario is the same as the currently playing one, cancel the current scenario.
            else
            {
                _currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedSecondaryTask(Game.PlayerPed.Handle);
            }

            // If the scenario name to play is called "forcestop" then clear the current scenario and force any tasks to be cleared.
            if (scenarioName == "forcestop")
            {
                _currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedTasksImmediately(Game.PlayerPed.Handle);
            }

        }
        #endregion

        #region Data parsing functions
        /// <summary>
        /// Converts a simple json string (only containing (string) key : (string) value).
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        #endregion

        #region Weather Sync
        /// <summary>
        /// Update the server with the new weather type, blackout status and dynamic weather changes enabled status.
        /// </summary>
        /// <param name="newWeather">The new weather type.</param>
        /// <param name="blackout">Manual blackout mode enabled/disabled.</param>
        /// <param name="dynamicChanges">Dynamic weather changes enabled/disabled.</param>
        public static void UpdateServerWeather(string newWeather, bool blackout, bool dynamicChanges, bool isSnowEnabled) => TriggerServerEvent("ScenarioCreator:UpdateServerWeather", newWeather, blackout, dynamicChanges, isSnowEnabled);

        /// <summary>
        /// Modify the clouds for everyone. If removeClouds is true, then remove all clouds. If it's false, then randomize the clouds.
        /// </summary>
        /// <param name="removeClouds">Removes the clouds from the sky if true, otherwise randomizes the clouds type for all players.</param>
        public static void ModifyClouds(bool removeClouds) => TriggerServerEvent("ScenarioCreator:UpdateServerWeatherCloudsType", removeClouds);
        #endregion

        #region Time Sync
        /// <summary>
        /// Update the server with the new time. If an invalid time is provided, then the time will be set to midnight (00:00);
        /// </summary>
        /// <param name="hours">Hours (0-23)</param>
        /// <param name="minutes">Minutes (0-59)</param>
        /// <param name="freezeTime">Should the time be frozen?</param>
        public static void UpdateServerTime(int hours, int minutes, bool freezeTime)
        {
            var realHours = hours;
            var realMinutes = minutes;
            if (hours is > 23 or < 0)
            {
                realHours = 0;
            }
            if (minutes is > 59 or < 0)
            {
                realMinutes = 0;
            }
            TriggerServerEvent("ScenarioCreator:UpdateServerTime", realHours, realMinutes, freezeTime);
        }
        #endregion

        #region StringToStringArray
        /// <summary>
        /// Converts the inputString into a string[] (array).
        /// Each string in the array is up to 99 characters long at max.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string[] StringToArray(string inputString)
        {
            return CitizenFX.Core.UI.Screen.StringToArray(inputString);
        }
        #endregion

        #region Hud Functions
        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        /// <param name="disableTextOutline">Disables the default text outline.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (IsHudPreferenceSwitchedOn() && Hud.IsVisible && !MainMenu.MiscSettingsMenu.HideHud && !IsPlayerSwitchInProgress() && IsScreenFadedIn() && !IsPauseMenuActive() && !IsFrontendFading() && !IsPauseMenuRestarting() && !IsHudHidden())
            {
                SetTextFont(font);
                SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    SetTextWrap(0f, xPosition);
                }
                SetTextJustification((int)justification);
                if (!disableTextOutline) { SetTextOutline(); }
                BeginTextCommandDisplayText("STRING");
                AddTextComponentSubstringPlayerName(text);
                EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
        #endregion

        #region ped info struct
        public struct PedInfo
        {
            public int version;
            public uint model;
            public bool isMpPed;
            public Dictionary<int, int> props;
            public Dictionary<int, int> propTextures;
            public Dictionary<int, int> drawableVariations;
            public Dictionary<int, int> drawableVariationTextures;
        };
        #endregion

        #region Get "Header" Menu Item
        /// <summary>
        /// Get a header menu item (text-centered, disabled MenuItem)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static MenuItem GetSpacerMenuItem(string title, string description = null)
        {
            var output = "~h~";
            var length = title.Length;
            var totalSize = 80 - length;

            for (var i = 0; i < (totalSize / 2) - (length / 2); i++)
            {
                output += " ";
            }
            output += title;
            var item = new MenuItem(output, description ?? "")
            {
                Enabled = false
            };
            return item;
        }
        #endregion

        #region Log Function
        /// <summary>
        /// Print data to the console and save it to the CitizenFX.log file. Only when ScenarioCreator debugging mode is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(string data)
        {
            if (MainMenu.DebugMode)
            {
                Debug.WriteLine(@data);
            }
        }
        #endregion

        #region Get Currently Opened Menu
        /// <summary>
        /// Returns the currently opened menu, if no menu is open, it'll return null.
        /// </summary>
        /// <returns></returns>
        public static Menu GetOpenMenu()
        {
            return MenuController.GetCurrentMenu();
        }
        #endregion

        #region Disable Movement Controls
        /// <summary>
        /// Disables all movement and camera related controls this frame.
        /// </summary>
        /// <param name="disableMovement"></param>
        /// <param name="disableCameraMovement"></param>
        public static void DisableMovementControlsThisFrame(bool disableMovement, bool disableCameraMovement)
        {
            if (disableMovement)
            {
                Game.DisableControlThisFrame(0, Control.MoveDown);
                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeft);
                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
                Game.DisableControlThisFrame(0, Control.MoveRight);
                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
                Game.DisableControlThisFrame(0, Control.MoveUp);
                Game.DisableControlThisFrame(0, Control.MoveUpDown);
                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleFlyMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDown);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDownOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeft);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeftRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRightOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUp);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                Game.DisableControlThisFrame(0, Control.VehicleSubMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.Duck);
                Game.DisableControlThisFrame(0, Control.SelectWeapon);
            }
            if (disableCameraMovement)
            {
                Game.DisableControlThisFrame(0, Control.LookBehind);
                Game.DisableControlThisFrame(0, Control.LookDown);
                Game.DisableControlThisFrame(0, Control.LookDownOnly);
                Game.DisableControlThisFrame(0, Control.LookLeft);
                Game.DisableControlThisFrame(0, Control.LookLeftOnly);
                Game.DisableControlThisFrame(0, Control.LookLeftRight);
                Game.DisableControlThisFrame(0, Control.LookRight);
                Game.DisableControlThisFrame(0, Control.LookRightOnly);
                Game.DisableControlThisFrame(0, Control.LookUp);
                Game.DisableControlThisFrame(0, Control.LookUpDown);
                Game.DisableControlThisFrame(0, Control.LookUpOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookDownOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftRight);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpDown);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook2);
                Game.DisableControlThisFrame(0, Control.VehicleLookBehind);
                Game.DisableControlThisFrame(0, Control.VehicleLookLeft);
                Game.DisableControlThisFrame(0, Control.VehicleLookRight);
                Game.DisableControlThisFrame(0, Control.NextCamera);
                Game.DisableControlThisFrame(0, Control.VehicleFlyAttackCamera);
                Game.DisableControlThisFrame(0, Control.VehicleCinCam);
            }
        }
        #endregion

        #region Draw model dimensions math util functions

        /*
            These util functions are taken from Deltanic's mapeditor resource for FiveM.
            https://gitlab.com/shockwave-fivem/mapeditor/tree/master
            Thank you Deltanic for allowing me to use these functions here.
        */

        /// <summary>
        /// Draws the bounding box for the entity with the provided rgba color.
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void DrawEntityBoundingBox(Entity ent, int r, int g, int b, int a)
        {
            var box = GetEntityBoundingBox(ent.Handle);
            DrawBoundingBox(box, r, g, b, a);
        }

        /// <summary>
        /// Gets the bounding box of the entity model in world coordinates, used by <see cref="DrawEntityBoundingBox(Entity, int, int, int, int)"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static Vector3[] GetEntityBoundingBox(int entity)
        {
            var min = Vector3.Zero;
            var max = Vector3.Zero;

            GetModelDimensions((uint)GetEntityModel(entity), ref min, ref max);
            //const float pad = 0f;
            const float pad = 0.001f;
            var retval = new Vector3[8]
            {
                // Bottom
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, min.Z - pad),

                // Top
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, max.Z + pad)
            };
            return retval;
        }

        /// <summary>
        /// Draws the edge poly faces and the edge lines for the specific box coordinates using the specified rgba color.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawBoundingBox(Vector3[] box, int r, int g, int b, int a)
        {
            var polyMatrix = GetBoundingBoxPolyMatrix(box);
            var edgeMatrix = GetBoundingBoxEdgeMatrix(box);

            DrawPolyMatrix(polyMatrix, r, g, b, a);
            DrawEdgeMatrix(edgeMatrix, 255, 255, 255, 255);
        }

        /// <summary>
        /// Gets the coordinates for all poly box faces.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static Vector3[][] GetBoundingBoxPolyMatrix(Vector3[] box)
        {
            return new Vector3[12][]
            {
                new Vector3[3] { box[2], box[1], box[0] },
                new Vector3[3] { box[3], box[2], box[0] },

                new Vector3[3] { box[4], box[5], box[6] },
                new Vector3[3] { box[4], box[6], box[7] },

                new Vector3[3] { box[2], box[3], box[6] },
                new Vector3[3] { box[7], box[6], box[3] },

                new Vector3[3] { box[0], box[1], box[4] },
                new Vector3[3] { box[5], box[4], box[1] },

                new Vector3[3] { box[1], box[2], box[5] },
                new Vector3[3] { box[2], box[6], box[5] },

                new Vector3[3] { box[4], box[7], box[3] },
                new Vector3[3] { box[4], box[3], box[0] }
            };
        }

        /// <summary>
        /// Gets the coordinates for all edge coordinates.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static Vector3[][] GetBoundingBoxEdgeMatrix(Vector3[] box)
        {
            return new Vector3[12][]
            {
                new Vector3[2] { box[0], box[1] },
                new Vector3[2] { box[1], box[2] },
                new Vector3[2] { box[2], box[3] },
                new Vector3[2] { box[3], box[0] },

                new Vector3[2] { box[4], box[5] },
                new Vector3[2] { box[5], box[6] },
                new Vector3[2] { box[6], box[7] },
                new Vector3[2] { box[7], box[4] },

                new Vector3[2] { box[0], box[4] },
                new Vector3[2] { box[1], box[5] },
                new Vector3[2] { box[2], box[6] },
                new Vector3[2] { box[3], box[7] }
            };
        }

        /// <summary>
        /// Draws the poly matrix faces.
        /// </summary>
        /// <param name="polyCollection"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawPolyMatrix(Vector3[][] polyCollection, int r, int g, int b, int a)
        {
            foreach (var poly in polyCollection)
            {
                var x1 = poly[0].X;
                var y1 = poly[0].Y;
                var z1 = poly[0].Z;

                var x2 = poly[1].X;
                var y2 = poly[1].Y;
                var z2 = poly[1].Z;

                var x3 = poly[2].X;
                var y3 = poly[2].Y;
                var z3 = poly[2].Z;
                DrawPoly(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a);
            }
        }

        /// <summary>
        /// Draws the edge lines for the model dimensions.
        /// </summary>
        /// <param name="linesCollection"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawEdgeMatrix(Vector3[][] linesCollection, int r, int g, int b, int a)
        {
            foreach (var line in linesCollection)
            {
                var x1 = line[0].X;
                var y1 = line[0].Y;
                var z1 = line[0].Z;

                var x2 = line[1].X;
                var y2 = line[1].Y;
                var z2 = line[1].Z;

                DrawLine(x1, y1, z1, x2, y2, z2, r, g, b, a);
            }
        }
        #endregion

        #region Map (math util) function
        /// <summary>
        /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min_in">The minimum range value of the value.</param>
        /// <param name="max_in">The max range value of the value.</param>
        /// <param name="min_out">The min output range value.</param>
        /// <param name="max_out">The max output range value.</param>
        /// <returns></returns>
        public static float Map(float value, float min_in, float max_in, float min_out, float max_out)
        {
            return ((value - min_in) * (max_out - min_out) / (max_in - min_in)) + min_out;
        }

        /// <summary>
        /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min_in">The minimum range value of the value.</param>
        /// <param name="max_in">The max range value of the value.</param>
        /// <param name="min_out">The min output range value.</param>
        /// <param name="max_out">The max output range value.</param>
        /// <returns></returns>
        public static double Map(double value, double min_in, double max_in, double min_out, double max_out)
        {
            return ((value - min_in) * (max_out - min_out) / (max_in - min_in)) + min_out;
        }
        #endregion

        #region Private message notification
        public static void PrivateMessage(string source, string message) => PrivateMessage(source, message, false);
        public static async void PrivateMessage(string source, string message, bool sent)
        {
            MainMenu.PlayersList.RequestPlayerList();
            await MainMenu.PlayersList.WaitRequested();

            var name = MainMenu.PlayersList.ToList()
                .Find(plr => plr.ServerId.ToString() == source)?.Name ?? "**Invalid**";

            if (MainMenu.MiscSettingsMenu == null || MainMenu.MiscSettingsMenu.MiscDisablePrivateMessages)
            {
                if (!(sent && source == Game.Player.ServerId.ToString()))
                {
                    TriggerServerEvent("ScenarioCreator:PmsDisabled", source);
                }
                return;
            }

            var sourcePlayer = new Player(GetPlayerFromServerId(int.Parse(source)));
            if (sourcePlayer != null)
            {
                var headshotHandle = RegisterPedheadshot(sourcePlayer.Character.Handle);
                var timer = GetGameTimer();
                var tookTooLong = false;
                while (!IsPedheadshotReady(headshotHandle) || !IsPedheadshotValid(headshotHandle))
                {
                    await Delay(0);
                    if (GetGameTimer() - timer > 2000)
                    {
                        // took too long.
                        tookTooLong = true;
                        break;
                    }
                }
                if (!tookTooLong)
                {
                    var headshotTxd = GetPedheadshotTxdString(headshotHandle);
                    if (sent)
                    {
                        Notify.CustomImage(headshotTxd, headshotTxd, message, $"<C>{GetSafePlayerName(name)}</C>", "Message Sent", true, 1);
                    }
                    else
                    {
                        Notify.CustomImage(headshotTxd, headshotTxd, message, $"<C>{GetSafePlayerName(name)}</C>", "Message Received", true, 1);
                    }
                }
                else
                {
                    if (sent)
                    {
                        Notify.Custom($"PM From: <C>{GetSafePlayerName(name)}</C>. Message: {message}");
                    }
                    else
                    {
                        Notify.Custom($"PM To: <C>{GetSafePlayerName(name)}</C>. Message: {message}");
                    }
                }
                UnregisterPedheadshot(headshotHandle);
            }
        }
        #endregion

        #region Keyfob personal vehicle func
        public static async void PressKeyFob(Vehicle veh)
        {
            var player = Game.Player;
            if (player != null && !player.IsDead && !player.Character.IsInVehicle())
            {
                var KeyFobHashKey = (uint)GetHashKey("p_car_keys_01");
                RequestModel(KeyFobHashKey);
                while (!HasModelLoaded(KeyFobHashKey))
                {
                    await Delay(0);
                }

                var KeyFobObject = CreateObject((int)KeyFobHashKey, 0, 0, 0, true, true, true);
                AttachEntityToEntity(KeyFobObject, player.Character.Handle, GetPedBoneIndex(player.Character.Handle, 57005), 0.09f, 0.03f, -0.02f, -76f, 13f, 28f, false, true, true, true, 0, true);
                SetModelAsNoLongerNeeded(KeyFobHashKey); // cleanup model from memory

                ClearPedTasks(player.Character.Handle);
                SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("WEAPON_UNARMED"), true);
                //if (player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
                //{
                //    player.Character.Weapons.Give(WeaponHash.Unarmed, 1, true, true);
                //}

                // if (!HasEntityClearLosToEntityInFront(player.Character.Handle, veh.Handle))
                {
                    /*
                    TODO: Work out how to get proper heading between entities.
                    */


                    //SetPedDesiredHeading(player.Character.Handle, )
                    //float heading = GetHeadingFromVector_2d(player.Character.Position.X - veh.Position.Y, player.Character.Position.Y - veh.Position.X);
                    //double x = Math.Cos(player.Character.Position.X) * Math.Sin(player.Character.Position.Y - (double)veh.Position.Y);
                    //double y = Math.Cos(player.Character.Position.X) * Math.Sin(veh.Position.X) - Math.Sin(player.Character.Position.X) * Math.Cos(veh.Position.X) * Math.Cos(player.Character.Position.Y - (double)veh.Position.Y);
                    //float heading = (float)Math.Atan2(x, y);
                    //Debug.WriteLine(heading.ToString());
                    //SetPedDesiredHeading(player.Character.Handle, heading);

                    ClearPedTasks(Game.PlayerPed.Handle);
                    TaskTurnPedToFaceEntity(player.Character.Handle, veh.Handle, 500);
                }

                var animDict = "anim@mp_player_intmenu@key_fob@";
                RequestAnimDict(animDict);
                while (!HasAnimDictLoaded(animDict))
                {
                    await Delay(0);
                }
                player.Character.Task.PlayAnimation(animDict, "fob_click", 3f, 1000, AnimationFlags.UpperBodyOnly);
                PlaySoundFromEntity(-1, "Remote_Control_Fob", player.Character.Handle, "PI_Menu_Sounds", true, 0);


                await Delay(1250);
                DetachEntity(KeyFobObject, false, false);
                DeleteObject(ref KeyFobObject);
                RemoveAnimDict(animDict); // cleanup anim dict from memory
            }

            await Delay(0);
        }
        #endregion

        #region Encoded float to normal float
        ///// <summary>
        ///// Converts an IEEE 754 (int encoded) floating-point to a real float value.
        ///// </summary>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //public static float IntToFloat(int input)
        //{
        //    // This function is based on the 'hex2float' snippet found here for Lua:
        //    // https://stackoverflow.com/a/19996852

        //    //string d = input.ToString("X8");

        //    var s1 = (char)int.Parse(d.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        //    var s2 = (char)int.Parse(d.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        //    var s3 = (char)int.Parse(d.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //    var s4 = (char)int.Parse(d.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

        //    var b1 = BitConverter.GetBytes(s1)[0];
        //    var b2 = BitConverter.GetBytes(s2)[0];
        //    var b3 = BitConverter.GetBytes(s3)[0];
        //    var b4 = BitConverter.GetBytes(s4)[0];

        //    int sign = b1 > 0x7F ? -1 : 1;
        //    int expo = ((b1 % 0x80) * 0x2) + (b2 / 0x80);
        //    float mant = ((b2 % 0x80) * 0x100 + b3) * 0x100 + b4;

        //    float n;
        //    if (mant == 0 && expo == 0)
        //    {
        //        n = sign * 0.0f;
        //    }
        //    else if (expo == 0xFF)
        //    {
        //        if (mant == 0)
        //        {
        //            n = (float)((double)sign * Math.E);
        //        }
        //        else
        //        {
        //            n = 0.0f;
        //        }
        //    }
        //    else
        //    {
        //        double left = 1.0 + mant / 0x800000;
        //        int right = expo - 0x7F;
        //        float other = (float)left * ((float)right * (float)right);
        //        n = (float)sign * (float)other;
        //    }
        //    return n;
        //}
        #endregion
    }
}
