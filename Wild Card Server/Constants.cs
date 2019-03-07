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
        public static Dictionary<int, AttackCard> attackCards = Database.GetAllAttackCards();
        public static Dictionary<int, HealCard> healCards = Database.GetAllHealCard();
        public static Dictionary<int, ItemCard> itemCards = Database.GetAllItemCard();
        public static Dictionary<int, Effect> effects = Database.GetAllEffects();

        public static int NoEffectID = 0;
        public static int ShootInHeadEffectID = 1;
        public static int ShootInArmEffectID = 2;
        public static int ShootInLegEffectID = 3;
        public static int ShootInBodyEffectID = 4;
        public static int InjuredArmID = 5;
        public static int InjuredLegID = 6;

        public static int HealInHeadEffectID = 7;
        public static int HealInArmID = 8;
        public static int HealInLegID = 9;
        public static int HealInBodyID = 10;



    }

    class MatchConstants
    {


        public delegate void UseEffect(TempPlayer player, int value = 0);
        public static Dictionary<string, UseEffect> effects = new Dictionary<string, UseEffect>()
        {
            {"ShootInHead", ShootInHead},
            {"ShootInArm", ShootInArm},
            {"ShootInLeg", ShootInLeg},
            {"ShootInBody", ShootInBody},

            {"HealInHead", HealInHead },
            {"HealInArm", HealInArm },
            {"HealInLeg", HealInLeg },
            {"HealInBody", HealInBody },


            {"InjuredArm", InjuredArm},
            {"InjuredLeg", InjuredLeg},


            { "AddBullet",AddBullet},
            {"FullReload",FullReload},
            {"Reload", Reload},
            {"Boom", Boom },
            {"AddAccuracy", AddAccuracy },
            {"DecreaseAccuracy", DecreaseAccuracy},
            {"AddAccuracyInitiative", AddAccuracy },
            {"AddEvasion", AddEvasion },
            {"AddHeal", AddHeal},

            {"RemoveArmNegativeEffects", RemoveArmNegativeEffects },




        };

        public static void ShootInHead(TempPlayer player, int value)
        {
            player.results.accuracy -= 50;
            player.results.dmgPerBullet = player.results.dmgPerBullet * 2;
        }
        public static void ShootInArm(TempPlayer player, int value)
        {
            ServerMatchManager manager = MatchMaker.matches[player.matchID];

            TempPlayer other = manager.p1 == player ? manager.p2 : manager.p1;

            other.AddEffect(Constants.InjuredArmID, -10, 5);
            //player.AddEffect(, 0, 10000); //TODO: CHECK IF IT'S Ok or change to some other value for duration
        }
        public static void ShootInLeg(TempPlayer player, int value)
        {
            ServerMatchManager manager = MatchMaker.matches[player.matchID];

            TempPlayer other = manager.p1 == player ? manager.p2 : manager.p1;

            other.AddEffect(Constants.InjuredLegID, -10, 5);
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

        public static void HealInHead(TempPlayer player, int value)
        {
            player.results.healing = 0;
        }

        public static void HealInArm(TempPlayer player, int value)
        {
            player.results.healing /= 2;
            player.effects.Remove(Constants.InjuredArmID);
        }

        public static void HealInLeg(TempPlayer player, int value)
        {
            player.results.healing /= 2;
            if (player.effects.ContainsKey(Constants.InjuredLegID))
            {
                player.effects.Remove(Constants.InjuredLegID);
            }
        }

        public static void HealInBody(TempPlayer player, int value)
        {

        }

        public static void AddBullet(TempPlayer player, int value)
        {
            player.n_bullets = Math.Min(player.n_bullets + value, player.max_bullets);
        }
        public static void Reload(TempPlayer player, int value)
        {
            player.n_bullets = value;
        }
        public static void FullReload(TempPlayer player, int value)
        {
            player.n_bullets = player.max_bullets;
        }
        public static void AddAccuracy(TempPlayer player, int value)
        {
            player.results.accuracy += value;
        }
        public static void DecreaseAccuracy(TempPlayer player, int value)
        {
            //TODO: Rename this function to DecreaseEnemyAccuracy
            player.results.accuracy -= value;

        }
        public static void Boom(TempPlayer player, int value)
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
            other.ReceivePureDamage(value);
        }
        public static void AddEvasion(TempPlayer player, int value)
        {
            player.results.evasion += value;
        }
        public static void AddHeal(TempPlayer player, int value)
        {
            player.results.healing += value;
        }

        public static void RemoveArmNegativeEffects(TempPlayer player, int value)
        {
            if (player.effects.ContainsKey(Constants.InjuredArmID))
            {
                player.effects.Remove(Constants.InjuredArmID);
            }
        }
    }
}
