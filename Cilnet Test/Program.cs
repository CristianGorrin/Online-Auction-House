using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Cilnet_Test
{
    class Program
    {
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 12345;
        private const int UPDATE_INTERVAL = 5000;

        static void Main(string[] args)
        {
            bool exit = false;

            var endPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

            var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(endPoint);

            var networkstream = new NetworkStream(server);
            var sr = new StreamReader(networkstream);
            var sw = new StreamWriter(networkstream);

            new Thread(() => 
            {
                while (!exit)
                {
                    bool run = true;

                    while (run)
                    {
                        string temp = string.Empty;
                        
                        try
                        {
                            temp = sr.ReadLine();
                        }
                        catch (Exception e)
                        {
                            temp = e.ToString();
                            exit = true;
                            run = false;
                        }

                        if (temp == null || temp == string.Empty)
                        {
                            run = false;
                        }
                        else
                        {
                            Console.WriteLine(temp);
                        }
                    }

                    Thread.Sleep(UPDATE_INTERVAL);
                }
            }).Start();

            while (!exit)
            {
                var input = Console.ReadLine();

                sw.WriteLine(input);
                
                try
                {
                    sw.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    exit = true;
                }
            }

            Console.ReadLine();
        }
    }
}
