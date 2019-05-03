using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Collections;



namespace Wild_Card_Server
{

    class DatabaseManager
    {

        #region Authorization/Registration
        public static bool IsAccountExist(string username)
        {
            string query = "SELECT username FROM accounts WHERE username='" + username + "';";

            MySqlCommand cmd = new MySqlCommand(query, MySQLConnector.MySQLSettings.connection);
            MySqlDataReader reader;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            var result = reader.HasRows;
            reader.Close();
            return result;
        }
        public static bool IsCorrectPassword(string username, string password)
        {
            string query = "SELECT password from accounts WHERE username='" + username + "'";
            MySqlCommand cmd = new MySqlCommand(query, MySQLConnector.MySQLSettings.connection);
            MySqlDataReader reader;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            string tempPass = string.Empty;
            while (reader.Read())
            {
                tempPass = reader["password"] + "";
            }
            reader.Close();

            return EncryptPassword(password) == tempPass;
        }
        private static string EncryptPassword(string password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return Encoding.ASCII.GetString(data);
        }
        #endregion

        #region GetCards
        public static Dictionary<int, DbInstanceCard> GetAllCards()
        {
            var result = new Dictionary<int, DbInstanceCard>();

            var query = "SELECT * from Cards";
            var connection = new MySqlConnection(MySQLConnector.CreateConnectionString());

            connection.Open();

            var cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader;

            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
           
            while (reader.Read())
            {
                DbInstanceCard tempCard = new DbInstanceCard();

                tempCard.Id = (int)reader["id"];
                tempCard.Type = (string)reader["type"];
                tempCard.Name = (string)reader["name"];
                tempCard.IsComboCard = (int)reader["isComboCard"] != 0;
                tempCard.NForCombo = (int)reader["nForComboCard"];
                tempCard.ComboCards = new List<int>();
                for (int i = 1; i <= tempCard.NForCombo; i++)
                {
                    var tempString = "comboCard" + i;
                    tempCard.ComboCards.Add((int)reader[tempString]);
                }

                tempCard.ComboCards.Sort();

                tempCard.CardImage = (string)reader["cardImage"];
                tempCard.ItemImage = (string)reader["itemImage"];
                tempCard.Value = (int)reader["value"];
                tempCard.Animation = (string)reader["animation"];

                result[tempCard.Id] = tempCard;
            }

            reader.Close();
            connection.Close();

            return result;

        }
        public static Dictionary<List<int>, DbInstanceCard> GetCombo4Cards()
        {
            var query = "SELECT * from Cards WHERE isComboCard = 1 AND nForComboCard = 4";
            return GetComboCardsWithQuery(query);
        }
        public static Dictionary<List<int>, DbInstanceCard> GetCombo3Cards()
        {
            var query = "SELECT * from Cards WHERE isComboCard = 1 AND nForComboCard = 3";
            return GetComboCardsWithQuery(query);
        }
        public static Dictionary<List<int>, DbInstanceCard> GetCombo2Cards()
        {
            var query = "SELECT * from Cards WHERE isComboCard = 1 AND nForComboCard = 2";
            return GetComboCardsWithQuery(query);
        }
        private static Dictionary<List<int>, DbInstanceCard> GetComboCardsWithQuery(string query)
        {

            var comparer = new Constants.ListComparer<int>();
            var result = new Dictionary<List<int>, DbInstanceCard>(comparer);

            var connection = new MySqlConnection(MySQLConnector.CreateConnectionString());

            connection.Open();

            var cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader;

            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


            //TO DO: To be sure that it's everything that we need in 'Card' class
            while (reader.Read())
            {
                DbInstanceCard tempCard = new DbInstanceCard();


                tempCard.Id = (int)reader["id"];
                tempCard.Type = (string)reader["type"];
                tempCard.Name = (string)reader["name"];
                tempCard.IsComboCard = (int)reader["isComboCard"] != 0;
                tempCard.NForCombo = (int)reader["nForComboCard"];
                tempCard.ComboCards = new List<int>();
                for (int i = 1; i <= tempCard.NForCombo; i++)
                {
                    var tempString = "comboCard" + i;
                    int cardID = (int)reader[tempString];
                    tempCard.ComboCards.Add(cardID);

                }
                tempCard.ComboCards.Sort();


                tempCard.CardImage = (string)reader["cardImage"];
                tempCard.ItemImage = (string)reader["itemImage"];
                tempCard.Value = (int)reader["value"];
                tempCard.Animation = (string)reader["animation"];


                result[tempCard.ComboCards] = tempCard;
            }

            reader.Close();
            connection.Close();

            return result;
        }
        #endregion

    }
}
