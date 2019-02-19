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


        public delegate void UseEffect(TempPlayer player, int value=0);
        public static Dictionary<string, UseEffect> effects = new Dictionary<string, UseEffect>()
        {
            {"ShootInHead", ShootInHead},
            {"ShootInArm", ShootInArm},
            {"ShootInLeg", ShootInLeg},
            {"ShootInBody", ShootInBody},

            {"InjuredArm", InjuredArm},
            {"InjuredLeg", InjuredLeg},


            { "AddBullet",AddBullet},
            {"Reload", Reload},
            {"Boom", Boom },
            {"AddAccuracy", AddAccuracy },
            {"AddAccuracyInitiative", AddAccuracy },
            {"AddEvasion", AddEvasion },
            



        };

        public static void ShootInHead(TempPlayer player, int value)
        {
            player.results.accuracy -= 50;
            player.results.dmgPerBullet = player.results.dmgPerBullet * 2;
        }
        public static void ShootInArm(TempPlayer player, int value)
        {
            TempPlayer other;
            if (MatchMaker.matches[player.matchID].p1 == player)
            {
                other = MatchMaker.matches[player.matchID].p2;
            }
            else
            {
                other = MatchMaker.matches[player.matchID].p1;
            }
            other.AddEffect("InjuredArm", 0, 10000); //TODO: CHECK IF IT'S Ok or change to some other value for duration
        }
        public static void ShootInLeg(TempPlayer player, int value)
        {
            TempPlayer other;
            if (MatchMaker.matches[player.matchID].p1 == player)
            {
                other = MatchMaker.matches[player.matchID].p2;
            }
            else
            {
                other = MatchMaker.matches[player.matchID].p1;
            }
            other.AddEffect("InjuredLeg", 0, 10000); //TODO: CHECK IF IT'S Ok or change to some other value for duration
        }
        public static void ShootInBody(TempPlayer player, int value)
        {

        }

        public static void InjuredArm(TempPlayer player, int value)
        {
            player.results.accuracy -= 10;
        }
        public static void InjuredLeg(TempPlayer player, int value)
        {
            player.results.evasion -= 10;
        }
        public static void AddBullet(TempPlayer player, int value)
        {
            player.results.bulletsSpent += value;
        }
        public static void Reload(TempPlayer player, int value)
        {
            player.n_bullets = value; 
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
