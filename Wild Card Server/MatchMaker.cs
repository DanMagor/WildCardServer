using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Wild_Card_Server
{
    class MatchMaker
    {
        private static List<TempPlayer> players = new List<TempPlayer>();
        private static bool isSearching = false;
        private static Thread searchingThread;
        

        private static int matchID = 0;

        public static ServerMatchManager[] matches = new ServerMatchManager[Constants.MAX_MATCHES]; //TODO Rework to LIST or fix NULL Problem

        public static void AddPlayerToSearch(int connectionID, string username)
        {
            
            players.Add(new TempPlayer(connectionID, username));
            if (!isSearching)
            {
                isSearching = true;
                searchingThread = new Thread(SearchingLoop);
                searchingThread.Name = "SearchingThread";
                searchingThread.Start();

            }
            
        }
        public static void SearchingLoop()
        {
            while(players.Count != 0)
            {
                if (players.Count >= 2)
                {
                    TempPlayer player1 = players[0];
                    players.RemoveAt(0);
                    TempPlayer player2 = players[0];
                    players.RemoveAt(0);
                    matches[matchID]= new ServerMatchManager(matchID, player1, player2);
                    matches[matchID].InitializeMatch();

                    matchID++;
                    
                }
            }
            isSearching = false;
        }

        public static void StartMatch(int matchID)
        {
            if (matches[matchID].p1.Ready && matches[matchID].p2.Ready)
            {
                Thread matchThread = new Thread(matches[matchID].StartMatch);
                matchThread.Name = "Match " + matchID.ToString();
                matchThread.Start();
            }
        }

        
    }
}
