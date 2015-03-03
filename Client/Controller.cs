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
        private Connection server;

        public Controller(string ip, int port)
        {
            this.server = new Connection(Connection.MeakSocket(ip, port));
            this.server.Start();
        }

        public void Exit()
        {
            this.server.Close();
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

                this.listener = new Thread(() => 
                {
                    while (this.running)
                    {
                        try
                        {
                            var temp = this.sr.ReadLine();

                            lock (this.commanders)
                                this.commanders.Enqueue(temp);
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

            public void Send(string newLine)
            {
                lock (this.sw)
                    this.sw.WriteLine(newLine);
            }

            public string Get()
            {
                lock (this.commanders)
                    return this.commanders.Dequeue();
            }

            public void Start()
            {
                if (this.running)
                    return;

                if (!this._socket.Connected)
                    throw new Exception("Is not connected to host");
                
                this.running = true;
                this.listener.Start();
                
                new Thread(() =>
                {
                    int tempId = -1;

                    for (int i = 0; i < 10; i++)
                    {
                        if (!this.BufferEmpty)
                        {
                            var temp = this.Get();

                            if (temp.Split(' ')[0] != "/id")
                            {
                                break;
                            }
                            else
                            {
                                if (int.TryParse(temp.Split(' ')[1], out tempId))
                                    break;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (tempId == -1)
                    {
                        this.Close();
                    }
                    else
                    {
                        this.id = tempId;
                    }
                }).Start();
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
                    case "auctionTik":
                        return CommandType.AuctionTik;
                    case "auctionSlot":
                        return CommandType.AuctionSlot;
                    default:
                        return CommandType.Error;
                }
            }

            public static List<Auction> InterpretAuction(string command)
            {
                if (command == "/listAuction null")
                    return null;

                var list = new List<Auction>();

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
                            return null;

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

            public static int InterpretId(string command)
            {
                int id = -1;

                if (!int.TryParse(command.Split(' ')[1], out id))
                    throw new ArgumentException();

                return id;
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

            public enum CommandType { Error, ListAuction, Id, AuctionTik, AuctionSlot }
        }
    }
}
