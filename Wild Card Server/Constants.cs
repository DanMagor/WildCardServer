using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Wild_Card_Server
{
    class Constants
    {
        public static int MAX_PLAYERS = 256;
        public static int MAX_MATCHES = 256;
        public static float LENGTH_OF_ROUND = 3.0f;

        public static Dictionary<int, Card> Cards = Database.GetAllCards();
        public static Dictionary<List<int>,Card> Combo4Cards = Database.GetCombo4Cards();
        public static Dictionary<List<int>,Card> Combo3Cards = Database.GetCombo3Cards();
        public static Dictionary<List<int>,Card> Combo2Cards = Database.GetCombo2Cards();






    }

}
