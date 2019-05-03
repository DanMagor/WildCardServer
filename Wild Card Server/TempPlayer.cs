using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Wild_Card_Server
{
    class TempPlayer
    {
        public Dictionary<int,Card> cardsForRoundPos; //Position to card
       
        
        public int connectionID;
        public int matchID;
        public string username;


        public List<Card> cardDeck;
        public int nAttackCardsInDeck;
        public int nHealCardsInDeck;
        public int nArmorCardsInDeck;

        public RoundResults results;



        public const int maxHealth = 100;
        private int health;
        private int armor;
       

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

  
  
        private bool ready = false;

        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
        }

        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        public int Armor
        {
            get { return armor; }
            set { armor = value; }
        }

       

        public TempPlayer(int _connectionID, string _username, int HP = 100)
        {
            connectionID = _connectionID;
            username = _username;
            health = HP;
            cardsForRoundPos = new Dictionary<int, Card>();
            results = new RoundResults
            {
                soloCardsPos = new List<int>(),
                combos = new List<List<int>>(),
                enemySelectedCards = new List<int>()
            };
            nAttackCardsInDeck = 0;
            nHealCardsInDeck = 0;
            nArmorCardsInDeck = 0;
            SetDefaultValuesForResult();

        }



        public void MakeShots(TempPlayer p2)
        {
           
        }
        public void UpdateStats()
        {

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
                health -= value;
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
            results = new RoundResults();
            results.soloCardsPos = new List<int>();
            results.combos = new List<List<int>>();
            results.enemySelectedCards = new List<int>();
            
        }




    }
}
