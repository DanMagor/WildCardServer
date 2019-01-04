using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wild_Card_Server
{
    class ServerMatchManager
    {
        public TempPlayer p1;
        public TempPlayer p2;

        public int matchID;

        public bool isActive = false;


        public ServerMatchManager(int _matchID, TempPlayer _player1, TempPlayer _player2)
        {
            p1 = _player1;
            p2 = _player2;
            matchID = _matchID;

        }

        public void InitializeMatch()
        {
            ServerTCP.PACKET_LoadMatch(p1.connectionID, matchID);
            ServerTCP.PACKET_LoadMatch(p2.connectionID, matchID);
        }

        public void StartMatch()
        {
            while (!p1.Ready || !p2.Ready) { };

            isActive = true;
            while (isActive)
            {
                SendCards();
                isActive = false;

            }
        }

        //TODO Rework Logic for random cards from DB

        private void SendCards()
        {
            Card[] cards = new Card[4];
            for (int i = 0; i < 4; i++)
            {
                cards[i] = new Card("Card" + i, Card.CardTypes.Attack, i);
            }
            ServerTCP.PACKET_SendCards(p1.connectionID, cards);
            ServerTCP.PACKET_SendCards(p2.connectionID, cards);
            
        }

    }
}
