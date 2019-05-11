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
        private static List<PlayerMatchEntity> playerInSearch = new List<PlayerMatchEntity>();
      

        private static int newMatchID = 0; //Fix Multi Threading issues here 

        public static Dictionary<int, MatchManager> Matches = new Dictionary<int, MatchManager>();


        //Fix Multi Threading issues here
        public static void AddPlayerToSearch(int connectionID, string username)
        {

            playerInSearch.Add(new PlayerMatchEntity(connectionID, username));
            if (playerInSearch.Count == 2)
            {
                PlayerMatchEntity player1 = playerInSearch[0];
                playerInSearch.RemoveAt(0);
                PlayerMatchEntity player2 = playerInSearch[0];
                playerInSearch.RemoveAt(0);
                Matches[newMatchID] = new MatchManager(newMatchID, player1, player2);
                var matchThread = new Thread(Matches[newMatchID].StartMatch) { Name = "Match " + newMatchID.ToString() };
                matchThread.Start();

                newMatchID++;

            }

        }
        

       


    }
}
