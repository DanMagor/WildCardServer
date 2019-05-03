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



        #region Initialization
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
            MySQLConnector.MySQLSettings.user = "root";
            MySQLConnector.MySQLSettings.password = "";
            MySQLConnector.MySQLSettings.server = "localhost";
            MySQLConnector.MySQLSettings.database = "Wild Card";

            MySQLConnector.ConntectToMySQL();
        }
        private static void InitializeClientObjects()
        {
            clientObjects = new ClientObject[Constants.MAX_PLAYERS];

            for (int i = 0; i < Constants.MAX_PLAYERS; i++)
            {
                clientObjects[i] = new ClientObject(null, 0);
            }
        }
        #endregion



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
                    return;
                }
            }
        }

        private static void SendDataTo(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
            buffer.WriteBytes(data);
            clientObjects[connectionID].myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            buffer.Dispose();
        }


        #region Data Sending Packets
        public static void PACKET_SendAllCards(int connectionID)
        {
            //TODO: Delete Later unneeded fields for client
            var buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendAllCardsData);

            buffer.WriteInteger(Constants.Cards.Count); // Number of cards in dictionary

            foreach (var card in Constants.Cards.Values) // Write each card in buffer
            {
                buffer.WriteInteger(card.Id);
                buffer.WriteString(card.Name);
                buffer.WriteString(card.Type);
                buffer.WriteBool(card.IsComboCard);
                buffer.WriteInteger(card.NForCombo);
                for (var i = 0; i < card.NForCombo; i++)
                {
                    buffer.WriteInteger(card.ComboCards[i]);
                }
                buffer.WriteString(card.CardImage);
                buffer.WriteString(card.ItemImage);
                buffer.WriteInteger(card.Value);
                buffer.WriteString(card.Animation);



            }


            SendDataTo(connectionID, buffer.ToArray()); //sending data to client

        }
        #endregion

        #region SceneLoading Packets
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
            //Write Player Username for scene loading
            buffer.WriteString(MatchMaker.Matches[matchID].Player1.ConnectionId == connectionID
                ? MatchMaker.Matches[matchID].Player1.Username
                : MatchMaker.Matches[matchID].Player2.Username);

            //Write Enemy Username
            buffer.WriteString(MatchMaker.Matches[matchID].Player1.ConnectionId == connectionID
                ? MatchMaker.Matches[matchID].Player2.Username
                : MatchMaker.Matches[matchID].Player1.Username);


            SendDataTo(connectionID, buffer.ToArray());
        }
        #endregion

        #region Match Packets
        public static void PACKET_Match_SendCards(int connectionID, ByteBuffer cards)
        {
            var buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendMatchCards);
            buffer.WriteBytes(cards.ToArray());
            SendDataTo(connectionID, buffer.ToArray());

        }

        public static void PACKET_Match_StartRound(int connectionID, float timerTime)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SStartRound);
            buffer.WriteFloat(timerTime);
            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_Match_ShowCards(int connectionID)
        {
            var buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SShowCards);
            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_Match_ShowResult(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SShowResult);
            buffer.WriteBytes(data);
            

            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_Match_ConfirmToggleCard(int connectionID, int cardPos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SConfirmToggleCard);
            buffer.WriteInteger(cardPos);
            SendDataTo(connectionID, buffer.ToArray());
        }

        public static void PACKET_Match_FinishGame(int connectionID, string winnerUsername)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SFinishGame);
            buffer.WriteString(winnerUsername);

            SendDataTo(connectionID, buffer.ToArray());
        }
        #endregion



    }
}
