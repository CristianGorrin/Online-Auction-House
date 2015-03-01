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

        public Items()
        {
            this._items = new List<Item>();
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

        public void CloseAuction(int id)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (this._items[i].ID == id)
                {
                    lock (this._items[i])
                    {
                        this._items[i].ItemSlot();
                        return;
                    }
                }
            }

            throw new ArgumentException("There is no item whit id: " + id.ToString());
        }
    }

    public class Item
    {
        private int id;
        private string descripcion;
        private int byId;
        private double price;

        private bool slot;

        public Item(int id, string descripcion, int byId, double startPrice)
        {
            if (startPrice < 0)
                throw new ArgumentException("The start price can't be less then 0");
            
            this.slot = false;

            this.id = id;
            this.descripcion = descripcion;
            this.byId = byId;
            this.price = startPrice;
        }


        public int ID { get { return this.id; } }
        public string Descripcion { get { return this.descripcion; } set { this.descripcion = value; } }
        public int ByID { get { return this.byId; } }
        public double Price { get { return this.price; } }
        public bool Slot { get { return this.slot; } }

        public void ItemSlot()
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
