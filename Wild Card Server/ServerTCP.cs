using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Wild_Card_Server
{
    class ServerTCP
    {
        private static TcpListener serverSocket;
        public static ClientObject[] clientObjects;

        public static void InitializeServer()
        {
            InitializeMySQLServer();
            InitializeClientObjects();
            InitializeServerSocket();
        }

        private static void InitializeServerSocket()
        {
            serverSocket = new TcpListener(IPAddress.Any, 5555);
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);
        }
        private static void InitializeMySQLServer()
        {
            MySQL.mySQLSettings.user = "root";
            MySQL.mySQLSettings.password = "";
            MySQL.mySQLSettings.server = "localhost";
            MySQL.mySQLSettings.database = "Wild Card";

            MySQL.ConntectToMySQL();
        }

        private static void ClientConnectCallback(IAsyncResult result)
        {
            TcpClient tempClient = serverSocket.EndAcceptTcpClient(result);
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);

            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (clientObjects[i].socket == null)
                {
                    clientObjects[i] = new ClientObject(tempClient, i);

                    Console.WriteLine("Incoming Connection from {0}", clientObjects[i].socket.Client.RemoteEndPoint.ToString());

                    ServerTCP.PACKET_WelcomeMsg(i, "Welcome to my Server");
                    return;
                }
            }
        }

        public static void SendDataTo(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
            buffer.WriteBytes(data);
            clientObjects[connectionID].myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            buffer.Dispose();

        }


        private static void InitializeClientObjects()
        {
            clientObjects = new ClientObject[Constants.MAX_PLAYERS];

            for (int i = 0; i < Constants.MAX_PLAYERS; i++)
            {
                clientObjects[i] = new ClientObject(null, 0);
            }
        }

        public static void PACKET_WelcomeMsg(int connectionID, string msg)
        {
            ByteBuffer buffer = new ByteBuffer();

            //1 step always add the package id
            buffer.WriteInteger((int)ServerPackages.SWelcomeMsg);

            //2 step send the information that you want to.
            buffer.WriteString(msg);
            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_LoadMenu(int connectionID, string username)
        {
            ByteBuffer buffer = new ByteBuffer();

            buffer.WriteInteger((int)ServerPackages.SLoadMenu);
            buffer.WriteString(username);
            //Add some data here if needed
            SendDataTo(connectionID, buffer.ToArray());

        }

        public static void PACKET_LoadMatch(int connectionID, int matchID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SLoadMatch);

            //For future actions we need to know MatchID
            buffer.WriteInteger(matchID);

            //Check Which players we sending data, cause we also want to send Enemy Username in Package
            if (MatchMaker.matches[matchID].p1.connectionID == connectionID)
            {
                buffer.WriteString(MatchMaker.matches[matchID].p2.username);
            }
            else
            {
                buffer.WriteString(MatchMaker.matches[matchID].p1.username);
            }



            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_SendAllCards(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendAllCardsAndEffects);

            //Write Attack Cards in Buffer
            buffer.WriteInteger(Constants.attackCards.Count); // Save number of Attack Cards for reading on Client
            foreach (var card in Constants.attackCards.Values)
            {
                //General Info
                buffer.WriteInteger(card.id);
                buffer.WriteString(card.type);
                buffer.WriteString(card.name);
                buffer.WriteString(card.image);

                //Attack Card Info
                buffer.WriteInteger(card.damage);
                buffer.WriteInteger(card.bullets);
                buffer.WriteInteger(card.accuracy);

                //Initiative Effect
                
                buffer.WriteInteger(card.initiativeEffect);
                buffer.WriteInteger(card.initiativeValue);
                buffer.WriteInteger(card.initiativeDuration);

                
            }

            //Write Heal Cards in Buffer
            buffer.WriteInteger(Constants.healCards.Count); // Save number of Heal Cards for reading on Client
            foreach (var card in Constants.healCards.Values)
            {
                //General Info
                buffer.WriteInteger(card.id);
                buffer.WriteString(card.type);

                buffer.WriteString(card.name);
                buffer.WriteString(card.image);

                //Heal Card Info
                buffer.WriteInteger(card.heal);
                
                //Initiative Effect
                buffer.WriteInteger(card.initiativeEffect);
                buffer.WriteInteger(card.initiativeValue);
                buffer.WriteInteger(card.initiativeDuration);

            }

            //Write Item Cards in buffer
            buffer.WriteInteger(Constants.itemCards.Count); // Save number of Item Cards for reading on Client
            foreach (var card in Constants.itemCards.Values)
            {
                //General Info
                 buffer.WriteInteger(card.id);
                buffer.WriteString(card.type);
                buffer.WriteString(card.name);
                buffer.WriteString(card.image);

                //Item Card Info 
                buffer.WriteInteger(card.itemDuration);
                buffer.WriteString(card.itemEffectLabel);
                buffer.WriteString(card.itemEffectImage);

                //Initiative Effect
                buffer.WriteInteger(card.initiativeEffect);
                buffer.WriteInteger(card.initiativeValue);
                buffer.WriteInteger(card.initiativeDuration);

            }


            //Write All effects info
            buffer.WriteInteger(Constants.effects.Count);
            foreach (var effect in Constants.effects.Values)
            {
                buffer.WriteInteger(effect.ID);
                buffer.WriteString(effect.name);
                buffer.WriteString(effect.image);
            }
            

            SendDataTo(connectionID, buffer.ToArray());

        }

        public static void PACKET_SendCards(int connectionID, ArrayList cards)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendCards);
            buffer.WriteInteger(cards.Count);

            foreach (int card_id in cards)
            {
                buffer.WriteInteger(card_id);
            }
            SendDataTo(connectionID, buffer.ToArray());

        }

        public static void PACKET_StartRound(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SStartRound);

            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_ShowResult(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SShowResult);
            buffer.WriteBytes(data);
            

            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_FinishGame(int connectionID, string winnerUsername)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SFinishGame);
            buffer.WriteString(winnerUsername);

            SendDataTo(connectionID, buffer.ToArray());
        }




    }
}
