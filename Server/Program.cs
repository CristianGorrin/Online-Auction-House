using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;

            var obj = new Server();
            obj.Start();

            while (!exit)
            {
                var input = Console.ReadLine();

                switch (input.ToLower())
                {
                    case "exit":
                        exit = true;
                        obj.Close();
                        break;
                    default:
                        break;
                }
            }

        }
    }
}
