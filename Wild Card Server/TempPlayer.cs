using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wild_Card_Server
{
    class TempPlayer
    {

        public int connectionID;
        public string username;
        public RoundResults results = new RoundResults();

        private int health;
        private int n_bullets;
        private Dictionary<string, int> effects; //Name of effect and remaining duration time 


        public struct RoundResults
        {
            public int dmgPerBullet;
            public int bulletsSpent;
            public int accuracy;
            public int evasion;
            public int healing;
            public int enemyPureDamage;
            public int selfdamage;

        }


        //private ArrayList equipment;
        //private int[] Deck;

        public int selectedCardID = -1;
        private bool ready = false;

        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
        }

        public int Health
        {
            get { return health; }
        }

        public int Bullets
        {
            get { return n_bullets; }
        }

        public TempPlayer(int _connectionID, string _username, int HP = 100, int bullets = 6)
        {
            connectionID = _connectionID;
            username = _username;
            health = HP;
            n_bullets = bullets;
            effects = new Dictionary<string, int>();
            SetDefaultValuesForResult();
            
        }

        //public void TakeShoot(int dmgPerBullet)
        //{
            
                
        //    ReceiveDamage(amountOfDamage);
        //}

        public void MakeShots(TempPlayer p2)
        {
            results.bulletsSpent = Math.Min(n_bullets, results.bulletsSpent);

            for (int i = 0; i < results.bulletsSpent; i++)
            {
                //Check Do we hit or not
                if (CheckHit(p2))
                {
                    p2.results.selfdamage += results.dmgPerBullet;
                }

                //In any case we spent bullet
                n_bullets -= 1;
            }
            
        }
        public void UpdateStats()
        {
            UpdateEffects();
            ReceiveDamage();
            selectedCardID = -1;


        }
        public bool CheckHit(TempPlayer p2)
        {
            Random r = new Random();
            bool hit =  r.Next(0, 100) < results.accuracy;
            if (hit)
            {
                return p2.results.evasion < r.Next(0, 100);
            }

            return false;
        }

        public void DealPureDamage(TempPlayer p2, int damage)
        {
            p2.results.selfdamage += damage;
        }

        private void ReceiveDamage()
        {
            health -= results.selfdamage;
        }

        public void AddEffect(string name, int duration)
        {
            effects[name] = duration;
        }
        public void UpdateEffects()
        {
            foreach (var eff in effects.ToList())
            {
                effects[eff.Key] = eff.Value - 1;
                if (effects[eff.Key] <= 0)
                {
                    effects.Remove(eff.Key);
                }
            }
        }
        public void UseEffects()
        {
            foreach (var keyEff in effects.Keys)
            {
                if (MatchConstants.effects.TryGetValue(keyEff, out MatchConstants.UseEffect effect))
                {
                    effect.Invoke(this);
                }
            }

            
        }

        public void SetDefaultValuesForResult()
        {
            results.dmgPerBullet = 0;
            results.bulletsSpent = 0;
            results.accuracy = 100;
            results.evasion = 0;
            results.healing = 0;
            results.selfdamage = 0;
        }




    }
}
