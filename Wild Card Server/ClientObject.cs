using System;
using System.Net.Sockets;


namespace Wild_Card_Server
{
    class ClientObject
    {
        public TcpClient socket;
        public NetworkStream myStream;
        public int connectionID;
        private byte[] receiveBuffer;
        public ByteBuffer buffer;

        public ClientObject(TcpClient _socket, int _connectionID)
        {
            if (_socket == null) return;

            socket = _socket;
            connectionID = _connectionID;

            socket.NoDelay = true;

            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            myStream = socket.GetStream();

            receiveBuffer = new byte[4096];

            myStream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int readBytes = myStream.EndRead(result);

                if (readBytes <= 0)
                {
                    CloseConnection();
                    return;
                }

                byte[] newBytes = new byte[readBytes];
                Buffer.BlockCopy(receiveBuffer, 0, newBytes, 0, readBytes);
                ServerHandleData.HandleData(connectionID, newBytes);
                myStream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, ReceiveCallback, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CloseConnection()
        {
            Console.WriteLine("Connection from {0} has been terminated", socket.Client.RemoteEndPoint.ToString());
            socket.Close();
            socket = null;
        }
    }
}
