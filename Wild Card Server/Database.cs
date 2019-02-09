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

    class Database
    {




        //ArrayList tables = new ArrayList { "attack_cards", "heal_cards", "item_cards" };

        public static void NewAccount(string username, string password)
        {
            string query = "INSERT INTO accounts(username,password,gold,level,exp) VALUES('" +
                username +
                "','" + EncryptPassword(password) +
                "','" + 50 +        //Start gold of the player
                "','" + 1 +         //Start lvl
                "','" + 0 + "')";    //Start exp

            MySqlCommand cmd = new MySqlCommand(query, MySQL.mySQLSettings.connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            Console.WriteLine("Account '{0}' was succesfully created", username);

        }

        public static bool AccountExist(string username)
        {
            string query = "SELECT username FROM accounts WHERE username='" + username + "';";

            MySqlCommand cmd = new MySqlCommand(query, MySQL.mySQLSettings.connection);
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

            if (reader.HasRows)
            {
                reader.Close();
                return true;
            }
            reader.Close();
            return false;

        }

        public static bool PasswordOK(string username, string password)
        {
            string query = "SELECT password from accounts WHERE username='" + username + "'";
            MySqlCommand cmd = new MySqlCommand(query, MySQL.mySQLSettings.connection);
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

            if (EncryptPassword(password) == tempPass) return true;

            return false;
        }

        public static string EncryptPassword(string password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return Encoding.ASCII.GetString(data);
        }



        public static string GetCardType(MySqlConnection connection, int cardID)
        {
            string query = "SELECT type from cards WHERE id='" + cardID + "'";
            MySqlCommand cmd = new MySqlCommand(query, connection);
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
            string type = "";
            while (reader.Read())
            {
                type = (string)reader["type"];
            }
            reader.Close();

            return type;
        }

        //TODO Change Method with Connection Attribute for multople Reading
        public static ByteBuffer GetAttackCardInfo(MySqlConnection connection, int cardID)
        {
            //TEMPORARY:
            string query = "SELECT * from attack_cards WHERE id='" + cardID + "'";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader;
            ByteBuffer buffer = new ByteBuffer();

            try
            {

                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                throw;
            }

            int damage = 0;
            int bullets = 0;

            while (reader.Read())
            {
                damage = (int)reader["damage"];
                bullets = (int)reader["bullets"];
            }

            reader.Close();

            Console.WriteLine("Card with ID '{0}' has {1} damage and {2} bullets", cardID, damage, bullets);

            //buffer.WriteInteger(cardID);
            //buffer.WriteInteger((int)CardTypes.Attack);
            buffer.WriteInteger(damage);
            buffer.WriteInteger(bullets);

            //TEMPORARY TESTING!!:
            buffer.WriteString("AddBullet");
            buffer.WriteInteger(1);

            return buffer;


        }

        public static ArrayList TakeRandomAttackCards(MySqlConnection connection, int numberOfCards)
        {
            //Console.WriteLine("OPEN READER");
            ArrayList resultCards = new ArrayList();
            string query = "SELECT * from attack_cards ORDER BY RAND() LIMIT " + numberOfCards + "";
            MySqlCommand cmd = new MySqlCommand(query, connection);
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
                uint cardID = (uint)reader["id"];
                resultCards.Add((int)cardID);
                //Console.WriteLine("Card with ID '{0}' was received", cardID);
            }

            reader.Close();
            //Console.WriteLine("CLOSE READER");
            return resultCards;
        }


        public static ArrayList TakeRandomCardsOfEachType(MySqlConnection connection, int numberOfCards = 3)
        {


            if (numberOfCards % 3 != 0 || numberOfCards <= 0)
            {
                Console.WriteLine("Wrong Number Of Cards, it must be multiple of 3, changed to right nearest up value");
                numberOfCards += 3 - numberOfCards % 3;
            }

            int numberOfPacks = numberOfCards / 3; //Number of cards of each type

            //Console.WriteLine("OPEN READER");
            ArrayList resultCards = new ArrayList();
            string query = "(SELECT * from attack_cards ORDER BY RAND() LIMIT " + numberOfPacks + ") UNION" +
                "(SELECT * from heal_cards ORDER BY RAND() LIMIT " + numberOfPacks + ") UNION" +
                "(SELECT * from item_cards ORDER BY RAND() LIMIT " + numberOfPacks + ")";
            MySqlCommand cmd = new MySqlCommand(query, connection);
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
                uint cardID = (uint)reader["id"];
                resultCards.Add((int)cardID);
               // Console.WriteLine("Card with ID '{0}' was received", cardID);
            }

            reader.Close();
            //Console.WriteLine("CLOSE READER");
            return resultCards;
        }


        public static ArrayList TakeAllCardsWithInformation(MySqlConnection connection)
        {

            
            //Console.WriteLine("OPEN READER");
            ArrayList resultCards = new ArrayList();
            string query = "(SELECT * from attack_cards)" +  "UNION" + 
                "(SELECT * from heal_cards)" + "UNION" +
                "(SELECT * from item_cards)";
            MySqlCommand cmd = new MySqlCommand(query, connection);
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
                uint cardID = (uint)reader["id"];
                int damage = (int)reader["damage"];
                int bullets = (int)reader["bullets"];
                string image = (string)reader["image"];
                resultCards.Add((int)cardID);
                resultCards.Add(damage);
                resultCards.Add(bullets);
                resultCards.Add(image);

                //Console.WriteLine("Card with ID '{0}' was received", cardID);
            }

            reader.Close();
            //Console.WriteLine("CLOSE READER");
            return resultCards;
        }


    }
}
