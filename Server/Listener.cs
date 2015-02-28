using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Listener
    {
        // Const
        private const string THREAD_NAME = "Server listener";

        // Flags
        private bool runnig;
        private bool closed;

        // Id counter
        private int idConter;

        // Socket
        private Socket _socket;

        // Client list
        private ClientList _clientList;

        // Worker thread
        private Thread _task;

        public Listener(ref Settings _settings, ref ClientList _clientList)
        {
            this.runnig = false;
            this.closed = false;

            this.idConter = 0;

            this._clientList = _clientList;

            // Socket setup
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._socket.Bind(_settings.ServerIP);
            this._socket.Listen(_settings.ServerQueueSize);

            // Worker thread setup
            this._task = new Thread(new ThreadStart(Task));
            this._task.IsBackground = false;
            this._task.Name = THREAD_NAME;
        }


        public bool Start()
        {
            if (this.closed)
                return false;
            else
            {
                if (!this.runnig)
                {
                    this.runnig = true;
                    this._task.Start();
                }

                return true;
            }
        }

        public void Close()
        {
            this.runnig = false;
            this._socket.Close();

            this._task.Join();
            this.closed = true;
        }

        private void Task()
        {
            while (this.runnig)
            {
                try
                {
                    var client = this._socket.Accept();

                    this._clientList.AddClient(new ClientObject(client, this.idConter++));
                }
                catch (Exception e)
                {
                    if (!this.runnig)
                        break;

                    throw;
                }
            }
        }
    }
}
