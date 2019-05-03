using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Wild_Card_Server
{
    class PlayerMatchEntity
    {
        public Dictionary<int,CardEntity> cardsForRoundPos; //Position to card
       
        
        public int ConnectionId;
        public int MatchID;
        public string Username;


        public List<CardEntity> CardDeck;
        public int Num_AttackCardsInDeck;
        public int Num_HealCardsInDeck;
        public int Num_ArmorCardsInDeck;

        public RoundResults Results;



        private const int maxHealth = 100;
        public int health;
        public int armor;
       

        public struct RoundResults
        {
            public bool amIShot;
            public List<int> soloCardsPos; //
            public List<List<int>> combos; //List of Lists. Each internal Array List is: 1. result card ID 2. Direction 3.sequence of card positions that were combined
            public int playerHP;
            public int playerArmor;
            

            public List<int> enemySelectedCards; // Sequence of cardIDs and directions(int)
            public int enemyHP;
            public int enemyArmor;





        }

  
  
        public bool isReady = false;

      

       

        public PlayerMatchEntity(int _connectionID, string _username, int HP = 100)
        {
            ConnectionId = _connectionID;
            Username = _username;
            health = HP;
            cardsForRoundPos = new Dictionary<int, CardEntity>();
            Results = new RoundResults
            {
                soloCardsPos = new List<int>(),
                combos = new List<List<int>>(),
                enemySelectedCards = new List<int>()
            };
            Num_AttackCardsInDeck = 0;
            Num_HealCardsInDeck = 0;
            Num_ArmorCardsInDeck = 0;
            SetDefaultValuesForResult();

        }



        

        public void ToggleCardSelection(int cardPos)
        {
            if (cardsForRoundPos.ContainsKey(cardPos))
            {
                cardsForRoundPos[cardPos].Selected = !cardsForRoundPos[cardPos].Selected;
            }
            else
            {
                Console.WriteLine("Wrong Card Position in Toggle Card");
            }
            
        }


        public void GetDamage(int value)
        {
            if (armor <= 0)
            {
                health = Math.Max(0,health-value);
            }
            else
            {
                armor = Math.Max(0, armor - value);
            }
        }
        public void GetHeal(int value)
        {
            health = Math.Min(maxHealth, health + value);
        }
        public void GetArmor(int value)
        {
            armor += value;
        }

        public void SetDefaultValuesForResult()
        {
            cardsForRoundPos.Clear();
            Results = new RoundResults
            {
                soloCardsPos = new List<int>(),
                combos = new List<List<int>>(),
                enemySelectedCards = new List<int>()
            };

        }




    }
}
