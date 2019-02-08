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

        public MySqlConnection mySQLConnection;


        public ServerMatchManager(int _matchID, TempPlayer _player1, TempPlayer _player2)
        {
            p1 = _player1;
            p2 = _player2;
            matchID = _matchID;
            mySQLConnection = new MySqlConnection(MySQL.CreateConnectionString());
            try
            {
                mySQLConnection.Open();
                Console.WriteLine("Match №{0} Succesfully connected to MySQL Server '{1}'", matchID, mySQLConnection.ToString());
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
                    
                    while ((!p1.Ready || !p2.Ready) && DateTime.Now.Subtract(roundStartTIme).Seconds <= Constants.LENGTH_OF_ROUND) {}
                    p1.Ready = false;
                    p2.Ready = false;
                    ServerTCP.PACKET_ShowResult(p1.connectionID);
                    ServerTCP.PACKET_ShowResult(p2.connectionID);
                    
                    
                }

            }
        }



        //TODO Rework Logic for random cards from DB
        private void SendCards()
        {
            ArrayList cards = Database.TakeRandomCardsOfEachType(mySQLConnection); //TODO Replace "3" with Const?


            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);

        }



    }
}
