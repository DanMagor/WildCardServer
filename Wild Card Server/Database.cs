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
            string query = "SELECT type from Cards WHERE id='" + cardID + "'";
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


        public static ByteBuffer GetAttackCard(MySqlConnection connection, int cardID)
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

            //General Parameters
            int damage = 0, bullets = 0, accuracy = 0;
            string name = "", image = "";

            //Initiative Effect information
            string initiativeName = "", initiativeEffect = "";
            int initiativeValue = 0, initiativeDuration = 0;

            //Additional Effects Information
            string additionalEffectName = "", additionalEffect = "";
            int addtionalEffectValue = 0, additionalEffectDuration = 0;



            while (reader.Read())
            {
                damage = (int)reader["damage"];
                bullets = (int)reader["bullets"];
                accuracy = (int)reader["accuracy"];
                name = (string)reader["name"];
                image = (string)reader["image"];


                initiativeName = (string)reader["initiativeName"];
                initiativeEffect = (string)reader["initiativeEffect"];
                initiativeValue = (int)reader["initiativeValue"];
                initiativeDuration = (int)reader["initiativeDuration"];


                additionalEffectName = (string)reader["additionalEffectName"];
                additionalEffect = (string)reader["additionalEffect"];
                addtionalEffectValue = (int)reader["additionalEffectValue"];
                additionalEffectDuration = (int)reader["additionalEffectDuration"];
            }

            reader.Close();

            Console.WriteLine("Card with ID '{0}' has {1} damage and {2} bullets", cardID, damage, bullets);


            buffer.WriteInteger(damage);
            buffer.WriteInteger(bullets);
            buffer.WriteInteger(accuracy);
            buffer.WriteString(name);
            buffer.WriteString(image);

            buffer.WriteString(initiativeName);
            buffer.WriteString(initiativeEffect);
            buffer.WriteInteger(initiativeValue);
            buffer.WriteInteger(initiativeDuration);

            buffer.WriteString(additionalEffectName);
            buffer.WriteString(additionalEffect);
            buffer.WriteInteger(addtionalEffectValue);
            buffer.WriteInteger(additionalEffectDuration);

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

            int numberOfPacks = numberOfCards / 3; //Number of Cards of each type

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
            string query = "(SELECT * from attack_cards)"; //+ "UNION" +
            //    "(SELECT * from heal_cards)" + "UNION" +
            //    "(SELECT * from item_cards)";
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

        #region GetCards
        public static Dictionary<int, Card> GetAllCards()
        {
            var result = new Dictionary<int, Card>();

            var query = "SELECT * from Cards";
            var connection = new MySqlConnection(MySQL.CreateConnectionString());

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
                Card tempCard = new Card();

                tempCard.ID = (int)reader["id"];
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

                result[tempCard.ID] = tempCard;
            }

            reader.Close();
            connection.Close();

            return result;

        }
        public static Dictionary<List<int>, Card> GetCombo4Cards()
        {
            var query = "SELECT * from Cards WHERE isComboCard = 1 AND nForComboCard = 4";
            return GetComboCardsWithQuery(query);
        }
        public static Dictionary<List<int>, Card> GetCombo3Cards()
        {
            var query = "SELECT * from Cards WHERE isComboCard = 1 AND nForComboCard = 3";
            return GetComboCardsWithQuery(query);
        }
        public static Dictionary<List<int>, Card> GetCombo2Cards()
        {
            var query = "SELECT * from Cards WHERE isComboCard = 1 AND nForComboCard = 2";
            return GetComboCardsWithQuery(query);
        }


        private static Dictionary<List<int>, Card> GetComboCardsWithQuery(string query)
        {

            var comparer = new Constants.ListComparer<int>();
            var result = new Dictionary<List<int>, Card>(comparer);



            var connection = new MySqlConnection(MySQL.CreateConnectionString());

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
                Card tempCard = new Card();


                tempCard.ID = (int)reader["id"];
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



        public static Dictionary<int,AttackCard> GetAllAttackCards()
        {

            var result = new Dictionary<int, AttackCard>();


            var query = "SELECT * from attack_cards";
            var connection = new MySqlConnection(MySQL.CreateConnectionString());
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
                AttackCard tempCard = new AttackCard();

                tempCard.id = (int)(uint)reader["id"];
                tempCard.type = "Attack";
                tempCard.damage = (int)reader["damage"];
                tempCard.bullets = (int)reader["bullets"];
                tempCard.accuracy = (int)reader["accuracy"];
                tempCard.name = (string)reader["name"];
                tempCard.image = (string)reader["image"];


                
                tempCard.initiativeEffect = (int)reader["initiativeEffectID"];
                tempCard.initiativeValue = (int)reader["initiativeValue"];
                tempCard.initiativeDuration = (int)reader["initiativeDuration"];


                
                result[tempCard.id] = tempCard;
                

            }

            reader.Close();
            connection.Close();

            return result;
        }
        public static Dictionary<int, HealCard> GetAllHealCard()
        {
            var result = new Dictionary<int, HealCard>();


            var query = "SELECT * from heal_cards";
            var connection = new MySqlConnection(MySQL.CreateConnectionString());
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
                HealCard tempCard = new HealCard();

                tempCard.id = (int)(uint)reader["id"];
                tempCard.type = "Heal";
                tempCard.heal = (int)reader["heal"];
                tempCard.name = (string)reader["name"];
                tempCard.image = (string)reader["image"];


                
                tempCard.initiativeEffect = (int)reader["initiativeEffectID"];
                tempCard.initiativeValue = (int)reader["initiativeValue"];
                tempCard.initiativeDuration = (int)reader["initiativeDuration"];


                result[tempCard.id] = tempCard;


            }

            reader.Close();
            connection.Close();

            return result;
        }
        public static Dictionary<int, ItemCard> GetAllItemCard()
        {
            var result = new Dictionary<int, ItemCard>();


            var query = "SELECT * from item_cards";
            var connection = new MySqlConnection(MySQL.CreateConnectionString());
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
                ItemCard tempCard = new ItemCard();

                tempCard.id = (int)(uint)reader["id"];
                tempCard.type = "Item";
                tempCard.name = (string)reader["name"];
                tempCard.image = (string)reader["image"];

                tempCard.itemEffectImage = (string)reader["image"]; // TODO TEMPORARY, CHANGE TO CONCRETE
                tempCard.itemDuration = (int)reader["initiativeDuration"];
                tempCard.itemEffectLabel =((int)reader["initiativeValue"]).ToString();
                
                tempCard.initiativeEffect = (int)reader["initiativeEffectID"];
                tempCard.initiativeValue = (int)reader["initiativeValue"];
                tempCard.initiativeDuration = (int)reader["initiativeDuration"];


                

                result[tempCard.id] = tempCard;


            }

            reader.Close();
            connection.Close();

            return result;
        }

        public static Dictionary<int, Effect> GetAllEffects()
        {
            var result = new Dictionary<int, Effect>();


            var query = "SELECT * from effects";
            var connection = new MySqlConnection(MySQL.CreateConnectionString());
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
                Effect effect = new Effect();

                effect.ID = (int)reader["id"];
                effect.name = (string)reader["name"];
                effect.image = (string)reader["image"];
                

                effect.delegateName = (string)reader["delegateName"];
                effect.predEffect = (int)reader["predEffect"];
                effect.selfEffect = (int)reader["selfEffect"];

                result[effect.ID] = effect;


            }

            reader.Close();
            connection.Close();

            return result;
        }

    }
}
