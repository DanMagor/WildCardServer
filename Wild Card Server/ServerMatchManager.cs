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
        

        public ServerMatchManager(int _matchID, TempPlayer _player1, TempPlayer _player2)
        {
            p1 = _player1;
            p2 = _player2;
            matchID = _matchID;
            
        }

        public void StartMatch()
        {
            ServerTCP.PACKET_LoadMatch(p1.connectionID, matchID);
            ServerTCP.PACKET_LoadMatch(p2.connectionID, matchID);
        }

    }
}
