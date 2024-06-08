using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

using CitizenFX.Core;
using CitizenFX.Core.Native;


using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;

namespace ScenarioCreatorServer
{
    public class ScenarioRepository : BaseScript
    {


        //database stuff
        private const String SERVER = "127.0.0.1";
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


        private ScenarioRepository(int id, String u, String p)
        {
            Id = id;
            Username = u;
            Password = p;
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

        }

        public static List<ScenarioRepository> GetUsers()
        {
            List<ScenarioRepository> users = new List<ScenarioRepository>();

            String query = "SELECT * FROM users";

            MySqlCommand cmd = new MySqlCommand(query, dbConn);

            dbConn.Open();

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = (int)reader["id"];
                String username = reader["username"].ToString();
                String password = reader["password"].ToString();

                ScenarioRepository u = new ScenarioRepository(id, username, password);

                users.Add(u);
            }

            reader.Close();

            dbConn.Close();

            return users;
        }

        public static ScenarioRepository Insert(String u, String p)
        {
            String query = string.Format("INSERT INTO users(username, password) VALUES ('{0}', '{1}')", u, p);

            MySqlCommand cmd = new MySqlCommand(query, dbConn);

            dbConn.Open();

            cmd.ExecuteNonQuery();
            int id = (int)cmd.LastInsertedId;

            ScenarioRepository user = new ScenarioRepository(id, u, p);

            dbConn.Close();

            return user;

        }
        public void Update(string u, string p)
        {
            String query = string.Format("UPDATE users SET username='{0}', password='{1}' WHERE ID={2}", u, p, Id);

            MySqlCommand cmd = new MySqlCommand(query, dbConn);

            dbConn.Open();

            cmd.ExecuteNonQuery();

            dbConn.Close();
        }

        public void Delete()
        {
            String query = string.Format("DELETE FROM users WHERE ID={0}", Id);

            MySqlCommand cmd = new MySqlCommand(query, dbConn);

            dbConn.Open();

            cmd.ExecuteNonQuery();

            dbConn.Close();
        }
    }
}