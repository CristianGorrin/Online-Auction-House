﻿using System;
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

        private Auction_Items.Items _items;

        public ClientList(ref Settings _settings, ref Items _items)
        {
            this.clientObjects = new ClientObject[_settings.ServerSize];

            this.runing = false;

            this._items = _items;

            this.worker = new Thread(new ThreadStart(Taskworker));
            this.worker.IsBackground = false;
            this.worker.Name = THREAD_NAME_WORKER;
        }

        public void Broadcaster(string message)
        {
            lock (this.clientObjects)
            {
                for (int i = 0; i < this.clientObjects.Length; i++)
                {
                    if (this.clientObjects[i] != null)
                        if (!this.clientObjects[i].Closed)
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
                            if (this.clientObjects[i].Closed || !clientObjects[i].Connected)
                            {
                                lock (this.clientObjects[i])
                                    this.clientObjects[i] = null;
                            }
                            else
                            {
                                string clientCommand = string.Empty;
                                string returnStement = string.Empty;

                                lock (this.clientObjects[i])
                                    clientCommand = clientObjects[i].ReedNext();

                                if (clientCommand != string.Empty)
                                {
                                    string[] command = clientCommand.Split(' ');

                                    if (command[0] == "/close")
                                    {
                                        lock (this.clientObjects[i])
                                        {
                                            this.clientObjects[i].Close();
                                        }
                                    }
                                    else if (command[0] == "/newAuction")
                                    {
                                        if (command.Length < 3)
                                        {
                                            returnStement = DefaultMessaging(clientCommand);
                                        }
                                        else
                                        {
                                            NewAuction(command, this.clientObjects[i].ID, out returnStement);
                                        }
                                    }
                                    else if (command[0] == "/bid")
                                    {
                                        BidAuction(command, this.clientObjects[i].ID, out returnStement);
                                    }
                                    else if (command[0] == "/listAuction")
                                    {
                                        if (command.Length > 1)
                                        {
                                            if (command[1].Split('=')[0] == "id")
                                            {
                                                int id = -1;

                                                if(int.TryParse(command[1].Split('=')[1], out id))
                                                    AuctionById(out returnStement, id);
                                                else
                                                    returnStement = DefaultMessaging(clientCommand);
                                            }
                                        }
                                        else
                                        {
                                            ListAuction(out returnStement);
                                        }
                                    }
                                    else
                                    {
                                        returnStement = DefaultMessaging(clientCommand);
                                    }
                                    
                                    if (returnStement != string.Empty)
                                        lock (this.clientObjects[i])
                                            this.clientObjects[i].Send(returnStement);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!this.runing)
                        break;

                    this.runing = false;
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
                            this.clientObjects[i].Close();
                }
            }
        }

        public void AddClient(ClientObject obj)
        {
            lock (this.clientObjects)
            {
                for (int i = 0; i < this.clientObjects.Length; i++)
                {
                    if (this.clientObjects[i] == null)
                    {
                        this.clientObjects[i] = obj;
                        break;
                    }
                }
            }
        }

        public int Connect()
        {
            int count = 0;

            for (int i = 0; i < this.clientObjects.Length; i++)
            {
                if (this.clientObjects[i] != null)
                    count++;
            }

            return count;
        }

        private string DefaultMessaging(string text)
        {
            return "Command \"" + text + "\" is not recognized.";
        }

        private bool BidAuction(string[] command, int byId, out string messaging)
        {
            bool ok = true;

            int itemId = -1;
            double amount = -1;

            if (!(command[1].Split('=')[0] == "id" && int.TryParse(command[1].Split('=')[1], out itemId)))
                ok = false;

            if (!(command[2].Split('=')[0] == "amount" && double.TryParse(command[2].Split('=')[1], out amount)))
                ok = false;

            if (ok)
                ok = this._items.Bid(itemId, amount, byId);
            

            if (ok)
            {
                messaging = "Accepted";
                return true;
            }
            else
            {
                messaging = "Reject";
                return false;
            }
        }

        private bool NewAuction(string[] command, int byId, out string messaging)
        {
            bool ok = true;

            string description = string.Empty;
            double startPrice = -1;

            if (command[1].Split('=')[1].Length > 0 && command[1].Split('=')[0] == "description")
            {
                description = command[1].Split('=')[1];
            }
            else
            {
                ok = false;
            }


            if (!double.TryParse(command[2].Split('=')[1], out startPrice) && command[2].Split('=')[0] == "startPrice")
                ok = false;


            if (ok)
            {
                var item = new Auction_Items.Item(this._items.NextId, description, byId, startPrice);
                this._items.NewItem(item);

                messaging = "Accepted";
                return true;
            }
            else
            {
                messaging = "Reject";
                return false;
            }
        }

        private void ListAuction(out string messaging)
        {
            var temp = "/listAuction ";

            var list = this._items.GetList(false);
            if (list.Count > 0)
            {
                foreach (var item in list)
                    temp += "{itemID=" + item.ID + ";description=" + item.Descripcion + ";price=" + item.Price + "}";
            }
            else
            {
                temp += "null";
            }

            messaging = temp;
        }

        private void AuctionById(out string messaging, int id)
        {
            string temp = string.Empty;

            var item = this._items.FindById(id);

            if (item == null)
                temp = "/listAuction " + id + ":null";
            else
                temp = "/listAuction " + id + ":{itemID=" + id + ";description=" + item.Descripcion
                    + ";price=" + item.Price + "}";

            messaging = temp;
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

        private Queue<string> clientCommands;

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

            this.clientCommands = new Queue<string>();

            var _thread = new Thread(() =>
            {
                try
                {
                    string temp;

                    while (!this.closed)
                    {
                        lock (this.sr)
                            temp = this.sr.ReadLine();

                        lock (this.clientCommands)
                            this.clientCommands.Enqueue(temp);

                        temp = string.Empty;
                    }
                }
                catch (Exception)
                {
                    this.closed = true;
                }
            });
            _thread.Name = "ClientThread_" + id;
            _thread.Start();
        }

        public bool Closed { get { return this.closed; } }
        public int ID { get { return this.id; } }
        public bool Connected { get { return this.clientSocket.Connected; } }
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
            if (this.clientCommands.Count > 0)
            {
                return this.clientCommands.Dequeue();
            }
            else
            {
                return string.Empty;
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
