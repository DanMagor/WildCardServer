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


        private int health;
        private int n_bullets;
        private ArrayList effects; // TODO Check that we don't need explicit type here



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

        public TempPlayer(int _connectionID, string _username, int HP = 100, int bullets = 6, ArrayList init_eff = null)
        {
            connectionID = _connectionID;
            username = _username;
            health = HP;
            n_bullets = bullets;
            effects = init_eff ?? new ArrayList();
        }

        public void TakeShoot(int bullets, int dmgPerBullet)
        {
            int amountOfDamage = dmgPerBullet * bullets;
            ReceiveDamage(amountOfDamage);
        }
        public void MakeShoot(int bullets)
        {
            if (n_bullets < 0) return;
            n_bullets -= bullets;
        }

        public void ReceiveDamage(int damage)
        {
            int finalDamage = effects.Count == 0 ? damage : 0;
            health -= finalDamage;
        }

        public void AddEffect(int eff)
        {
            effects.Add(eff);
        }

        public void UpdateEffects()
        {
            effects.RemoveAt(0);
        }






    }
}
