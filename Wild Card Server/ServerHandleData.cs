﻿using System;
using System.Collections.Generic;
using System.Collections;
using MySql.Data.MySqlClient;

namespace Wild_Card_Server
{
    class ServerHandleData
    {
        public delegate void Packet_(int connectionID, byte[] data);
        public static Dictionary<int, Packet_> packetListener;
        private static int pLength;

        public static void InitializePacketListener()
        {
            packetListener = new Dictionary<int, Packet_>();
            packetListener.Add((int)ClientPackages.CLogin, HandleLogin);
            packetListener.Add((int)ClientPackages.CSearchOpponent, HandleSearch);
            packetListener.Add((int)ClientPackages.CSetReady, Handle_Match_SetReady);
            packetListener.Add((int)ClientPackages.CSendToggleCard, Handle_Match_ToggleCard);
            packetListener.Add((int)ClientPackages.CRestartMatch, Handle_Match_RestartMatch);
            packetListener.Add((int)ClientPackages.CShot, Handle_Match_Shot);
            packetListener.Add((int)ClientPackages.CLeaveMatch, Handle_Match_LeaveMatch);

        }

        public static void HandleData(int connectionID, byte[] data)
        {
            //Copying our packet information into a temporary array to edit and peek it
            byte[] buffer = (byte[])data.Clone();

            //Checking if the connected player which sent this package has an instance of the bytebuffer
            // in order to read out the information of the byte[]buffer
            if (ServerTCP.clientObjects[connectionID].buffer == null)
            {
                //if there is no instance, then create a new one
                ServerTCP.clientObjects[connectionID].buffer = new ByteBuffer();
            }

            //Reading out the package from the player in order to check which package it actually is.
            ServerTCP.clientObjects[connectionID].buffer.WriteBytes(buffer);

            // Checking if the received package is empty, if so then do not continue executing this code
            if (ServerTCP.clientObjects[connectionID].buffer.Count() == 0)
            {
                ServerTCP.clientObjects[connectionID].buffer.Clear();
                return;
            }

            //Checking if the package actually contains information
            if (ServerTCP.clientObjects[connectionID].buffer.Length() >= 4)
            {
                //Read out  the full package length
                pLength = ServerTCP.clientObjects[connectionID].buffer.ReadInteger(false);

                if (pLength <= 0)
                {
                    //if there is no package or is invalid then close this method
                    ServerTCP.clientObjects[connectionID].buffer.Clear();
                    return;
                }
            }

            while (pLength > 0 & pLength <= ServerTCP.clientObjects[connectionID].buffer.Length() - 4)
            {
               
                ServerTCP.clientObjects[connectionID].buffer.ReadInteger();
                data = ServerTCP.clientObjects[connectionID].buffer.ReadBytes(pLength);
                HandleDataPackages(connectionID, data);
               

                pLength = 0;
                if (ServerTCP.clientObjects[connectionID].buffer.Length() >= 4)
                {
                    pLength = ServerTCP.clientObjects[connectionID].buffer.ReadInteger(false);
                }

                if (pLength <= 0)
                {
                    //if there is no package or is invalid then close this method
                    ServerTCP.clientObjects[connectionID].buffer.Clear();
                    return;
                }

                if (pLength <= 1)
                {
                    ServerTCP.clientObjects[connectionID].buffer.Clear();
                }
            }

        }
        private static void HandleDataPackages(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();

            Packet_ packet;
            if (packetListener.TryGetValue(packageID, out packet))
            {
                packet.Invoke(connectionID, data);
            }
        }


        private static void HandleLogin(int connectionId, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            string username = buffer.ReadString();
            string password = buffer.ReadString();

            var mySQLConnection = new MySqlConnection(MySQLConnector.CreateConnectionString());
            try
            {
                mySQLConnection.Open();
                Console.WriteLine("Player {0} Succesfully connected to MySQL Server '{1}'", username, mySQLConnection.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            
            mySQLConnection.Close();
            ServerTCP.PACKET_SendAllCards(connectionId);
            Console.WriteLine("Player '{0}' succesfully logged into his account", username);
            ServerTCP.PACKET_LoadMenu(connectionId, username);
        }
        private static void HandleSearch(int connectionId, byte[] data)
        {

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            string username = buffer.ReadString();

            Console.WriteLine("Player '{0}' started search", username);
            MatchMaker.AddPlayerToSearch(connectionId, username);
        }


        #region Handle Match Packages
        private static void Handle_Match_SetReady(int connectionId, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            int matchID = buffer.ReadInteger();

            if (MatchMaker.Matches[matchID].Player1.ConnectionId == connectionId)
            {
                MatchMaker.Matches[matchID].Player1.isReady = true;
                Console.WriteLine("Player '{0}' ready for match", MatchMaker.Matches[matchID].Player1.Username);
            }
            else
            {
                MatchMaker.Matches[matchID].Player2.isReady = true;
                Console.WriteLine("Player '{0}' ready for match", MatchMaker.Matches[matchID].Player2.Username);
            }

        }
        private static void Handle_Match_ToggleCard(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInteger(); //Package ID
            var matchID = buffer.ReadInteger();
            buffer.WriteInteger(connectionID); // Write Player Connection ID for recognition
            MatchMaker.Matches[matchID].ToggleCardSelection(buffer);


        }
        private static void Handle_Match_RestartMatch(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInteger(); //read package ID
            int matchID = buffer.ReadInteger();
            MatchMaker.Matches[matchID].RequestRestart();



        }
        private static void Handle_Match_Shot(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInteger(); //packageID
            var matchID = buffer.ReadInteger();
            

            MatchMaker.Matches[matchID].PlayerShot(connectionID);

        }
        private static void Handle_Match_LeaveMatch(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInteger(); //Read package ID
            var matchId = buffer.ReadInteger();
            MatchMaker.Matches[matchId].RequestLeaveMatch(connectionID);
        }
        #endregion
    }
}
