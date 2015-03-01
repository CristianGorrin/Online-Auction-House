using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction_Items
{
    public class Items
    {
        private List<Item> _items;
        private int idCount;

        public Items()
        {
            this._items = new List<Item>();
            idCount = 1;
        }

        public void NewItem(Item _item)
        {
            lock (this._items)
            {
                this._items.Add(_item);
            }
        }

        public bool UpdatePrice(int id, double amount)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (this._items[i].ID == id)
                {
                    lock (this._items[i])
                    {
                        this._items[i].UpdatePrice(amount);
                        return true;
                    }
                }
            }

            return false;
        }

        public void AuctionSlot(int id,int to)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (this._items[i].ID == id)
                {
                    lock (this._items[i])
                    {
                        this._items[i].ItemSlot(to);
                        return;
                    }
                }
            }

            throw new ArgumentException("There is no item whit id: " + id.ToString());
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
    }

    public class Item
    {
        private int id;
        private string descripcion;
        private int byId;
        private double price;
        private int? soltToId;

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
            soltToId = null;
        }


        public int ID { get { return this.id; } }
        public string Descripcion { get { return this.descripcion; } set { this.descripcion = value; } }
        public int ByID { get { return this.byId; } }
        public double Price { get { return this.price; } }
        public bool Slot { get { return this.slot; } }
        public int? SlotTo { get { return this.soltToId; } }

        public void ItemSlot(int idTo)
        {
            this.slot = true;
        }

        public bool UpdatePrice(double amount)
        {
            if (amount > this.price && amount > 0)
            {
                this.price += amount;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
