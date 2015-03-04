using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool running;

        private Auctions _auctions;
        private Controller _controller;
        private Thread worker;

        public MainWindow()
        {
            InitializeComponent();
            this.running = true;
            this._auctions = new Auctions();

            this._controller = new Controller("127.0.0.1", 12346);
            this.worker = new Thread(new ThreadStart(Task));

            this.worker.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._controller.Exit();
            this.running = false;

            this.worker.Join();
        }

        private void Task()
        {
            while (this.running)
            {
                if (!this._controller.BufferEmpty)
                {
                    var obj = this._controller.Updates();
                    string temp = string.Empty;

                    switch (((Type) obj).Type)
                    {
                        case CommandType.Error:
                            throw new InvalidOperationException();
                        case CommandType.ListAuction:
                            ListAuctionUpdate((Auctions)obj);
                            break;
                        case CommandType.Id:
                            this.Dispatcher.Invoke((Action) (() => { this.Title += " - ID " + ((ID)obj).Id.ToString(); }));
                            break;
                        case CommandType.AuctionTik:
                            AuctionTikUpdate((AuctionTik)obj);
                            break;
                        case CommandType.AuctionSlot:
                            AuctionSlotUpdate((AuctionSlot)obj);
                            break;
                        case CommandType.AuctionUpdate:
                            AuctionUpdate((AuctionUpdate)obj);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private void btnAuctionSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtAuctionName.Text == string.Empty)
                MessageBox.Show("plz enter a name of the new auction");

            double price = -1;

            if (!double.TryParse(this.txtAuctionPrice.Text, out price) || this.txtAuctionPrice.Text == string.Empty)
                MessageBox.Show("plz enter a start price of the auction");

            if (!this._controller.NewAuction(this.txtAuctionName.Text, price))
                MessageBox.Show("The auction wanes accepted");

            this.Dispatcher.Invoke((Action)(() =>
            {
                this.txtAuctionName.Text = string.Empty;
                this.txtAuctionPrice.Text = string.Empty;
            }));
        }

        private void dgvAuctionList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedIndex == -1)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {

                    this.btnBidSubmit.IsEnabled = false;
                    this.txtBidAmount.IsEnabled = false;

                    this.txtBidAmount.Text = string.Empty;
                    this.txtBidID.Text = string.Empty;
                }));
            }
            else
            {
                var temp = (Client.Auction)((DataGrid)sender).SelectedItem;

                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (this.txtBidID.Text != temp.ID.ToString())
                        this.txtBidAmount.Text = string.Empty;

                    this.txtBidID.Text = temp.ID.ToString();

                    this.btnBidSubmit.IsEnabled = true;
                    this.txtBidAmount.IsEnabled = true;
                }));
            }
        }

        private void btnBidSubmit_Click(object sender, RoutedEventArgs e)
        {
            string id = string.Empty;
            string amount = string.Empty;

            this.Dispatcher.Invoke((Action)(() =>
            {
                id = this.txtBidID.Text;
                amount = this.txtBidAmount.Text;
            }));

            try
            {
                int.Parse(id);
                double.Parse(amount);
            }
            catch (Exception)
            {
                MessageBox.Show("Can't use the value for bid");
                return;
            }

            if (!this._controller.Bid(id, amount))
                MessageBox.Show("Reject by the server" );

            this.Dispatcher.Invoke((Action)(() =>
            {
                this.txtBidAmount.Text = string.Empty;
            }));
        }

        private void AuctionTikUpdate(AuctionTik tik)
        {
            string temp = string.Empty;

            temp = "[" + DateTime.Now.ToString() + "] ";

            var _auction = this._auctions.Find(tik.ID);

            if (_auction != null)
            {
                temp += "(" + _auction.ID + ") Description: " + _auction.Description;
            }
            else
            {
                temp += " Id: " + tik.ID;
            }

            if (tik.Value == 0) temp += " - First"; else temp += " - Second";

            this.Dispatcher.Invoke((Action)(() =>
            {
                this.txtAuctionEvents.Text += temp + Environment.NewLine;
            }));
        }

        private void AuctionSlotUpdate(AuctionSlot update)
        {
            var temp = "[" + DateTime.Now.ToString() + "] ";

            temp += "Auction Slot: item " + update.ItemId + " to client id" + update.ClientId;

            this.Dispatcher.Invoke((Action) (() =>
            {
                this.txtAuctionEvents.Text += temp + Environment.NewLine;
            }));
        }

        private void ListAuctionUpdate(Auctions list)
        {
            this._auctions = list;

            var temp = new List<DG>();

            foreach (var item in this._auctions.List)
                temp.Add((DG)item);

            object selObj = null;

            this.Dispatcher.Invoke((Action)(() =>
            {
                string[] txt = { this.txtBidID.Text, this.txtBidAmount.Text };

                if (this.dgvAuctionList.SelectedIndex != -1)
                    selObj = this.dgvAuctionList.SelectedItem;

                this.dgvAuctionList.ItemsSource = temp;

                if (selObj != null)
                {
                    int count = -1;

                    foreach (var item in this.dgvAuctionList.ItemsSource)
                    {
                        count++;
                        if (((DG)item).ID == ((DG)selObj).ID)
                        {
                            this.dgvAuctionList.SelectedIndex = count;

                            this.txtBidID.Text = txt[0];
                            this.txtBidAmount.Text = txt[1];
                        }
                    }
                }
            }));
        }

        private void AuctionUpdate(AuctionUpdate update)
        {
            this._auctions.BidUpdate(update.ItemId, update.Price);

            this.Dispatcher.Invoke((Action)(() =>
            {
                this.txtAuctionEvents.Text += "[" + DateTime.Now.ToString() + "] Bid on item " + update.ItemId + 
                    ", new price " + update.Price + Environment.NewLine;
                this.dgvAuctionList.ItemsSource = this._auctions.ToDGV();
            }));
        }
    }
}
