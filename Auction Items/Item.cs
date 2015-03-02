using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Auction_Items
{
    public class Items
    {
        protected List<Item> _items;
        protected int idCount;

        public Items()
        {
            this._items = new List<Item>();
            idCount = 1;
        }

        public virtual void NewItem(Item _item)
        {
            lock (this._items)
            {
                this._items.Add(_item);
            }
        }

        public virtual bool UpdatePrice(int id, double amount)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (this._items[i].ID == id)
                {
                    lock (this._items[i])
                    {
                        if (!this._items[i].Slot)
                        {
                            return this._items[i].UpdatePrice(amount, id);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return false;
        }

        public int OpenAuctions()
        {
            int count = 0;

            foreach (var item in this._items)
            {
                if (!item.Slot)
                    count++;
            }

            return count;
        }

        public int NextId { get { return this.idCount++; } }

        public virtual bool Bid(int itemId, double amount, int byId)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (this._items[i].ID == itemId)
                {
                    lock (this._items[i])
                    {
                        return this._items[i].UpdatePrice(amount, byId);
                    }
                }
            }

            return false;
        }

        public List<Item> GetList(bool slot)
        {
            var list = new List<Item>();

            foreach (var item in this._items)
            {
                if (item.Slot == slot)
                    list.Add(item);
            }

            return list;
        }

        public virtual Item FindById(int id)
        {
            for (int i = 0; i < this._items.Count; i++)
                if (this._items[i].ID == id)
                    return this._items[i];

            return null;
        }
    }

    public class Item
    {
        private int id;
        private string descripcion;
        private int byId;
        private double price;
        private int? toCilnetId;

        private DateTime? lastBid;
        private int broadcastedTimes;

        private bool slot;

        public Item(int id, string description, int byId, double startPrice)
        {
            if (startPrice < 0)
                throw new ArgumentException("The start price can't be less then 0");
            
            this.slot = false;

            this.id = id;
            this.descripcion = description;
            this.byId = byId;
            this.price = startPrice;
            this.toCilnetId = null;
            this.broadcastedTimes = 0;
            this.lastBid = null;
        }


        public int ID { get { return this.id; } }
        public string Descripcion { get { return this.descripcion; } set { this.descripcion = value; } }
        public int ByID { get { return this.byId; } }
        public double Price { get { return this.price; } }
        public bool Slot { get { return this.slot; } }
        public int? ToCilnetId { get { return this.toCilnetId; } }
        public DateTime? LastBid { get {  return this.lastBid;} }
        public int BroadcastedTimes { get { return this.broadcastedTimes; } }

        public void ItemSlot()
        {
            this.slot = true;
        }

        public void Broadcasted()
        {
            this.broadcastedTimes++;
        }

        public bool UpdatePrice(double amount, int cilnetId)
        {
            if (lastBid != null)
                if (this.lastBid <= DateTime.Now.AddSeconds(-30))
                    return false;

            if (amount > this.price)
            {
                this.lastBid = DateTime.Now;
                this.broadcastedTimes = 0;

                this.price = amount;
                this.toCilnetId = cilnetId;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
