using System;
using System.Threading;


namespace Wild_Card_Server
{
    class Program
    {

        private static Thread consoleThread;

        static void Main(string[] args)
        {
            InitializeConsoleThread();
            ServerHandleData.InitializePacketListener();
            ServerTCP.InitializeServer();
        }

        private static void InitializeConsoleThread()
        {
            consoleThread = new Thread(ConsoleLoop);
            consoleThread.Name = "ConsoleThread";
            consoleThread.Start();
        }

        private static void ConsoleLoop()
        {
            while (true)
            {

            }

        }
    }
}
