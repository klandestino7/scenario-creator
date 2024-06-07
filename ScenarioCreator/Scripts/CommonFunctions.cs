using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using ScenarioCreatorClient.Scripts;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

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

        #region Spawn Vehicle
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
            // if (!VehicleSpawner.allowedCategories[modelClass])
            // {
            // Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
            // return 0;
            // }

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

            Utils.Log("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");

            // Get the heading & position for where the vehicle should be spawned.
            var pos = new Vector3(x, y, z);
            if (pos.IsZero)
            {
                pos = spawnInside ? GetEntityCoords(Game.PlayerPed.Handle, true) : GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
                pos += new Vector3(0f, 0f, 1f);
            }

            heading = heading == -1 ? GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f) : heading;

            if (Game.PlayerPed.IsInVehicle())
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

            Utils.Log($"New vehicle, hash:{vehicleHash}, handle:{vehicle.Handle}, force-re-save-name:{saveName ?? "NONE"}, created at x:{pos.X} y:{pos.Y} z:{pos.Z + 1f} " +
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

                // VehicleOptions.SetHeadlightsColorForVehicle(vehicle, vehicleInfo.headlightColor);

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
                        // headlightColor = VehicleOptions.GetHeadlightsColorForVehicle(veh),
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
                            // if (StorageManager.SaveVehicleInfo("veh_" + saveName, vi, false))
                            // {
                            // Notify.Success($"Vehicle {saveName} saved.");
                            // }
                            // If the save was not successfull:
                            // else
                            // {
                            //  Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: "(" + saveName + ")");
                            // }
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
                        // StorageManager.SaveVehicleInfo("veh_" + updateExistingSavedVehicleName, vi, true);
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
            // MainMenu.SavedVehiclesMenu?.UpdateMenuAvailableCategories();

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
        #endregion

        /// <summary>
        /// Copy of <see cref="BaseScript.Delay(int)"/>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }

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
    }
}
