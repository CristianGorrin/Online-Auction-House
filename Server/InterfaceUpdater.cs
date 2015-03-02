using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Server
{
    public class InterfaceUpdater
    {
        private const int UPDATE_INTERVAL = 5000;
        private const string THREAD_NAME = "Interface updater";
        private const string CONSOLE_TITLE = "Online Auction House";

        private bool runnig;
        private bool updateing;

        private Settings _settings;
        private Listener _listener;
        private ClientList clients;
        private Items auctionItems;

        private Thread task;

        public InterfaceUpdater(ref Settings _settings, ref Listener _listener, ref ClientList clients, ref Items auctionItems)
        {
            this.runnig = false;
            this.updateing = false;

            this._settings = _settings;
            this._listener = _listener;
            this.clients = clients;
            this.auctionItems = auctionItems;

            this.task = new Thread(new ThreadStart(Task));
            this.task.Name = THREAD_NAME;

            Console.Title = CONSOLE_TITLE + " - " + this._settings.ServerIP;

        }

        public void Start()
        {
            this.runnig = true;
            this.task.Start();
        }

        public void Close()
        {
            Console.WriteLine("Closing the program...");

            this.runnig = false;
            this.task.Join();
        }

        public void ForceUpdate()
        {
            if (!this.updateing)
                this.task.Interrupt();
        }

        private void Task()
        {
            while (this.runnig)
            {
                this.updateing = true;

                var list = new List<string>();

                if (this._listener.Runnig) list.Add("Server is running"); else list.Add("Server is not running");
                list.Add("Server queue size: " + this._settings.ServerQueueSize.ToString());
                list.Add("Connect: " + this.clients.Connect().ToString() + " of " + this._settings.ServerSize.ToString());
                list.Add("Auctions open: " + this.auctionItems.OpenAuctions().ToString());

                Console.Clear();
                foreach (var item in list)
                    Console.WriteLine(item);

                this.updateing = false;

                try
                {
                    Thread.Sleep(UPDATE_INTERVAL);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
