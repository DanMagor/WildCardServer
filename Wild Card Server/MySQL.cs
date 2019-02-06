using System;
using MySql.Data.MySqlClient;


namespace Wild_Card_Server
{
    public class MySQL
    {
        public static MySQLSettings mySQLSettings;

        public static void ConntectToMySQL()
        {
            mySQLSettings.connection = new MySqlConnection(CreateConnectionString());
            ConnectToMySQLServer();
        }

        public static void ConnectToMySQLServer()
        {
            try
            {
                mySQLSettings.connection.Open();
                Console.WriteLine("Succesfully connected to MySQL Server '{0}'",mySQLSettings.database);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public static void CloseConnection()
        {
            mySQLSettings.connection.Close();
        }

        public static string CreateConnectionString()
        {
            var db = mySQLSettings;
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

