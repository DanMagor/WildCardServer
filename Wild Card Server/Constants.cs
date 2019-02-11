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

        public static Dictionary<int, Card> cards = Database.GetAllCards();
        public static Dictionary<int,AttackCard> attackCards = Database.GetAllAttackCards();
        public static Dictionary<int, HealCard> healCards = Database.GetAllHealCard();
        public static Dictionary<int, ItemCard> itemCards = Database.GetAllItemCard();
        


        
    }

    class MatchConstants
    {


        public delegate void UseEffect(TempPlayer player, int value);
        public static Dictionary<string, UseEffect> effects = new Dictionary<string, UseEffect>()
        {
            { "AddBullet",AddBullet},
            {"Reload", Reload},
            {"Boom", Boom },
            {"AddAccuracy", AddAccuracy },
            {"AddAccuracyInitiative", AddAccuracy },
            {"AddEvasion", AddEvasion },



        };
        public static void AddBullet(TempPlayer player, int value)
        {
            player.results.bulletsSpent += value;
        }
        public static void Reload(TempPlayer player, int value)
        {
            player.n_bullets = value; //TODO: REWORK LOGIC IN THE RIGHT WAY
        }
        public static void AddAccuracy(TempPlayer player, int value)
        {
            player.results.accuracy += value;
        }
        public static void Boom(TempPlayer player, int value)
        {
            TempPlayer other;
            if(MatchMaker.matches[player.matchID].p1 == player)
            {
                other = MatchMaker.matches[player.matchID].p2;
            }
            else
            {
                other = MatchMaker.matches[player.matchID].p1;
            }
            other.ReceivePureDamage(value);
        }
        public static void AddEvasion(TempPlayer player, int value)
        {
            player.results.evasion += value;
        }
    }
}
