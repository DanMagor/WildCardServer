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

        public static Dictionary<int,ServerMatchManager> Matches = new Dictionary<int, ServerMatchManager>(); 



        public static void AddPlayerToSearch(int connectionID, string username)
        {
            
            players.Add(new TempPlayer(connectionID, username));
            if (players.Count == 2)
            {
                TempPlayer player1 = players[0];
                players.RemoveAt(0);
                TempPlayer player2 = players[0];
                players.RemoveAt(0);
                Matches[matchID] = new ServerMatchManager(matchID, player1, player2);

                Matches[matchID].InitializeMatch();
                while (!Matches[matchID].p1.Ready && !Matches[matchID].p2.Ready) { }
                if (Matches[matchID].p1.Ready && Matches[matchID].p2.Ready)
                {
                    var matchThread = new Thread(Matches[matchID].StartMatch) { Name = "Match " + matchID.ToString() };
                    matchThread.Start();
                }
                

                matchID++;

            }
            //if (!isSearching)
            //{
            //    isSearching = true;
            //    searchingThread = new Thread(SearchingLoop) {Name = "SearchingThread"}; //Simplified Initialization
            //    searchingThread.Start();

            //}
            
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
                    Matches[matchID]= new ServerMatchManager(matchID, player1, player2);
                    Matches[matchID].InitializeMatch();

                    matchID++;
                    
                }
            }
            isSearching = false;
        }

        public static void StartMatch(int matchID)
        {
            if (Matches[matchID].p1.Ready && Matches[matchID].p2.Ready)
            {
                var matchThread = new Thread(Matches[matchID].StartMatch) {Name = "Match " + matchID.ToString()};
                matchThread.Start();
            }
        }

        
    }
}
