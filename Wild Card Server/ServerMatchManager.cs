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
                    string type = Database.GetCardType(matchSQLConnection, player.selectedCardID);
                    switch (type)
                    {
                        case "Attack":
                            Console.Write("ATTTACK!!");
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
            //DETECT Other player for shooting
            TempPlayer other = player == p1 ? p2 : p1;

            // READ INFO ABOUT CARD
            var buffer = Database.GetAttackCardInfo(matchSQLConnection, player.selectedCardID);
            int dmgPBullet = buffer.ReadInteger();
            int bullets = buffer.ReadInteger();
            string effect = buffer.ReadString();
            int duration = buffer.ReadInteger();

            //add Effect as Active to Player
            player.AddEffect(effect, duration);

            //Save temp results for match
            player.results.dmgPerBullet = dmgPBullet;
            player.results.bulletsSpent = bullets;

            //Use Active Effect for results calculating
            player.UseEffects();

            player.MakeShots(other);
        }

        private void CalculateHealCard(TempPlayer player)
        {

        }

        private void CalculateItemCard(TempPlayer player)
        {

        }

        //TODO Rework Logic for random cards from DB
        private void SendCards()
        {
            ArrayList cards = Database.TakeRandomCardsOfEachType(matchSQLConnection); //TODO Replace "3" with Const?


            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);

        }



    }
}
