using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Client
{
    public class Controller
    {
        private const int UPDATE_INTERVAL = 5000;

        private bool running;

        private Connection server;
        private Thread task;

        private Queue<object> clientUpdates;

        public Controller(string ip, int port)
        {
            this.running = true;
            this.clientUpdates = new Queue<object>();

            this.server = new Connection(Connection.MeakSocket(ip, port));
            this.server.Start();

            this.task = new Thread(new ThreadStart(Task));
            this.task.Name = "Client tasks";
            this.task.Start();

            new Thread(() =>
            {
                while (this.running)
                {
                    this.server.Send("/listAuction");
                    Thread.Sleep(10000);
                }
            }).Start();
        }
        public bool BufferEmpty { get { if (this.clientUpdates.Count < 1) return true; else return false; } }
        public int ID { get { return this.server.ID; } }

        public bool NewAuction(string name, double price)
        {
            this.server.Send("/newAuction description=" + name + " startPrice=" + price);
            return this.server.GetRunurnBool();
        }

        public object Updates()
        {
            lock (this.clientUpdates)
                return this.clientUpdates.Dequeue();
        }

        private void Task()
        {
            while (this.running)
            {
                if (!this.server.BufferEmpty)
                {
                    string command = this.server.Get();
                    object result;

                    switch (CommandInterpreter.Interpret(command))
                    {
                        case CommandType.Error:
                            break;
                        case CommandType.ListAuction:
                            result = CommandInterpreter.InterpretAuction(command);
                            lock (this.clientUpdates)
                                this.clientUpdates.Enqueue(result);
                            break;
                        case CommandType.Id:
                            result = CommandInterpreter.InterpretId(command);
                            lock (this.clientUpdates)
                                this.clientUpdates.Enqueue(result);
                            break;
                        case CommandType.AuctionTik:
                            result = CommandInterpreter.InterpretAuctionTik(command);
                            lock (this.clientUpdates)
                                this.clientUpdates.Enqueue(result);
                            break;
                        case CommandType.AuctionSlot:
                            result = CommandInterpreter.InterpretAuctionSlot(command);
                            lock (this.clientUpdates)
                                this.clientUpdates.Enqueue(result);
                            break;
                        case CommandType.AuctionUpdate:
                            result = CommandInterpreter.InterpretAuctionUpdate(command);

                            lock (this.clientUpdates)
                                this.clientUpdates.Enqueue(result);
                            break;
                        default:
                            throw new ArgumentException();
                    }
                }
                else
                {
                    Thread.Sleep(UPDATE_INTERVAL);
                }
            }
        }

        public void Exit()
        {
            this.running = false;
            this.server.Close();
        }

        public bool Bid(string id, string amount)
        {
            this.server.Send("/bid id=" + id + " amount=" + amount.ToString());
            return this.server.GetRunurnBool();
        }

        private class Connection
        {
            private bool running;

            private Thread listener;
            
            private Socket _socket;
            private NetworkStream ns;
            private StreamReader sr;
            private StreamWriter sw;

            private Queue<string> commanders;
            private Queue<bool> returnBools;

            private int id;

            public Connection(Socket _socket)
            {
                this.id = -1;
                this.running = false;

                this._socket = _socket;
                this.ns = new NetworkStream(this._socket);
                this.sr = new StreamReader(this.ns);
                this.sw = new StreamWriter(this.ns);

                this.commanders = new Queue<string>();
                this.returnBools = new Queue<bool>();

                this.listener = new Thread(() => 
                {
                    while (this.running)
                    {
                        try
                        {
                            var temp = this.sr.ReadLine();
                            if (temp == "Accepted" || temp == "Reject")
                            {
                                bool input;

                                if (temp == "Accepted")
                                    input = true;
                                else
                                    input = false;

                                lock (this.returnBools)
                                    this.returnBools.Enqueue(input);
                            }
                            else
                            {
                                lock (this.commanders)
                                    this.commanders.Enqueue(temp);
                            }
                        }
                        catch (Exception)
                        {
                            this.running = false;
                        }
                    }
                });
                this.listener.Name = "Listener Task";
            }

            public bool Running { get { return this.running; } }
            public bool BufferEmpty { get {  if(this.commanders.Count < 1) return true; else return false; } }
            public int ID { get { return this.id; } }

            public void Send(string newLine)
            {
                lock (this.sw)
                {
                    this.sw.WriteLine(newLine);
                    this.sw.Flush();
                }
            }

            public string Get()
            {
                lock (this.commanders)
                    return this.commanders.Dequeue();
            }

            public bool GetRunurnBool()
            {
                for (int i = 0; i < 10; i++)
                    if (this.returnBools.Count > 0)
                        break; else Thread.Sleep(500);

                lock (this.returnBools)
                    try
                    {
                        return returnBools.Dequeue();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
            }

            public void Start()
            {
                if (this.running)
                    return;

                if (!this._socket.Connected)
                    throw new Exception("Is not connected to host");
                
                this.running = true;
                
                this.listener.Start();
            }

            public void Close()
            {
                this.running = false;

                Send("/close");

                try
                {
                    this.ns.Close();

                }
                catch (Exception)
                {
                    
                    throw;
                }

                try
                {
                    this._socket.Close();
                }
                catch (Exception)
                {
                    
                    throw;
                }

                Thread.Sleep(500);
                if (this.listener.IsAlive)
                    try
                    {
                        this.listener.Interrupt();

                        if (this.listener.IsAlive)
                            throw new Exception("the listener is not closet");

                    }
                    catch (Exception)
                    {
                        this.listener.Abort();
                        throw;
                    }
            }

            public static Socket MeakSocket(string ip, int port)
            {
                var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(new IPEndPoint(IPAddress.Parse(ip),port));

                return _socket;
            }
        }

        public static class CommandInterpreter
        {
            public static CommandType Interpret(string command)
            {
                switch (command.Split(' ')[0])
                {
                    case "/listAuction":
                        return CommandType.ListAuction;
                    case "/id":
                        return CommandType.Id;
                    case "/auctionTik":
                        return CommandType.AuctionTik;
                    case "/auctionSlot":
                        return CommandType.AuctionSlot;
                    case "/auctionUpdate":
                        return CommandType.AuctionUpdate;
                    default:
                        return CommandType.Error;
                }
            }

            public static Auctions InterpretAuction(string command)
            {
                if (command == "/listAuction null")
                    return new Auctions();

                var list = new Auctions();

                var input = command.Split(' ');
                input = input[1].Split('{');

                for (int i = 0; i < input[0].Length; i++)
                {
                    if (input[0][i] == ':')
                    {
                        var temp = input[1].Split(';');

                        int id = -1;
                        string description = string.Empty;
                        double price = -1D;

                        bool ok = true;

                        description = temp[1].Split('=')[1];

                        if (!int.TryParse(temp[0].Split('=')[1], out id))
                            ok = false;

                        if (!double.TryParse(temp[2].Split('=')[1].Split('}')[0], out price))
                            ok = false;

                        if (!ok)
                            return new Auctions();

                        list.Add(new Auction(id, description, price));

                        break;
                    }
                }

                if (list.Count < 1)
                {
                    for (int i = 1; i < input.Length; i++)
                    {
                        var temp = input[i].Split(';');

                        int id = -1;
                        string description = string.Empty;
                        double price = -1D;

                        bool ok = true;

                        description = temp[1].Split('=')[1];
                        if (!int.TryParse(temp[0].Split('=')[1], out id))
                            ok = false;

                        if (!double.TryParse(temp[2].Split('=')[1].Split('}')[0], out price))
                            ok = false;

                        if (ok)
                            list.Add(new Auction(id, description, price));
                    }
                }

                return list;
            }

            public static ID InterpretId(string command)
            {
                int id = -1;

                if (!int.TryParse(command.Split(' ')[1], out id))
                    throw new ArgumentException();

                return new ID(id);
            }

            public static AuctionTik InterpretAuctionTik(string command)
            {
                var input = command.Split(' ');

                int id = -1;
                int value = -1;

                if(!int.TryParse(input[1].Split('=')[1], out id))
                    throw new ArgumentException();

                if(!int.TryParse(input[2].Split('=')[1], out value))
                    throw new ArgumentException();

                return new AuctionTik(id, value);
            }

            public static AuctionSlot InterpretAuctionSlot(string command)
            {
                var input = command.Split(' ');

                int itemId = -1;
                int clientId = -1;

                if (!int.TryParse(input[1].Split('=')[1], out itemId))
                    throw new ArgumentException();

                if (!int.TryParse(input[2].Split('=')[1], out clientId))
                    throw new ArgumentException();

                return new AuctionSlot(itemId, clientId);
            }

            public static AuctionUpdate InterpretAuctionUpdate(string command)
            {
                var input = command.Split(' ');

                int id = -1;
                double price = -1D;

                if(!int.TryParse(input[1].Split('=')[1], out id))
                    return null;

                if (!double.TryParse(input[2].Split('=')[1], out price))
                    return null;

                return new AuctionUpdate(id, price);
            }

        }
    }

    public enum CommandType { Error, ListAuction, Id, AuctionTik, AuctionSlot, AuctionUpdate }
}
