using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


namespace Server
{
    public class ClientList
    {
        private const int THREAD_SLEEP_TIME = 2000;
        private const string THREAD_NAME_WORKER = "Client Worker";

        // Flags
        private bool runing;

        // All clients connected to the server 
        private ClientObject[] clientObjects;

        private Thread worker;

        public ClientList(ref Settings _settings)
        {
            this.clientObjects = new ClientObject[_settings.ServerSize];

            this.runing = false;

            this.worker = new Thread(new ThreadStart(Taskworker));
            this.worker.IsBackground = false;
            this.worker.Name = THREAD_NAME_WORKER;
        }

        private void Broadcaster(string message)
        {
            lock (this.clientObjects)
            {
                for (int i = 0; i < this.clientObjects.Length; i++)
                {
                    this.clientObjects[i].Send(message);
                }
            }
        }

        private void Taskworker()
        {
            while (this.runing)
            {
                try
                {
                    for (int i = 0; i < clientObjects.Length; i++)
                    {
                        if (this.clientObjects[i] != null)
                        {
                            string clientCommand = string.Empty;

                            lock (this.clientObjects[i])
                            {

                            }

                            if (clientCommand != string.Empty)
                            {
                                switch (clientCommand)
                                {
                                    default:
                                        lock (this.clientObjects[i])
                                            this.clientObjects[i].Send("Command \"" + clientCommand + "\" is not recognized.");
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!this.runing)
                        break;

                    throw;
                }

                Thread.Sleep(THREAD_SLEEP_TIME);
            }
        }

        public void Start()
        {
            this.runing = true;
            this.worker.Start();
        }

        public void Close()
        {
            this.runing = false;
            this.worker.Join();

            lock (this.clientObjects)
            {
                for (int i = 0; i < this.clientObjects.Length; i++)
                {
                    if (this.clientObjects[i] != null)
                        if (!this.clientObjects[i].Closed)
                            new Thread(() => this.clientObjects[i].Close()).Start();
                }
            }
        }

        public void AddClient(ClientObject obj)
        {
            for (int i = 0; i < this.clientObjects.Length; i++)
            {
                if (this.clientObjects[i] == null)
                {
                    lock (this.clientObjects[i])
                    {
                        this.clientObjects[i] = obj;
                    }
                }
            }
        }
    }

    public class ClientObject
    {
        private Socket clientSocket;

        NetworkStream networkstream;
        StreamReader sr;
        StreamWriter sw;

        private bool closed;

        private int id;

        public ClientObject(Socket _socket, int id)
        {
            this.closed = false;

            this.clientSocket = _socket;

            this.networkstream = new NetworkStream(this.clientSocket);
            this.sr = new StreamReader(this.networkstream);
            this.sw = new StreamWriter(this.networkstream);

            this.id = id;

            this.sw.WriteLine(@"/id " + this.id.ToString());
            this.sw.Flush();
        }

        public bool Closed { get { return this.closed; } }

        public void Send(string message)
        {
            lock (this.sw)
            {
                this.sw.WriteLine(message);
                this.sw.Flush();
            }
        }

        public string ReedNext()
        {
            string input;

            lock (this.sr)
            {
                input = this.sr.ReadLine();
            }

            if (input == string.Empty || input == null)
            {
                return string.Empty;
            }
            else
            {
                return input;
            }
        }

        public void Close()
        {
            this.closed = true;
            try
            {
                sr.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

            try
            {
                sw.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
           
            try
            {
                networkstream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

            try
            {
                clientSocket.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
