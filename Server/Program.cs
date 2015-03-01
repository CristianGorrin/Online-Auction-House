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
                var input = Console.ReadKey();

                switch (input.Key)
                {
                    case ConsoleKey.Escape:
                        exit = true;
                        obj.Close();
                        break;
                    case ConsoleKey.Spacebar:
                        obj.InterdaceUpdate();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
