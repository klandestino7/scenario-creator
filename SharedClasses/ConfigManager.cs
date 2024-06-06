using System;
using System.Collections.Generic;

using CitizenFX.Core;

using Newtonsoft.Json;


using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorShared
{
    public static class ConfigManager
    {
        public enum Setting
        {
            // General settings
            ScenarioCreator_use_permissions,
            ScenarioCreator_menu_staff_only,
            ScenarioCreator_menu_toggle_key,
            ScenarioCreator_noclip_toggle_key,
            ScenarioCreator_keep_spawned_vehicles_persistent,
            ScenarioCreator_use_els_compatibility_mode,
            ScenarioCreator_handle_invisibility,
            ScenarioCreator_quit_session_in_rockstar_editor,
            ScenarioCreator_server_info_message,
            ScenarioCreator_server_info_website_url,
            ScenarioCreator_teleport_to_wp_keybind_key,
            ScenarioCreator_disable_spawning_as_default_character,
            ScenarioCreator_enable_animals_spawn_menu,
            ScenarioCreator_pvp_mode,
            keep_player_head_props,
            ScenarioCreator_disable_server_info_convars,
            ScenarioCreator_player_names_distance,
            ScenarioCreator_disable_entity_outlines_tool,
            ScenarioCreator_disable_player_stats_setup,

            // Vehicle Chameleon Colours
            ScenarioCreator_using_chameleon_colours,

            // Kick & ban settings
            ScenarioCreator_default_ban_message_information,
            ScenarioCreator_auto_ban_cheaters,
            ScenarioCreator_auto_ban_cheaters_ban_message,
            ScenarioCreator_log_ban_actions,
            ScenarioCreator_log_kick_actions,

            // Weather settings
            ScenarioCreator_enable_weather_sync,
            ScenarioCreator_enable_dynamic_weather,
            ScenarioCreator_dynamic_weather_timer,
            ScenarioCreator_current_weather,
            ScenarioCreator_blackout_enabled,
            ScenarioCreator_weather_change_duration,
            ScenarioCreator_enable_snow,

            // Time settings
            ScenarioCreator_enable_time_sync,
            ScenarioCreator_freeze_time,
            ScenarioCreator_ingame_minute_duration,
            ScenarioCreator_current_hour,
            ScenarioCreator_current_minute,
            ScenarioCreator_sync_to_machine_time,

            // Voice Chat Settings
            ScenarioCreator_override_voicechat_default_range,

            // Key Mapping
            ScenarioCreator_keymapping_id,
        }

        /// <summary>
        /// Get a boolean setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static bool GetSettingsBool(Setting setting)
        {
            return GetConvar(setting.ToString(), "false") == "true";
        }

        /// <summary>
        /// Get an integer setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static int GetSettingsInt(Setting setting)
        {
            var convarInt = GetConvarInt(setting.ToString(), -1);
            if (convarInt == -1)
            {
                if (int.TryParse(GetConvar(setting.ToString(), "-1"), out var convarIntAlt))
                {
                    return convarIntAlt;
                }
            }
            return convarInt;
        }

        /// <summary>
        /// Get a float setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static float GetSettingsFloat(Setting setting)
        {
            if (float.TryParse(GetConvar(setting.ToString(), "-1.0"), out var result))
            {
                return result;
            }
            return -1f;
        }

        /// <summary>
        /// Get a string setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string GetSettingsString(Setting setting, string defaultValue = null)
        {
            var value = GetConvar(setting.ToString(), defaultValue ?? "");
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return value;
        }

        /// <summary>
        /// Debugging mode
        /// </summary>
        public static bool DebugMode => IsDuplicityVersion() ? IsServerDebugModeEnabled() : IsClientDebugModeEnabled();

        /// <summary>
        /// Default value for server debugging mode.
        /// </summary>
        /// <returns></returns>
        public static bool IsServerDebugModeEnabled()
        {
            return GetResourceMetadata("ScenarioCreator", "server_debug_mode", 0).ToLower() == "true";
        }

        /// <summary>
        /// Default value for client debugging mode.
        /// </summary>
        /// <returns></returns>
        public static bool IsClientDebugModeEnabled()
        {
            return GetResourceMetadata("ScenarioCreator", "client_debug_mode", 0).ToLower() == "true";
        }

        #region Get saved locations from the locations.json
        /// <summary>
        /// Gets the locations.json data.
        /// </summary>
        /// <returns></returns>
        public static Locations GetLocations()
        {
            var data = new Locations();

            var jsonFile = LoadResourceFile(GetCurrentResourceName(), "config/locations.json");
            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
#if CLIENT
                    ScenarioCreatorClient.Notify.Error("The locations.json file is empty or does not exist, please tell the server owner to fix this.");
#endif
#if SERVER
                    ScenarioCreatorServer.DebugLog.Log("The locations.json file is empty or does not exist, please fix this.", ScenarioCreatorServer.DebugLog.LogLevel.error);
#endif
                }
                else
                {
                    data = JsonConvert.DeserializeObject<Locations>(jsonFile);
                }
            }
            catch (Exception e)
            {
#if CLIENT
                ScenarioCreatorClient.Notify.Error("An error occurred while processing the locations.json file. Teleport Locations and Location Blips will be unavailable. Please correct any errors in the locations.json file.");
#endif
                Debug.WriteLine($"[ScenarioCreator] json exception details: {e.Message}\nStackTrace:\n{e.StackTrace}");
            }

            return data;
        }

        /// <summary>
        /// Gets just the teleport locations data from the locations.json.
        /// </summary>
        /// <returns></returns>
        public static List<TeleportLocation> GetTeleportLocationsData()
        {
            return GetLocations().teleports;
        }

        /// <summary>
        /// Gets just the blips data from the locations.json.
        /// </summary>
        /// <returns></returns>
        public static List<LocationBlip> GetLocationBlipsData()
        {
            return GetLocations().blips;
        }

        /// <summary>
        /// Struct used for deserializing json only.
        /// </summary>
        public struct Locations
        {
            public List<TeleportLocation> teleports;
            public List<LocationBlip> blips;
        }

        /// <summary>
        /// Teleport location struct.
        /// </summary>
        public struct TeleportLocation
        {
            public string name;
            public Vector3 coordinates;
            public float heading;

            public TeleportLocation(string name, Vector3 coordinates, float heading)
            {
                this.name = name;
                this.coordinates = coordinates;
                this.heading = heading;
            }
        }

        /// <summary>
        /// Location blip struct.
        /// </summary>
        public struct LocationBlip
        {
            public string name;
            public Vector3 coordinates;
            public int spriteID;
            public int color;

            public LocationBlip(string name, Vector3 coordinates, int spriteID, int color)
            {
                this.name = name;
                this.coordinates = coordinates;
                this.spriteID = spriteID;
                this.color = color;
            }
        }
        #endregion
    }




}
