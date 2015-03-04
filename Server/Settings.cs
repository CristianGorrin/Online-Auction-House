using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace Server
{
    public class Settings
    {
        // Default constant settings values
        private const string DEFAULT_SERVER_IP = "127.0.0.1";
        private const int DEFAULT_SERVER_PORT = 12346;
        private const int DEFAULT_SERVER_SIZE = 20;
        private const int DEFAULT_SERVER_QUEUE = 10;

        // Settings fields
        public IPEndPoint ServerIP { private set; get; }
        public int ServerSize { private set; get; }
        public int ServerQueueSize { private set; get; }
        public int ServerPort { private set; get; }

        public Settings()
        {
            // Default Settings
            this.ServerIP = new IPEndPoint(IPAddress.Parse(DEFAULT_SERVER_IP), DEFAULT_SERVER_PORT);
            this.ServerPort = DEFAULT_SERVER_PORT;
            this.ServerSize = DEFAULT_SERVER_SIZE;
            this.ServerQueueSize = DEFAULT_SERVER_QUEUE;
        }
    }
}
