using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorClient
{
    internal class Utils : BaseScript
    {
        #region Variables

        private const float RayDistance = 25f;
        #endregion


        #region Methods
        /// <summary>
        /// Used internally for getting direction vector from rotation vector
        /// </summary>
        /// <param name="rotation">Input rotation vector</param>
        /// <returns>Output direction vector</returns>
        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var adj = new Vector3(
                (float)Math.PI / 180f * rotation.X,
                (float)Math.PI / 180f * rotation.Y,
                (float)Math.PI / 180f * rotation.Z
            );

            return new Vector3(
                (float)(-Math.Sin(adj.Z) * Math.Abs(Math.Cos(adj.X))),
                (float)(Math.Cos(adj.Z) * Math.Abs(Math.Cos(adj.X))),
                (float)Math.Sin(adj.X)
            );
        }
        public static async Task<bool> LoadAnimDict( string animDict ) 
        {
            RequestAnimDict(animDict);
            
            while (!HasAnimDictLoaded(animDict))
            {
                await Delay(1);
            }

            return true;
        }
        public static async Task<bool> LoadEntityModel( uint hashModel ) 
        {

            if (!IsModelValid(hashModel))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return false;
            }

            RequestModel(hashModel);
            
            while (!HasModelLoaded(hashModel))
            {
                await Delay(1);
            }

            return true;
        }

        public static RaycastResult GetPlayerRayCastResult() 
        {
            var camRotation = GetGameplayCamRot(0);
            var camCoords = GetGameplayCamCoord();
            var camDirection = RotationToDirection(camRotation);

            var dest = new Vector3(
                camCoords.X + (camDirection.X * RayDistance),
                camCoords.Y + (camDirection.Y * RayDistance),
                camCoords.Z + (camDirection.Z * RayDistance)
            );
            
#if DEBUG
            DrawLine(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, dest.X, dest.Y, dest.Z, 255, 0, 0, 255);
#endif

            return World.Raycast(camCoords, dest, IntersectOptions.Everything, Game.PlayerPed);
        } 

        /// <summary>
        /// Used to get coords of reycast from player camera;
        /// </summary>
        /// <returns>destination if no hit was found and coords of hit if there was one</returns>
        public static Vector3 GetCoordsPlayerIsLookingAt()
        {
            var res = GetPlayerRayCastResult();

            var camRotation = GetGameplayCamRot(0);
            var camCoords = GetGameplayCamCoord();
            var camDirection = RotationToDirection(camRotation);

            var dest = new Vector3(
                camCoords.X + (camDirection.X * RayDistance),
                camCoords.Y + (camDirection.Y * RayDistance),
                camCoords.Z + (camDirection.Z * RayDistance)
            );

            return res.DitHit ? res.HitPosition : dest;
        }

        /// <summary>
        /// Print data to the console and save it to the CitizenFX.log file. Only when vMenu debugging mode is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(string data)
        {
            if (MainScript.DebugMode)
            {
                // Debug.WriteLine(@data);
            }
        }

        #endregion
    }

}
