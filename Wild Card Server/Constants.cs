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
        public static float LENGTH_OF_BEFORE_ROUND_TIMER = 1.0f;


        public static Dictionary<int, DbInstanceCard> Cards = DatabaseManager.GetAllCards();
        public static Dictionary<List<int>, DbInstanceCard> Combo4Cards = DatabaseManager.GetCombo4Cards();
        public static Dictionary<List<int>, DbInstanceCard> Combo3Cards = DatabaseManager.GetCombo3Cards();
        public static Dictionary<List<int>, DbInstanceCard> Combo2Cards = DatabaseManager.GetCombo2Cards();


        //Need for Combo Detection 
        public class ListComparer<T> : IEqualityComparer<List<T>>
        {
            public bool Equals(List<T> x, List<T> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<T> obj)
            {
                int hashcode = 0;
                foreach (T t in obj)
                {
                    hashcode ^= t.GetHashCode();
                }
                return hashcode;
            }
        }




    }

}
