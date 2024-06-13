using System;
using System.Collections.Generic;

using CitizenFX.Core;

using Newtonsoft.Json;


using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorShared
{
    public static class Globals
    {
        /// <summary>
        /// The name of the script
        /// </summary>
        public const string ScriptName = "ScenarioCreator";

        /// <summary>
        /// The prefix used for key-value pairs used to store personal presets
        /// </summary>
        public const string KvpPrefix = "scenariocreator_";

        /// <summary>
        /// The expected name of the resource
        /// </summary>
        public const string ResourceName = "scenario";

        /// <summary>
        /// The prefix used for commands exposed by the script
        /// </summary>
        public const string CommandPrefix = "scenariocreator_";

        public enum eEntityTypeToClass  {
            EntityPed = 1,
            EntityVehicle = 2,
            EntityProp = 3
        }
    }
}
