using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;

namespace Wild_Card_Server
{
    class ServerMatchManager
    {
        public TempPlayer p1;
        public TempPlayer p2;

        public int matchID;

        public bool isActive = false;

        public MySqlConnection matchSQLConnection;


        public ServerMatchManager(int _matchID, TempPlayer _player1, TempPlayer _player2)
        {
            p1 = _player1;
            p2 = _player2;
            matchID = _matchID;

            p1.matchID = matchID;
            p2.matchID = matchID;
            matchSQLConnection = new MySqlConnection(MySQL.CreateConnectionString());
            try
            {
                matchSQLConnection.Open();
                Console.WriteLine("Match №{0} Succesfully connected to MySQL Server '{1}'", matchID, matchSQLConnection.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        public void InitializeMatch()
        {
            ServerTCP.PACKET_LoadMatch(p1.connectionID, matchID);
            ServerTCP.PACKET_LoadMatch(p2.connectionID, matchID);
        }

        public void StartMatch()
        {
            //Wait while both clients are ready
            while (!p1.Ready || !p2.Ready) { }
            isActive = true;
            StartRound();
        }



        //TODO : REFACTOR IT!!
        public void StartRound()
        {
            DateTime roundStartTIme;
            //roundStartTIme = DateTime.Now.Add(TimeSpan.FromSeconds(Constants.LENGTH_OF_ROUND));
            while (isActive)
            {

                //Wait while client Catch Cards
                if ((p1.Ready && p2.Ready))
                {

                    SendCards();
                    p1.Ready = false;
                    p2.Ready = false;
                    roundStartTIme = DateTime.Now;
                    ServerTCP.PACKET_StartRound(p1.connectionID);
                    ServerTCP.PACKET_StartRound(p2.connectionID);

                    while ((!p1.Ready || !p2.Ready) && DateTime.Now.Subtract(roundStartTIme).Seconds <= Constants.LENGTH_OF_ROUND) { }


                    CalculateResults();



                    //Send Info to each player in format: playerHealth, EnemyHealth, PlayerBullets, EnemyBullets, PlayerCard, EnemyCard //Later add: PlayerAction, EnemyAction for animation
                    //for player1:
                    ByteBuffer buffer = new ByteBuffer();
                    buffer.WriteInteger(p1.Health);
                    buffer.WriteInteger(p2.Health);
                    buffer.WriteInteger(p1.Bullets);
                    buffer.WriteInteger(p2.Bullets);
                    buffer.WriteInteger(p1.selectedCardID);
                    buffer.WriteInteger(p2.selectedCardID);

                    //for player2:
                    ByteBuffer buffer2 = new ByteBuffer();
                    buffer2.WriteInteger(p2.Health);
                    buffer2.WriteInteger(p1.Health);
                    buffer2.WriteInteger(p2.Bullets);
                    buffer2.WriteInteger(p1.Bullets);
                    buffer2.WriteInteger(p2.selectedCardID);
                    buffer2.WriteInteger(p1.selectedCardID);

                    //Set Ready to false, for animations
                    p1.Ready = false;
                    p2.Ready = false;
                    //deselect cards:
                   

                    //Sending:
                    ServerTCP.PACKET_ShowResult(p1.connectionID, buffer.ToArray());
                    ServerTCP.PACKET_ShowResult(p2.connectionID, buffer2.ToArray());



                }

            }
        }

        private void CalculateResults()
        {


            TempPlayer[] players = { p1, p2 };
            foreach (var player in players)
            {
                if (player.selectedCardID != -1)
                {
                    string type = Constants.cards[player.selectedCardID].type;
                    switch (type)
                    {
                        case "Attack":
                            CalculateAttackCard(player);
                            break;
                        case "Heal":
                            CalculateHealCard(player);
                            break;
                        case "Item":
                            CalculateItemCard(player);
                            break;
                    }
                }
                else
                {
                    CalculateNoCard(player);
                }
            }
            foreach (var player in players)
            {
                player.UpdateStats();
                player.SetDefaultValuesForResult();
            }
            
        }

        private void CalculateNoCard(TempPlayer player)
        {
            player.UseEffects();
            
        }
        private void CalculateAttackCard(TempPlayer player)
        {
           

            AttackCard card = Constants.attackCards[player.selectedCardID];

            //Add initiative if player was first
            if (player.initiative &&  card.initiativeEffect != "")
            {
                player.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
            }

            //add additional effect if card has it
            if (card.additionalEffect != "")
            {
                player.AddEffect(card.additionalEffect, card.additionalEffectValue, card.additionalEffectDuration);
            }
            

            //Save temp results for match
            player.results.dmgPerBullet = card.damage;
            player.results.bulletsSpent = card.bullets;
            player.results.accuracy += card.accuracy; //+ because default value for accuracy is 100

            //Use Active Effect for results calculating
            player.UseEffects();


            //DETECT Other player for shooting
            TempPlayer other = player == p1 ? p2 : p1;
            player.MakeShots(other);
        }

        private void CalculateHealCard(TempPlayer player)
        {
            HealCard card = Constants.healCards[player.selectedCardID];

            //Add initiative if player was first
            if (player.initiative && card.initiativeEffect != "")
            {
                player.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
            }

            //add additional effect if card has it
            if (card.additionalEffect != "")
            {
                player.AddEffect(card.additionalEffect, card.additionalEffectValue, card.additionalEffectDuration);
            }

            player.results.healing += card.heal;

            //Use Active Effect for results calculating
            player.UseEffects();

        }

        private void CalculateItemCard(TempPlayer player)
        {
            ItemCard card = Constants.itemCards[player.selectedCardID];

            //Add initiative if player was first
            if (player.initiative && card.initiativeEffect != "")
            {
                player.AddEffect(card.initiativeEffect, card.initiativeValue, card.initiativeDuration);
            }

            //add additional effect if card has it
            if (card.additionalEffect != "")
            {
                player.AddEffect(card.additionalEffect, card.additionalEffectValue, card.additionalEffectDuration);
            }
            

            //Use Active Effect for results calculating
            player.UseEffects();

        }

        //TODO Rework Logic for random cards for EACH Player from DB
        private void SendCards()
        {

            //TODO CHANGE, NOW WORKS ONLY FOR ATTACK CARDS
            ArrayList cards = new ArrayList();

            Random rand = new Random();

            //Add Attack Card
            cards.Add(Constants.attackCards.ElementAt(rand.Next(0, Constants.attackCards.Count)).Key);

            //Add Heal Card
            cards.Add(Constants.healCards.ElementAt(rand.Next(0, Constants.healCards.Count)).Key);

            //Add Item Card
            cards.Add(Constants.itemCards.ElementAt(rand.Next(0, Constants.itemCards.Count)).Key);


            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);

        }



    }
}
