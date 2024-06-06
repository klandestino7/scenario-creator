using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;

using ScenarioCreatorShared;

using static CitizenFX.Core.Native.API;
using static ScenarioCreatorServer.DebugLog;
using static ScenarioCreatorShared.ConfigManager;

namespace ScenarioCreatorServer
{

    public static class DebugLog
    {
        public enum LogLevel
        {
            error = 1,
            success = 2,
            info = 4,
            warning = 3,
            none = 0
        }

        /// <summary>
        /// Global log data function, only logs when debugging is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(dynamic data, LogLevel level = LogLevel.none)
        {
            if (MainServer.DebugMode || level == LogLevel.error || level == LogLevel.warning)
            {
                var prefix = "[ScenarioCreator] ";
                if (level == LogLevel.error)
                {
                    prefix = "^1[ScenarioCreator] [ERROR]^7 ";
                }
                else if (level == LogLevel.info)
                {
                    prefix = "^5[ScenarioCreator] [INFO]^7 ";
                }
                else if (level == LogLevel.success)
                {
                    prefix = "^2[ScenarioCreator] [SUCCESS]^7 ";
                }
                else if (level == LogLevel.warning)
                {
                    prefix = "^3[ScenarioCreator] [WARNING]^7 ";
                }
                Debug.WriteLine($"{prefix}[DEBUG LOG] {data.ToString()}");
            }
        }
    }

    public class MainServer : BaseScript
    {
        #region vars
        // Debug shows more information when doing certain things. Leave it off to improve performance!
        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "server_debug_mode", 0) == "true";

        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainServer()
        {
            // name check
            if (GetCurrentResourceName() != "ScenarioCreator")
            {
                var InvalidNameException = new Exception("\r\n\r\n^1[ScenarioCreator] INSTALLATION ERROR!\r\nThe name of the resource is not valid. " +
                    "Please change the folder name from '^3" + GetCurrentResourceName() + "^1' to '^2ScenarioCreator^1' (case sensitive) instead!\r\n\r\n\r\n^7");
                try
                {
                    throw InvalidNameException;
                }
                catch (Exception e)
                {
                    Debug.Write(e.Message);
                }
            }
            else
            {
                // Add event handlers.
                EventHandlers.Add("ScenarioCreator:GetPlayerIdentifiers", new Action<int, NetworkCallbackDelegate>((TargetPlayer, CallbackFunction) =>
                {
                    var data = new List<string>();
                    Players[TargetPlayer].Identifiers.ToList().ForEach(e =>
                    {
                        if (!e.Contains("ip:"))
                        {
                            data.Add(e);
                        }
                    });
                    CallbackFunction(JsonConvert.SerializeObject(data));
                }));
                EventHandlers.Add("ScenarioCreator:RequestPermissions", new Action<Player>(PermissionsManager.SetPermissionsForPlayer));
                EventHandlers.Add("ScenarioCreator:RequestServerState", new Action<Player>(RequestServerStateFromPlayer));

                // check addons file for errors
                var addons = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                try
                {
                    JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
                    // If the above crashes, then the json is invalid and it'll throw warnings in the console.
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine($"\n\n^1[ScenarioCreator] [ERROR] ^7Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
                }

                // check if permissions are setup (correctly)
                if (!GetSettingsBool(Setting.ScenarioCreator_use_permissions))
                {
                    Debug.WriteLine("^3[ScenarioCreator] [WARNING] ScenarioCreator is set up to ignore permissions!\nIf you did this on purpose then you can ignore this warning.\nIf you did not set this on purpose, then you must have made a mistake while setting up ScenarioCreator.\nPlease read the ScenarioCreator documentation (^5https://docs.vespura.com/ScenarioCreator^3).\nMost likely you are not executing the permissions.cfg (correctly).^7");
                }

                Tick += PlayersFirstTick;

                // Start the loops
                if (GetSettingsBool(Setting.ScenarioCreator_enable_weather_sync))
                {
                    Tick += WeatherLoop;
                }

                if (GetSettingsBool(Setting.ScenarioCreator_enable_time_sync))
                {
                    Tick += TimeLoop;
                }
            }
        }
        #endregion

        #region command handler
        [Command("scenario", Restricted = true)]
        internal void ServerCommandHandler(int source, List<object> args, string _)
        {
            if (args != null)
            {
                if (args.Count > 0)
                {
                    if (args[0].ToString().ToLower() == "debug")
                    {
                        DebugMode = !DebugMode;
                        if (source < 1)
                        {
                            Debug.WriteLine($"Debug mode is now set to: {DebugMode}.");
                        }
                        else
                        {
                            Players[source].TriggerEvent("chatMessage", $"ScenarioCreator Debug mode is now set to: {DebugMode}.");
                        }
                        return;
                    }
                    else if (args[0].ToString().ToLower() == "help")
                    {
                        Debug.WriteLine("Available commands:");
                        Debug.WriteLine("(server console only): ScenarioCreatorserver ban <id|name> <server id|username> <reason> (player must be online!)");
                        Debug.WriteLine("(server console only): ScenarioCreatorserver unban <uuid>");
                        Debug.WriteLine("ScenarioCreatorserver weather <new weather type | dynamic <true | false>>");
                        Debug.WriteLine("ScenarioCreatorserver time <freeze|<hour> <minute>>");
                        Debug.WriteLine("ScenarioCreatorserver migrate (This copies all banned players in the bans.json file to the new ban system in ScenarioCreator v3.3.0, you only need to do this once)");
                    }
                    else
                    {
                        Debug.WriteLine($"ScenarioCreator is currently running version: {Version}. Try ^5ScenarioCreatorserver help^7 for info.");
                    }
                }
                else
                {
                    Debug.WriteLine($"ScenarioCreator is currently running version: {Version}. Try ^5ScenarioCreatorserver help^7 for info.");
                }
            }
            else
            {
                Debug.WriteLine($"ScenarioCreator is currently running version: {Version}. Try ^5ScenarioCreatorserver help^7 for info.");
            }
        }
        #endregion

        #region Player join/quit
        private readonly HashSet<string> joinedPlayers = new();

        private Task PlayersFirstTick()
        {
            Tick -= PlayersFirstTick;

            foreach (var player in Players)
            {
                joinedPlayers.Add(player.Handle);
            }

            return Task.FromResult(0);
        }

        [EventHandler("playerJoining")]
        internal void OnPlayerJoining([FromSource] Player sourcePlayer)
        {
            joinedPlayers.Add(sourcePlayer.Handle);

            foreach (var player in Players)
            {
                if (IsPlayerAceAllowed(player.Handle, "ScenarioCreator.MiscSettings.JoinQuitNotifs") ||
                    IsPlayerAceAllowed(player.Handle, "ScenarioCreator.MiscSettings.All"))
                {
                    player.TriggerEvent("ScenarioCreator:PlayerJoinQuit", sourcePlayer.Name, null);
                }
            }
        }

        [EventHandler("playerDropped")]
        internal void OnPlayerDropped([FromSource] Player sourcePlayer, string reason)
        {
            if (!joinedPlayers.Contains(sourcePlayer.Handle))
            {
                return;
            }

            joinedPlayers.Remove(sourcePlayer.Handle);

            foreach (var player in Players)
            {
                if (IsPlayerAceAllowed(player.Handle, "ScenarioCreator.MiscSettings.JoinQuitNotifs") ||
                    IsPlayerAceAllowed(player.Handle, "ScenarioCreator.MiscSettings.All"))
                {
                    player.TriggerEvent("ScenarioCreator:PlayerJoinQuit", sourcePlayer.Name, reason);
                }
            }
        }
        #endregion
    }
}
