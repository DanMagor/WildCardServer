using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wild_Card_Server
{
    class TempPlayer
    {
        public int connectionID;
        public string username;

        private bool ready = false;

        public bool Ready
        {
            get {return ready;}
            set {ready = value;}
        }

        public TempPlayer(int _connectionID, string _username)
        {
            connectionID = _connectionID;
            username = _username;
        }

        
        
    }
}
