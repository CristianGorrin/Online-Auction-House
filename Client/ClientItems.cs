using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Auctions : Type
    {
        private List<Auction> list;

        public Auctions()
        {
            this.list = new List<Auction>();

        }

        public int Count { get { return this.list.Count; } }
        public CommandType Type { get { return CommandType.ListAuction; } }
        public List<Auction> List { get { return this.list; } }

        public void Add(Auction item)
        {
            this.list.Add(item);
        }

        public void UpdateingAuctionTik(AuctionTik obj)
        {
            for (int i = 0; i < this.list.Count; i++)
                if (this.list[i].ID == obj.ID)
                    lock (this.list[i])
                        this.list[i].AuctionTik = obj.Value;
        }

        public Auction Find(int id)
        {
            foreach (var item in this.list)
            {
                if (item.ID == id)
                {
                    return item;
                }
            }

            return null;
        }

        public bool BidUpdate(int id, double newPrice)
        {
            for (int i = 0; i < this.list.Count; i++)
            {
                if (this.list[i].ID == id)
                {
                    this.list[i].Price = newPrice;
                    return true;
                }
            }

            return false;
        }

        public List<DG> ToDGV()
        {
            var tempList = new List<DG>();

            foreach (var item in this.list)
                tempList.Add((DG)item);

            return tempList;
        }
    }

    public class Auction : DG
    {
        private int itemID;
        private string description;
        private double price;
        
        private int auctionTik;

        public Auction(int id, string description, double price)
        {
            this.itemID = id;
            this.description = description;
            this.price = price;

            this.auctionTik = -1;
        }

        public int ID { get { return this.itemID; } set { this.itemID = value; } }
        public string Description { get { return this.description; } set { this.description = value; } }
        public double Price { get { return this.price; } set { this.price = value; } } 
        public int AuctionTik{ get { return this.auctionTik;} set { this.auctionTik = value; } }
    }

    public class AuctionTik : Type
    {
        private int id;
        private int value;

        public AuctionTik(int id, int value)
        {
            this.id = id;
            this.value = value;
        }

        public int ID { get { return this.id; } }
        public int Value { get { return this.value; } }
        public CommandType Type { get { return CommandType.AuctionTik; } }
    }

    public class AuctionSlot : Type
    {
        private int itemId;
        private int clientId;

        public AuctionSlot(int itemId, int clientId)
        {
            this.itemId = itemId;
            this.clientId = clientId;
        }

        public int ItemId { get { return this.itemId; } }
        public int ClientId { get { return this.clientId; } }
        public CommandType Type { get { return CommandType.AuctionSlot; } }
    }

    public class AuctionUpdate : Type
    {
        private int itemId;
        private double price;

        public AuctionUpdate(int itemId, double price)
        {
            this.itemId = itemId;
            this.price = price;
        }

        public int ItemId { get { return this.itemId; } }
        public double Price { get { return this.price; } }

        public CommandType Type { get { return CommandType.AuctionUpdate; } }
    }

    public class ID : Type
    {
        public int Id { get; private set; }

        public ID(int id)
        {
            this.Id = id;
        }

        public CommandType Type { get { return CommandType.Id; } }
    }

    public interface Type
    {
        CommandType Type { get; }
    }

    public interface DG
    {
        int ID { get; }
        string Description { get; }
        double Price { get; }
    }
}
