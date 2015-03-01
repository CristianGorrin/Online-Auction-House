using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Auction_Items;

namespace Server
{
    public class Server
    {
        private Settings _settings;
        private Listener _listener;
        private ClientList clients;
        private Auction_Items.Items _items;
        private InterfaceUpdater iu;

        public Server()
        {
            this._settings = new Settings();
            this._items = new Items();
            this.clients = new ClientList(ref this._settings, ref this._items);
            this._listener = new Listener(ref this._settings, ref this.clients);
            this.iu = new InterfaceUpdater(ref this._settings, ref this._listener, ref this.clients, ref _items);

        }

        public void Start()
        {
            this._listener.Start();
            this.clients.Start();
            this.iu.Start();
        }

        public void Close()
        {
            this._listener.Close();
            this.clients.Close();
            this.iu.Close();
        }

        public void InterdaceUpdate()
        {
            this.iu.ForceUpdate();
        }
    }
}
