using System;
using MySql.Data.MySqlClient;


namespace Wild_Card_Server
{
    public class MySQLConnector
    {
        public static MySQLSettings MySQLSettings;

        public static void ConntectToMySQL()
        {
            MySQLSettings.connection = new MySqlConnection(CreateConnectionString());
            ConnectToMySQLServer();
        }

        private static void ConnectToMySQLServer()
        {
            try
            {
                MySQLSettings.connection.Open();
                Console.WriteLine("Succesfully connected to MySQL Server '{0}'",MySQLSettings.database);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public static void CloseConnection()
        {
            MySQLSettings.connection.Close();
        }

        public static string CreateConnectionString()
        {
            var db = MySQLSettings;
            string connectionString = "SERVER=" + db.server + ";" +
                "DATABASE=" + db.database + ";" +
                "UID=" + db.user + ";" +
                "PASSWORD=" + db.password + ";";
            return connectionString;
        }
    }

    public struct MySQLSettings
    {
        public MySqlConnection connection;
        public string server;
        public string database;
        public string user;
        public string password;
    }
}

