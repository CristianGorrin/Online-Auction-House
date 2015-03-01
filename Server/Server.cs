using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        private Settings _settings;
        private Listener _listener;
        private ClientList clients;
        private Items _items;
        private InterfaceUpdater iu;

        public Server()
        {
            this._settings = new Settings();
            this._items = new Items();
            this.clients = new ClientList(ref this._settings, ref this._items);
            this._listener = new Listener(ref this._settings, ref this.clients);
            this.iu = new InterfaceUpdater(ref this._settings, ref this._listener, ref this.clients, ref _items);

            this._items.Bind(ref this.clients);
        }

        public void Start()
        {
            this._listener.Start();
            this.clients.Start();
            this.iu.Start();
            this._items.Start();
        }

        public void Close()
        {
            this._items.Close();
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
