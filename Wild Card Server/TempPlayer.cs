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

        public int selectedCardID;
        private bool ready = false;

        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
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
            int amountOfDamage = bullets * dmgPerBullet;
            ReceiveDamage(amountOfDamage);
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
