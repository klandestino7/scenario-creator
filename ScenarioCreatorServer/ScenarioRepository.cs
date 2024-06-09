using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScenarioCreatorShared;


using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorServer
{
    public class ScenarioRepository
    {
        //database stuff
        private const String SERVER = "localhost";
        private const String DATABASE = "fivem_server";
        private const String UID = "root";
        private const String PASSWORD = "";
        private static MySqlConnection dbConn;

        // User class stuff
        public int Id { get; private set; }

        public String Username { get; private set; }

        public String Password { get; private set; }

        public ScenarioRepository(){
            InitializeDB();
        }

        public static void StopMySQL()
        {
            dbConn.Dispose();
        }

        public static void InitializeDB()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = SERVER;
            builder.UserID = UID;
            builder.Password = PASSWORD;
            builder.Database = DATABASE;

            String connString = builder.ToString();

            builder = null;

            Console.WriteLine(connString);

            dbConn = new MySqlConnection(connString);

            Debug.WriteLine($"MYSQL STATE :: {dbConn.State}");

            dbConn.Close();
        }

        public static int CreateScene( string sceneName )
        {
            String query = string.Format(
                "INSERT INTO scenario (name) VALUES ('{0}')", 
                sceneName
            );
            MySqlCommand cmd = new MySqlCommand(query, dbConn);

            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            int id = (int)cmd.LastInsertedId;
            
            dbConn.Close();

            return id;
        }
        public static List<Scenario> GetAllScenes()
        {
            List<Scenario> _scenarios = new List<Scenario>() { };

            try
            {
                String query = "SELECT * FROM `scenario`";

                if (dbConn.State == System.Data.ConnectionState.Closed)
                {
                    dbConn.Open();
                }

                MySqlCommand cmd = new MySqlCommand(query, dbConn);

                MySqlDataReader reader = cmd.ExecuteReader();

                 while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    String name = reader["name"].ToString();

                    Scenario u = new Scenario(id, name);

                    _scenarios.Add(u);
                }
    
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            dbConn.Close();
            return _scenarios;
        }
        public static Scenario GetScenarioFromId( int sceneId )
        {
            Debug.WriteLine($" GetScenarioFromId :: {sceneId}");
            Scenario _scenario = null;

            String query = string.Format("SELECT * FROM scenario WHERE id={0}", sceneId);

            MySqlCommand cmd = new MySqlCommand(query, dbConn);
    
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = Convert.ToInt32(reader["id"]);
                String name = reader["name"].ToString();

                _scenario = new Scenario(id, name);
            }

            reader.Close();
            dbConn.Close();

            return _scenario;
        }
        public static List<ScenarioPed> GetAllPedsFromScenario( int sceneId )
        {
            List<ScenarioPed> _scenarioPeds = new List<ScenarioPed>();

            try{
                String query = string.Format("SELECT * FROM scenario_peds WHERE scenarioId={0}", sceneId);

                MySqlCommand cmd = new MySqlCommand(query, dbConn);
        
                if (dbConn.State == System.Data.ConnectionState.Closed)
                {
                    dbConn.Open();
                }

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // int id = (int)reader["id"];
                    // int scenarioId = reader["scenarioId"] != DBNull.Value ? Convert.ToInt32(reader["scenarioId"]) : 0;
                    // String model = reader["model"].ToString();
                    // Vector3 position = JsonConvert.DeserializeObject<Vector3>(reader["position"].ToString());
                    // Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(reader["rotation"].ToString());
                    // int outfitVariation = (int)reader["outfitVariation"];
                    // bool isFreezed = (bool)reader["isFreezed"];
                    // bool isInvincible = (bool)reader["isInvincible"];
                    // String scenario = reader["scenarioAnim"] != DBNull.Value ? reader["scenarioAnim"].ToString() : null;
                    // String anim = reader["anim"] != DBNull.Value ? reader["anim"].ToString() : null;
                    // String animDict = reader["animDict"] != DBNull.Value ? reader["animDict"].ToString() : null;
                    // uint flags = reader["flags"] != DBNull.Value ? (uint)reader["flags"] : 0; ;
                    // String relationship = reader["relationship"] != DBNull.Value ? reader["relationship"].ToString() : null;
                    // String weaponModel = reader["weaponModel"] != DBNull.Value ? reader["weaponModel"].ToString() : null;

                    // ScenarioPed u = new ScenarioPed(
                    //     id,
                    //     scenarioId,
                    //     model,
                    //     position,
                    //     rotation,
                    //     outfitVariation,
                    //     isFreezed,
                    //     isInvincible,
                    //     scenario,
                    //     anim,
                    //     animDict,
                    //     flags,
                    //     relationship,
                    //     weaponModel
                    // );

                    // _scenarioPeds.Add(u);
                }

                reader.Close();

            }
            catch (Exception ex) {
    
                Debug.WriteLine(ex.ToString());
            }
            dbConn.Close();

            return _scenarioPeds;
        }
        public static List<ScenarioProp> GetAllPropsFromScenario( int sceneId )
        {
            List<ScenarioProp> _scenarioProps = new List<ScenarioProp>();

            String query = string.Format("SELECT * FROM scenario_props WHERE scenarioId={0}", sceneId);

            MySqlCommand cmd = new MySqlCommand(query, dbConn);
    
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = (int)reader["id"];
                String model = reader["model"].ToString();
                Vector3 position = JsonConvert.DeserializeObject<Vector3>(reader["position"].ToString());
                Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(reader["rotation"].ToString());
                int attachedToPedId = reader["attachedToPedId"] != DBNull.Value ? (int)reader["attachedToPedId"] : 0;
                String attachedMetadata = reader["attachedMetadata"] != DBNull.Value ? reader["attachedMetadata"].ToString() : null;

                ScenarioProp u = new ScenarioProp(
                    id,
                    sceneId,
                    model,
                    position,
                    rotation,
                    attachedToPedId,
                    attachedMetadata
                );

                _scenarioProps.Add(u);
            }

            reader.Close();

            dbConn.Close();

            return _scenarioProps;
        }
         public static List<ScenarioVehicle> GetAllVehiclesFromScenario( int sceneId )
        {
            List<ScenarioVehicle> _scenarioVehicles = new List<ScenarioVehicle>();

            String query = string.Format("SELECT * FROM scenario_vehicles WHERE scenarioId={0}", sceneId);

            MySqlCommand cmd = new MySqlCommand(query, dbConn);
    
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = (int)reader["id"];
                int scenarioId = reader["scenarioId"] != DBNull.Value ? Convert.ToInt32(reader["scenarioId"]) : 0;
                String model = reader["model"].ToString();
                Vector3 position = JsonConvert.DeserializeObject<Vector3>(reader["position"].ToString());
                Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(reader["rotation"].ToString());
                String props = reader["props"].ToString();
                String plate = reader["plate"].ToString();
                int pedDriver = reader["pedDriver"] != DBNull.Value ? (int)reader["pedDriver"] : 0;
                String driverMetadata = reader["driverMetadata"] != DBNull.Value ? reader["driverMetadata"].ToString() : string.Empty;

                ScenarioVehicle u = new ScenarioVehicle(
                    id,
                    scenarioId,
                    model,
                    position,
                    rotation,
                    props,
                    plate,
                    pedDriver,
                    driverMetadata
                );

                _scenarioVehicles.Add(u);
            }

            reader.Close();

            dbConn.Close();

            return _scenarioVehicles;
        }
        public static void AddPropOnDBScene( int sceneId, ScenarioProp prop )
        {
            Debug.WriteLine($" AddPropOnDBScene :: {sceneId}");

            String query = string.Format(
                "INSERT INTO scenario_props (scenarioId, model, position, rotation, attachedToPedId, attachedMetadata) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", 
                sceneId,
                prop.Model,
                JsonConvert.SerializeObject(prop.Position),
                JsonConvert.SerializeObject(prop.Rotation),
                prop.AttachedToPedId,
                prop.AttachedMetadata != null ? JsonConvert.SerializeObject(prop.AttachedMetadata) : ""
            );

            Debug.WriteLine($" query :: {query}");

            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }
        public static void AddVehicleOnDBScene( int sceneId, ScenarioVehicle vehicle )
        {
            Debug.WriteLine($" AddVehicleOnDBScene :: {sceneId}");

            String query = string.Format(
                "INSERT INTO scenario_vehicles (scenarioId, model, position, rotation, props, plate, pedDriver, driverMetadata) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", 
                sceneId,
                vehicle.Model,
                JsonConvert.SerializeObject(vehicle.Position),
                JsonConvert.SerializeObject(vehicle.Rotation),
                vehicle.Props != null ? JsonConvert.SerializeObject(vehicle.Props) : "[]",
                vehicle.Plate,
                vehicle.PedDriver,
                vehicle.PedDriverMetadata != null ? vehicle.PedDriverMetadata : ""
            );

            Debug.WriteLine($" query :: {query}");

            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }
        public static void AddPedOnDBScene( int sceneId, ScenarioPed ped )
        {
            Debug.WriteLine($" AddPedOnDBScene :: {sceneId}");

            String query = string.Format(
                "INSERT INTO scenario_peds (scenarioId, model, position, rotation, outfitVariation, isFreezed, isInvincible, scenarioAnim, anim, animDict, flags, relationship, weaponModel) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}')", 
                sceneId,
                ped.Model,
                JsonConvert.SerializeObject(ped.Position),
                JsonConvert.SerializeObject(ped.Rotation),
                ped.OutfitVariation,
                ped.IsFreezed ? 1 : 0,
                ped.IsInvincible ? 1 : 0,
                ped.Scenario,
                ped.Anim,
                ped.Dict,
                ped.Flags,
                ped.Relationship,
                ped.WeaponModel
            );

            Debug.WriteLine($" query :: {query}");

            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }
        public static void AddVehiclesOnDBScene( int sceneId, List<ScenarioVehicle> vehicles )
        {

            foreach (var vehicle in vehicles)
            {
                AddVehicleOnDBScene( sceneId, vehicle );
            }
        }
        public static void UpdateVehicleFromDBScene( int vehicleId, ScenarioVehicle vehicle )
        {
            String query = string.Format(
                "UPDATE scenario_vehicles SET model='{0}', position='{1}', rotation='{2}', props='{3}', plate='{4}', pedDriver='{5}', driverMetadata='{6}' WHERE vehicleId='{7}'", 
                vehicle.Model,
                JsonConvert.SerializeObject(vehicle.Position),
                JsonConvert.SerializeObject(vehicle.Rotation),
                vehicle.Props != null ? JsonConvert.SerializeObject(vehicle.Props) : "[]",
                vehicle.Plate,
                vehicle.PedDriver,
                vehicle.PedDriverMetadata != null ? vehicle.PedDriverMetadata : "",
                vehicleId
            );
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }
        public static void DeleteVehicleFromDBScene( int vehicleId )
        {
            String query = string.Format("DELETE FROM scenario_vehicles WHERE vehicleId='{0}'", vehicleId );
            QueryExecute(query);
        }

        private static MySqlCommand QueryExecute( string query )
        {
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }

            cmd.ExecuteNonQuery();
            dbConn.Close();

            return cmd;
        }
    }
}