using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LatitudeClassLibrary;
using Phidget22;
using Phidget22.Events;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace VM
{  
    public partial class VMachine : Form
    {
        DataForEvent LatitudeVM;
        List<EventItem> itemsForVM;
        RFID rfidVM;
        Visitor currentVisitor;
        SellingOrder newSellOrder;
        ServicePoint myVM;
       
        public VMachine()
        {
            InitializeComponent();
            currentVisitor = null;
            newSellOrder = null;
            rfidVM = new RFID();
            rfidVM.Tag += rfidVM_Reader;
            LatitudeVM = new DataForEvent(1);
            LatitudeVM.SyncEventInfo();
            myVM = LatitudeVM.MyEvent.GetShopById(5); // This is vending machine id 5
            itemsForVM = new List<EventItem>();
            int count = 0;
            foreach(EventItem i in LatitudeVM.MyEvent.Items)
            {
                if (i is ItemForSale)
                {
                    if (((ItemForSale)i).IsSuitableForVM)
                    {
                        itemsForVM.Add(i);
                        count++;
                        if (i.QuantityInStock > 0)
                        {
                            cbSelectItemVM.Items.Add(count);
                        }
                    }
                }
            }
            GeneratePictureBox(itemsForVM, tabVM);
            GenerateLabel(itemsForVM, tabVM);
        }

        private void rfidVM_Reader(object sender, RFIDTagEventArgs e)
        {
            if (e.Tag != null)
            {
                currentVisitor = LatitudeVM.MyEvent.GetVisitorByRfid(e.Tag);
                if (currentVisitor != null)
                {
                    tbCustomerName.Text = currentVisitor.FirstName + " " + currentVisitor.LastName;
                    tbBalance.Text = string.Format("€{0:n2}", currentVisitor.Balance);
                    cbSelectItemVM.Enabled = true;
                    newSellOrder = new SellingOrder(LatitudeVM.GetNextOrderNumber() + 1, currentVisitor, myVM);
                    rfidVM.Close();
                }
                else
                {
                    MessageBox.Show("Cannot recognize visitor with this RFID tag");
                }
            }

        }

        private void GenerateLabel(List<EventItem> tempItemList, TabPage destination)
        {
            int x = 20;
            int y = 127;
            int width = 90;
            int height = 25;
            for (int i = 0; i < tempItemList.Count(); i++)
            {
                Label lbl = new Label();
                lbl.Location = new System.Drawing.Point(x, y);
                lbl.Name = "lbl" + tempItemList[i].Sku.ToString();
                lbl.Size = new System.Drawing.Size(width, height);
                lbl.Text = string.Format("#{0}: €{1:n2}", i+1, ((ItemForSale)tempItemList[i]).Price);
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                x += width + 23;

                if (x + width + 23 > destination.Width)
                {
                    x = 20;
                    y += height + 132;
                }
                destination.Controls.Add(lbl);
            }
        }

        private void GeneratePictureBox(List<EventItem> tempItemList, TabPage destination)
        {
            int x = 11;
            int y = 11;
            int width = 110;
            int height = 110;
            foreach (EventItem i in tempItemList)
            {
                PictureBox picture = new PictureBox();
                picture.Location = new System.Drawing.Point(x, y);
                picture.Name = "img" + i.Sku.ToString();
                picture.ImageLocation = i.ImgPath;
                picture.Size = new System.Drawing.Size(width, height);
                picture.BorderStyle = BorderStyle.FixedSingle;
                x += width + 5; // extra space

                if (x + width > destination.Width)
                {
                    x = 11;
                    y += height + 50;
                }
                destination.Controls.Add(picture);
            }
        }

        private void btnScanRFID_Click(object sender, EventArgs e)
        {
            rfidVM.Open();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult cancelOrder = MessageBox.Show("Are you sure you want to cancel this order?", "Cancelling...", MessageBoxButtons.YesNo);
            if (cancelOrder == DialogResult.Yes)
            {
                try
                {
                    newSellOrder.CancelOrder();
                    RefreshVMForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void RefreshVMForm()
        {
            btnScanRFID.Enabled = true;
            cbSelectItemVM.Items.Clear();
            for(int i = 0; i< itemsForVM.Count; i++)
            {
                int selectNr = i + 1;
                if (itemsForVM[i].QuantityInStock > 0)
                {
                    cbSelectItemVM.Items.Add(selectNr);
                }
            }
            cbSelectItemVM.Enabled = false;
            btnCancel.Enabled = false;
            btnPay.Enabled = false;
            tbCustomerName.Text = "";
            tbBalance.Text = "";
            lbVendingMachine.Items.Clear();
            lbVendingMachine.Items.Add("Welcome to Latitude!");
            lbVendingMachine.Items.Add("");
            lbVendingMachine.Items.Add("Please follow steps below to buy snack/ drink");
            lbVendingMachine.Items.Add("at our vending machine:");
            lbVendingMachine.Items.Add("1.Scan your RFID");
            lbVendingMachine.Items.Add("2.Select item nr. in the selection box");
            lbVendingMachine.Items.Add("3.Click pay & enjoy your selected snack / drink");
            lbVendingMachine.Items.Add("or cancel if you change your mind");
            lbVendingMachine.Items.Add("");
            lbVendingMachine.Items.Add("We hope you have a great time at Latitude!");
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            try
            {
                newSellOrder.PayOrder();
                if (newSellOrder.IsPaid)
                {
                    lbVendingMachine.Items.Clear();
                    lbVendingMachine.Items.Add("Please take out your selected item!");
                    DialogResult payOrder = MessageBox.Show("Do you want to print this receipt?", "Printing...", MessageBoxButtons.YesNo);
                    if (payOrder == DialogResult.Yes)
                    {
                        newSellOrder.PrintOrder();
                    }

                    // update database
                    LatitudeVM.InsertNewOrder(newSellOrder);

                    // refresh form
                    RefreshVMForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void cbSelectItemVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            EventItem item = itemsForVM[Convert.ToInt32(cbSelectItemVM.Text) - 1];
            newSellOrder.OrderLineItem.Clear();
            newSellOrder.AddItemToOrder(item, 1);
            btnCancel.Enabled = true;
            btnPay.Enabled = true;
            lbVendingMachine.Items.Clear();
            lbVendingMachine.Items.Add(string.Format("{0}, you have selected a {1}", currentVisitor.FirstName, item.Name));
        }
    }
}
