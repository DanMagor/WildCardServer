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

        
    }

    class MatchConstants
    {
        

        public delegate void UseEffect(TempPlayer player);
        public static Dictionary<string, UseEffect> effects = new Dictionary<string, UseEffect>()
        {
            { "AddBullet",AddBullet},

        };
        public static void AddBullet(TempPlayer player)
        {
            player.results.bulletsSpent += 1;
        }
    }
}
