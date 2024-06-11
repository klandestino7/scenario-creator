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
        private static String SERVER = GetConvar("mysql_host", "localhost");
        private static String DATABASE = GetConvar("mysql_database", "fivem_server");
        private static String UID = GetConvar("user", "root");
        private static String PASSWORD = GetConvar("password", "");
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

            // Debug.WriteLine($"MYSQL STATE :: {dbConn.State}");

            dbConn.Close();
        }

        public static int CreateScene( string sceneName, string position )
        {
            String query = string.Format(
                "INSERT INTO scenario (name, defaultPosition) VALUES ('{0}', '{1}')", 
                sceneName,
                position
            );

            // Debug.WriteLine($" query :: {query}");
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
                    Vector3 position = JsonConvert.DeserializeObject<Vector3>(reader["defaultPosition"].ToString());

                    Scenario u = new Scenario(id, name, position);

                    _scenarios.Add(u);
                }
    
                reader.Close();
            }
            catch (Exception ex)
            {
                // Debug.WriteLine(ex.ToString());
            }

            dbConn.Close();
            return _scenarios;
        }
        public static Scenario GetScenarioFromId( int sceneId )
        {
            // Debug.WriteLine($" GetScenarioFromId :: {sceneId}");
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
                Vector3 position = JsonConvert.DeserializeObject<Vector3>(reader["defaultPosition"].ToString());

                _scenario = new Scenario(id, name, position);
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

                    // Debug.WriteLine($" MySqlDataReader :: {reader}");
                while (reader.Read())
                {
                    
                    int id = (int)reader["id"];
                    int scenarioId = reader["scenarioId"] != DBNull.Value ? Convert.ToInt32(reader["scenarioId"]) : 0;
                    String model = reader["model"] != DBNull.Value ? reader["model"].ToString() : null; 
                    Vector3 position = JsonConvert.DeserializeObject<Vector3>(reader["position"].ToString());
                    Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(reader["rotation"].ToString());
                    int outfitVariation = (int)reader["outfitVariation"];
                    // bool isFreezed = (int)reader["isFreezed"] == 1;
                    // bool isInvincible = (int)reader["isInvincible"] == 1;
                    String scenario = reader["scenarioAnim"] != DBNull.Value ? reader["scenarioAnim"].ToString() : null;
                    String anim = reader["anim"] != DBNull.Value ? reader["anim"].ToString() : null;
                    String animDict = reader["animDict"] != DBNull.Value ? reader["animDict"].ToString() : null;
                    String flags = reader["flags"] != DBNull.Value ? reader["flags"].ToString() : null;
                    String relationship = reader["relationship"] != DBNull.Value ? reader["relationship"].ToString() : null;
                    String weaponModel = reader["weaponModel"] != DBNull.Value ? reader["weaponModel"].ToString() : null;

                    ScenarioPed u = new ScenarioPed(
                        id,
                        scenarioId,
                        model,
                        position,
                        rotation,
                        outfitVariation,
                        false, // isFreezed
                        false, // isInvincible
                        scenario,
                        anim,
                        animDict,
                        0, // flags
                        relationship,
                        weaponModel
                    );

                    _scenarioPeds.Add(u);
                }

                reader.Close();

            }
            catch (Exception ex) {
    
                // Debug.WriteLine(ex.ToString());
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
                _AttachedMetadata attachedMetadata = JsonConvert.DeserializeObject<_AttachedMetadata>(reader["attachedMetadata"].ToString());

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
                _PedDriverMetadata driverMetadata = JsonConvert.DeserializeObject<_PedDriverMetadata>(reader["driverMetadata"].ToString());

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
        public static int AddPropOnDBScene( int sceneId, ScenarioProp prop )
        {
            // Debug.WriteLine($" AddPropOnDBScene :: {sceneId}");

            String query = string.Format(
                "INSERT INTO scenario_props (scenarioId, model, position, rotation, attachedToPedId, attachedMetadata) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", 
                sceneId,
                prop.Model,
                JsonConvert.SerializeObject(prop.Position),
                JsonConvert.SerializeObject(prop.Rotation),
                prop.AttachedToPedId,
                prop.AttachedMetadata != null ? JsonConvert.SerializeObject(prop.AttachedMetadata) : ""
            );

            // Debug.WriteLine($" query :: {query}");

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
        public static int AddVehicleOnDBScene( int sceneId, ScenarioVehicle vehicle )
        {
            // Debug.WriteLine($" AddVehicleOnDBScene :: {sceneId}");

            String query = string.Format(
                "INSERT INTO scenario_vehicles (scenarioId, model, position, rotation, props, plate, pedDriver, driverMetadata) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", 
                sceneId,
                vehicle.Model,
                JsonConvert.SerializeObject(vehicle.Position),
                JsonConvert.SerializeObject(vehicle.Rotation),
                vehicle.Props != null ? JsonConvert.SerializeObject(vehicle.Props) : "[]",
                vehicle.Plate,
                vehicle.PedDriver,
                vehicle.PedDriverMetadata != null ? JsonConvert.SerializeObject(vehicle.PedDriverMetadata) : ""
            );

            // Debug.WriteLine($" query :: {query}");

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
        public static int AddPedOnDBScene( int sceneId, ScenarioPed ped )
        {
            // Debug.WriteLine($" AddPedOnDBScene :: {sceneId}");

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

            // Debug.WriteLine($" query :: {query}");

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
                "UPDATE scenario_vehicles SET model='{0}', position='{1}', rotation='{2}', props='{3}', plate='{4}', pedDriver='{5}', driverMetadata='{6}' WHERE id='{7}'", 
                vehicle.Model,
                JsonConvert.SerializeObject(vehicle.Position),
                JsonConvert.SerializeObject(vehicle.Rotation),
                vehicle.Props != null ? JsonConvert.SerializeObject(vehicle.Props) : "[]",
                vehicle.Plate,
                vehicle.PedDriver,
                vehicle.PedDriverMetadata != null ? JsonConvert.SerializeObject(vehicle.PedDriverMetadata) : "",
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
        public static void UpdatePropFromDBScene( int propId, ScenarioProp prop )
        {
            String query = string.Format(
                "UPDATE scenario_props SET position='{0}', rotation='{1}', attachedToPedId='{2}', attachedMetadata='{3}' WHERE id={4}", 
                JsonConvert.SerializeObject(prop.Position),
                JsonConvert.SerializeObject(prop.Rotation),
                prop.AttachedToPedId,
                prop.AttachedMetadata != null ? JsonConvert.SerializeObject(prop.AttachedMetadata) : "",
                propId
            );
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }
        public static void UpdatePedFromDBScene( int pedId, ScenarioPed ped )
        {
            String query = string.Format(
                "UPDATE scenario_peds SET  position='{0}', rotation='{1}', isFreezed='{2}', isInvincible='{3}', scenarioAnim='{4}', anim='{5}', animDict='{6}', flags='{7}', relationship='{8}', weaponModel='{9}' WHERE id='{10}'", 
                JsonConvert.SerializeObject(ped.Position),
                JsonConvert.SerializeObject(ped.Rotation),
                ped.IsFreezed ? 1 : 0,
                ped.IsInvincible ? 1 : 0,
                ped.Scenario,
                ped.Anim,
                ped.Dict,
                ped.Flags != null ? ped.Flags : "0",
                ped.Relationship,
                ped.WeaponModel,
                pedId
            );
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }
            cmd.ExecuteNonQuery();
            dbConn.Close();
        }
        public static void DeleteSceneFromDB( int sceneId )
        {
            String query = string.Format("DELETE FROM scenario WHERE id='{0}'", sceneId );
            QueryExecute(query);
        }
        public static void DeleteVehicleFromDBScene( int vehicleId )
        {
            String query = string.Format("DELETE FROM scenario_vehicles WHERE id='{0}'", vehicleId );
            QueryExecute(query);
        }
        public static void DeletePropFromDBScene( int propId )
        {
            String query = string.Format("DELETE FROM scenario_props WHERE id='{0}'", propId );
            QueryExecute(query);
        }
        public static void DeletePedFromDBScene( int pedId )
        {
            String query = string.Format("DELETE FROM scenario_peds WHERE id='{0}'", pedId );
            QueryExecute(query);
        }

         public static void UpdateEntityWorldPosition( int entityId, string entityTable, string position, string rotation )
        {
            String query = string.Format(
                "UPDATE {0} SET  position='{1}', rotation='{2}' WHERE id='{3}'", 
                entityTable,
                position,
                rotation,
                entityId
            );

            MySqlCommand cmd = new MySqlCommand(query, dbConn);

            if (dbConn.State == System.Data.ConnectionState.Closed)
            {
                dbConn.Open();
            }

            cmd.ExecuteNonQuery();
            dbConn.Close();
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