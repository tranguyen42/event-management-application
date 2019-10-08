using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Phidget22;
using Phidget22.Events;
using System.IO;
using LatitudeClassLibrary;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace latittude
{
    public partial class Form1 : Form
    {
        // RFID
        private RFID rfid_CheckIn;
        private RFID rfid_CheckOut;
        private RFID rfid_Camping;
        private RFID rfid_Buying;
        private RFID rfid_Renting;
        private RFID rfid_Lost;
        

       // global variables
        private Event myEvent;
        private Employee currentUser;
        private Visitor currentVisitor;
        private double amountToPay;
        private double balance;
        private double ticketPrice;
        private double campFee;
        private DataForEvent LatitudeEvent;
        Dictionary<EventItem, int> tempOrder; 
        SellingOrder newSellOrder;
        RentingOrder newRentingOrder;

        public Form1()
        {
            InitializeComponent();

            // RFID
            rfid_CheckIn = new RFID();
            rfid_CheckIn.Tag += rfid_CheckIn_Reader;
            rfid_CheckOut = new RFID();
            rfid_CheckOut.Tag += rfid_CheckOut_Reader;
            rfid_Camping = new RFID();
            rfid_Camping.Tag += rfid_Camping_Reader;
            rfid_Buying = new RFID();
            rfid_Buying.Tag += rfid_Buying_Reader;
            rfid_Renting = new RFID();
            rfid_Renting.Tag += rfid_Renting_Reader;
            rfid_Lost = new RFID();
            rfid_Lost.Tag += rfid_Lost_Reader;

            // global variables in app
            currentUser = null;
            currentVisitor = null;
            balance = 0;
            ticketPrice = EventPriceList.RegularAtGateTicket; // default is regular ticket
            campFee = 0;
            amountToPay = ticketPrice + campFee + balance;
            tbTotalToPay.Text = string.Format("{0:n2}", amountToPay);
            tempOrder = new Dictionary<EventItem, int>();
            newSellOrder = null;
            newRentingOrder = null;

            // set datetimepicker max & min value
            dateTimeDOB.MaxDate = DateTime.Now.AddYears(-18); // dob of employees
            dateTimeDOB.MinDate = DateTime.Now.AddYears(-65);
            datePickerVisitor.MaxDate = DateTime.Now.AddYears(-15); // dob of visitors
            datePickerVisitor.MinDate = DateTime.Now.AddYears(-90);

            // set enablity of tabPage
            tabPage8.Enabled = false; // tabPage Selling
            tabPage9.Enabled = false; // tabPage Lending

            // set visibility
            pnlLogIn.Visible = true;
            pnlMainMenu.Visible = false;

            // set password textbox
            textBox2.PasswordChar = '*'; // display * for password textbox
            tbPasswordEmp.PasswordChar = '*';
            tbPassword.PasswordChar = '*';

            // set controls' location
            tbcServicePoint.Location = new Point(tbcEntranceCheck.Location.X, tbcEntranceCheck.Location.Y);
            tbcStockManagement.Location = new Point(tbcEntranceCheck.Location.X, tbcEntranceCheck.Location.Y);
            tbcEmployeeManagement.Location = new Point(tbcEntranceCheck.Location.X, tbcEntranceCheck.Location.Y);
            pnlMyAccount.Location = new Point(tbcEntranceCheck.Location.X, tbcEntranceCheck.Location.Y);
            pnlEventManagement.Location = new Point(tbcEntranceCheck.Location.X, tbcEntranceCheck.Location.Y);
            pnlUpdateNewItem.Location = new Point(pnlUpdateExistingItem.Location.X, pnlUpdateExistingItem.Location.Y);
            pnlNewForRent.Location = new Point(pnlNewForSale.Location.X, pnlNewForSale.Location.Y);
            pnlExistingEmp.Location = new Point(pnlNewEmp.Location.X, pnlNewEmp.Location.Y);
            pnlSpotCode.Location = new Point(pnlReserveNewCamp.Location.X, pnlReserveNewCamp.Location.Y);

            // load data from database
            LatitudeEvent = new DataForEvent(1);
            LatitudeEvent.SyncEventInfo();
            myEvent = LatitudeEvent.MyEvent;
           

            // setup FileWatcher
            logFileWatcher.Created += new FileSystemEventHandler(OnChanged);
            logFileWatcher.EnableRaisingEvents = true;

            // read all current logFiles in logFiles folder, then move read files to oldLogFiles folder
            var txtFiles = Directory.EnumerateFiles("../../../documents/logFiles/", "*.txt");
            foreach (string currentPath in txtFiles)
            {
                int id = LatitudeEvent.GetNextLogFileId() + 1;
                LogFile newLog = new LogFile(id);
                try
                {
                    newLog.ReadLogFile(Path.GetFileName(currentPath));
                    newLog.MoveFile(Path.GetFileName(currentPath));
                    // top up account
                    TopUpAccount(newLog.EventAccountTopUp);
                    // update database
                    LatitudeEvent.InsertNewLog(newLog); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // fileWatcher event
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                int id = LatitudeEvent.GetNextLogFileId() + 1;
                LogFile newFile = new LogFile(id);
                if (newFile.ReadLogFile(e.Name))
                {
                    newFile.MoveFile(e.Name);
                }
                // top up account
                TopUpAccount(newFile.EventAccountTopUp);
                // update database
                LatitudeEvent.InsertNewLog(newFile);
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
        }

        private void TopUpAccount(List<KeyValuePair<int, double>> myList)
        {
            foreach (KeyValuePair<int, double> pair in myList)
            {
                Visitor v = myEvent.GetVisitorByEventAccount(pair.Key);
                if (v != null)
                {
                    v.Deposit(pair.Value);
                }
                else
                {
                    throw new LatitudeException("Top up failed! Cannot find visitor with event account " + pair.Key);
                }
            }
        }

        // RFID event
        private void rfid_CheckIn_Reader(object sender, RFIDTagEventArgs e)
        {
            if (e.Tag != null)
            {
                if (myEvent.GetVisitorByRfid(e.Tag) == null)
                {
                    currentVisitor.AssignNewRfid(e.Tag);
                    btnShowCheckInVisitors_Click(this, new EventArgs());
                    LatitudeEvent.UpdateCheckInVisitor(currentVisitor);
                    rfid_CheckIn.Close();
                }
                else
                {
                    MessageBox.Show("This RFID tag has been given to another visitor. Please put another RFID tag on scanner");
                }
            }
        }

        private void rfid_Lost_Reader(object sender, RFIDTagEventArgs e)
        {
            if (e.Tag != null)
            {
                if (myEvent.GetVisitorByRfid(e.Tag) == null)
                {
                    currentVisitor.AssignNewRfid(e.Tag);
                    LatitudeEvent.UpdateCheckInVisitor(currentVisitor);
                    lbLostRFIDInfo.Items.Clear();
                    lbLostRFIDInfo.Items.Add("New RFID tag given: " + currentVisitor.Rfid);
                    btnShowCheckInVisitors_Click(this, new EventArgs());
                    rfid_Lost.Close();
                    
                }
                else
                {
                    MessageBox.Show("This RFID tag has been given to another visitor. Please put another RFID tag on scanner");
                }
            }
        }

        private void rfid_CheckOut_Reader(object sender, RFIDTagEventArgs e)
        {

            if (e.Tag != null)
            {
                Visitor v = myEvent.GetVisitorByRfid(e.Tag);
                if (v != null)
                {
                    try
                    {
                        v.CheckOut();
                        LatitudeEvent.UpdateCheckOutVisitor(v);
                        lbVisitorDetail.Items.Clear();
                        lbVisitorDetail.Items.Add("Visitor with ticket nr. " + v.TicketNr + " checked out");
                        btnShowCheckInVisitors_Click(this, new EventArgs());
                        rfid_CheckOut.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("This RFID tag hasn't been given to any visitor. Please try again!");
                }          
            }
        }

        private void rfid_Camping_Reader(object sender, RFIDTagEventArgs e)
        {
            if (e.Tag != null)
            {
                currentVisitor = myEvent.GetVisitorByRfid(e.Tag);
                if (currentVisitor != null)
                {
                    lbSpotsInfo.Items.Clear();
                    foreach (string s in Regex.Split(currentVisitor.AsAString(), "\n"))
                    {
                        lbSpotsInfo.Items.Add(s);
                    }
                    if (currentVisitor.HasReserveCamp)
                    {
                        tbStatusSpotCheck.Text = "yes";
                        cbSpotId.Enabled = false;
                        cbSpotId.Items.Clear();
                        cbSpotId.Items.Add(currentVisitor.ReservedSpot.SpotId);
                        cbSpotId.Text = cbSpotId.Items[0].ToString();
                    }
                    else
                    {
                        tbStatusSpotCheck.Text = "not yet";
                        cbSpotId.Enabled = true;
                        cbSpotId.Items.Clear();
                        foreach (CampingSpot spot in myEvent.Spots)
                        {
                            if (currentVisitor.TicketType == Ticket.GroupAtGate || currentVisitor.TicketType == Ticket.GroupOnline)
                            {
                                if (spot.Renters.Count == 0)
                                {
                                    cbSpotId.Items.Add(spot.SpotId);
                                }
                            }
                            else
                            {
                                if (spot.IsAvailable)
                                {
                                    cbSpotId.Items.Add(spot.SpotId);
                                }
                            }
                        }
                        cbSpotId.Text = "";
                    }
                }
                else
                {
                    // clear controls
                    tbStatusSpotCheck.Text = "";
                    cbSpotId.Items.Clear();
                    lbSpotsInfo.Items.Clear();
                    MessageBox.Show("This RFID tag hasn't been given to any visitor");
                }
            }
        }

        private void rfid_Buying_Reader(object sender, RFIDTagEventArgs e)
        {
            if (e.Tag != null)
            {
                currentVisitor = myEvent.GetVisitorByRfid(e.Tag);
                if (currentVisitor != null)
                {
                    pnlNewOrder.Enabled = true;
                    btnScanRFIDSell.Enabled = false;
                    cbSellingShopList.Enabled = false;
                    tbCustomerInfo.Text = currentVisitor.FirstName + " " + currentVisitor.LastName;
                    tbCustomerBalance.Text = string.Format("{0:n2}", currentVisitor.Balance);
                }
                else
                {
                    MessageBox.Show("No visitor found with RFID tag " + e.Tag);
                }
            }
        }

        private void rfid_Renting_Reader(object sender, RFIDTagEventArgs e)
        {
            if (e.Tag != null)
            {
                currentVisitor = myEvent.GetVisitorByRfid(e.Tag);
                if (currentVisitor != null)
                {
                    MakeEnable(new List<Control>() {lbBorrowingItems, btnReturnAll, tbSkuReturn, tbQuantityReturn,
                        btnCancelLending, btnPayRentingFee, tbcLending});
                    btnScanRFIDRent.Enabled = false;
                    cbLendingShopList.Enabled = false;
                    tbBorrowerName.Text = currentVisitor.FirstName + " " + currentVisitor.LastName;
                    tbCustomerBalanceLending.Text = string.Format("{0:n2}", currentVisitor.Balance);
                    if (currentVisitor.Borrowings.Count != 0)
                    {
                        lbBorrowingItems.Items.Clear();
                        lbBorrowingItems.Items.Add("Current borrowings:");
                        lbBorrowingItems.Items.Add("--------------------------");
                        foreach (KeyValuePair<EventItem, int> pair in currentVisitor.Borrowings)
                        {
                            lbBorrowingItems.Items.Add(string.Format("Sku {0} - {1} - quantity {2}", pair.Key.Sku, pair.Key.Name, pair.Value));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No visitor found with RFID tag " + e.Tag);
                }
            }
        }
        
        // personalize greeting in application
        private string Greeting()
        {
            if (DateTime.Now.Hour >= 18)
            {
                return "Good evening, " + currentUser.FirstName;
            }
            else if (DateTime.Now.Hour >= 12)
            {
                return "Good afternoon, " + currentUser.FirstName;
            }
            else
            {
                return "Good morning, " + currentUser.FirstName;
            }
        }

        //------------------------------------------------------------//
        // Generate drink item in shop
        private void GenerateDrinkItem()
        {
            GeneratePictureBox(myEvent.GetItemsForSale()[SaleType.Drink], tabDrink);
            GenerateButtonAdd(myEvent.GetItemsForSale()[SaleType.Drink], tabDrink);
            GenerateButtonRemove(myEvent.GetItemsForSale()[SaleType.Drink], tabDrink);
        }

        // generate food item in shop
        private void GenerateFoodItem()
        {
            GeneratePictureBox(myEvent.GetItemsForSale()[SaleType.Food], tabFood);
            GenerateButtonAdd(myEvent.GetItemsForSale()[SaleType.Food], tabFood);
            GenerateButtonRemove(myEvent.GetItemsForSale()[SaleType.Food], tabFood);
        }

        // generate lending item in shop
        private void GenerateLendingItem()
        {
            GeneratePictureBox(myEvent.GetItemsForRent(), tabLending);
            GenerateButtonAdd(myEvent.GetItemsForRent(), tabLending);
            GenerateButtonRemove(myEvent.GetItemsForRent(), tabLending);
        }

        // generate souvenir item in shop
        private void GenerateSouvenirItem()
        {
            GeneratePictureBox(myEvent.GetItemsForSale()[SaleType.Souvenir], tabSouvenir);
            GenerateButtonAdd(myEvent.GetItemsForSale()[SaleType.Souvenir], tabSouvenir);
            GenerateButtonRemove(myEvent.GetItemsForSale()[SaleType.Souvenir], tabSouvenir);
        }

        // generate picture box in form for item image
        public static void GeneratePictureBox(List<EventItem> tempItemList, TabPage destination)
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

        // generate button Add in form
        private void GenerateButtonAdd(List<EventItem> tempItemList, TabPage destination)
        {
            int x = 15;
            int y = 130;
            int width = 50;
            int height = 25;
            foreach (EventItem i in tempItemList)
            {
                Button button = new Button();
                button.Location = new System.Drawing.Point(x, y);
                button.Name = "btnAdd" + i.Sku.ToString();
                button.Size = new System.Drawing.Size(width, height);
                button.Text = "+";
                button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                x += width + 65;
                button.Tag = i;
                if (i.QuantityInStock == 0)
                {
                    button.Enabled = false;
                }
                button.Click += new EventHandler(ButtonAddClick);
                if (i is ItemForSale)
                {
                    button.Click += new EventHandler(UpdateShownSellingOrder);
                }
                else
                {
                    button.Click += new EventHandler(UpdateShownRentingOrder);
                    button.Click += new EventHandler(DisableReturnButton);
                }
                button.Click += new EventHandler(UpdateStockAvailable);
                if (x + width + 65 > destination.Width)
                {
                    x = 15;
                    y += height + 135;
                }
                destination.Controls.Add(button);
            }
        }  

        // add item to order when btnAdd clicked
        private void ButtonAddClick(object sender, EventArgs e)
        {
            if (((Button)sender).Tag is ItemForSale)
            {
                if (tempOrder.Count == 0)
                {
                    newSellOrder = new SellingOrder(LatitudeEvent.GetNextOrderNumber() + 1, currentVisitor, currentUser, tempOrder, myEvent.GetShopById(Convert.ToInt32(cbSellingShopList.Text)));
                }
                newSellOrder.AddItemToOrder((EventItem)((Button)sender).Tag, 1);
            }
            else
            {
                if (tempOrder.Count == 0)
                {
                    newRentingOrder = new RentingOrder(LatitudeEvent.GetNextOrderNumber() + 1, currentVisitor, currentUser, tempOrder, myEvent.GetShopById(Convert.ToInt32(cbLendingShopList.Text)));
                }
                newRentingOrder.AddItemToOrder((EventItem)((Button)sender).Tag, 1);
            }
        }

        // if item is out of stock, it cannot be added to order => disable add button
        private void UpdateStockAvailable(object sender, EventArgs e)
        {
            if (((Button)sender).Tag is EventItem)
            {
                if (((EventItem)((Button)sender).Tag).QuantityInStock <= 0)
                {
                    ((Button)sender).Enabled = false;
                }
                else
                {
                    ((Button)sender).Enabled = true;
                }
            }
        }

        // show in selling listbox & totalPrice textbox
        private void UpdateShownSellingOrder(object sender, EventArgs e)
        {
            if (newSellOrder != null)
            {
                tbTotalPrice.Text = string.Format("{0:0.00}", newSellOrder.GetTotal());
                lbOrderOverview.Items.Clear();
                foreach (KeyValuePair<EventItem, int> d in tempOrder)
                {
                    lbOrderOverview.Items.Add(string.Format("Sku {0} - {1} - quantity: {2}", d.Key.Sku, d.Key.Name, d.Value));
                }
                if (currentVisitor.TicketType == Ticket.VipAtGate || currentVisitor.TicketType == Ticket.VipOnline)
                {
                    lbOrderOverview.Items.Add("-------------------------------");
                    lbOrderOverview.Items.Add("With special offer 20% discount");
                }
            }
        }

        // show in borrowing listbox & totalRentingFee textbox
        private void UpdateShownRentingOrder(object sender, EventArgs e)
        {
            if (newRentingOrder != null)
            {
                tbTotalFee.Text = string.Format("{0:0.00}", newRentingOrder.GetTotal());
                lbBorrowingItems.Items.Clear();
                foreach (KeyValuePair<EventItem, int> d in tempOrder)
                {
                    lbBorrowingItems.Items.Add(string.Format("Sku {0} - {1} - quantity: {2}", d.Key.Sku, d.Key.Name, d.Value));
                }
                if (currentVisitor.TicketType == Ticket.VipAtGate || currentVisitor.TicketType == Ticket.VipOnline)
                {
                    lbBorrowingItems.Items.Add("-------------------------------");
                    lbBorrowingItems.Items.Add("With special offer 20% discount");
                }
            }
        }

        private void DisableReturnButton(object sender, EventArgs e)
        {
            btnReturnAll.Enabled = false;
            btnReturnSelectedItem.Enabled = false;
            btnPayFine.Enabled = false;
            tbSkuReturn.Enabled = false;
            tbQuantityReturn.Enabled = false;
        }

        // generate button Remove in form
        private void GenerateButtonRemove(List<EventItem> tempItemList, TabPage destination)
        {
            int x = 65;
            int y = 130;
            int width = 50;
            int height = 25;
            foreach (EventItem i in tempItemList)
            {
                Button button = new Button();
                button.Location = new System.Drawing.Point(x, y);
                button.Name = "btnRemove" + i.Sku.ToString();
                button.Size = new System.Drawing.Size(width, height);
                button.Text = "-";
                button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                x += width + 65;
                button.Tag = i;
                button.Click += new EventHandler(ButtonRemoveClick);
                if (i is ItemForSale)
                {
                    button.Click += new EventHandler(UpdateShownSellingOrder);
                }
                else
                {
                    button.Click += new EventHandler(UpdateShownRentingOrder);
                }
                if (i.QuantityInStock == 0)
                {
                    button.Enabled = false;
                }
                if (x + width > destination.Width)
                {
                    x = 65;
                    y += height + 135;
                }
                destination.Controls.Add(button);
            }
        }

        // remove item from order when btnRemove clicked
        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            try
            {
                EventItem item = (EventItem)((Button)sender).Tag;
                if (item is ItemForSale)
                {
                    newSellOrder.RemoveItemFromOrder(item, 1);

                }
                else
                {
                    newRentingOrder.RemoveItemFromOrder(item, 1);
                }

                if (tempOrder[item] == 0)
                {
                    tempOrder.Remove(item);
                }
                
                if(item.QuantityInStock > 0)
                {
                    string buttonName = "btnAdd" + item.Sku.ToString();
                    Button b;
                    if(item is ItemForSale)
                    {
                        if (((ItemForSale)item).ItemType == SaleType.Drink)
                        {
                            b = FindButtonWithName(buttonName, tabDrink);
                        }
                        else if(((ItemForSale)item).ItemType == SaleType.Food)
                        {
                            b = FindButtonWithName(buttonName, tabFood);
                        }
                        else
                        {
                            b = FindButtonWithName(buttonName, tabSouvenir);
                        }
                    }
                    else
                    {
                        b = FindButtonWithName(buttonName, tabLending);
                    }
                    b.Enabled = true;
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Item should be first added to create a new order");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Button FindButtonWithName(string name, TabPage page)
        {
            foreach (Control b in page.Controls)
            {
                if (b is Button)
                {
                    if (b.Name == name)
                    {
                        return (Button)b;
                    }
                }
            }
            return null;
        }

        //--------------------------------------------------------------////////////////////////////////////
        // Will have to remove/ edit later everything above this line //////////////////////////////////////

        private void MakeInvisible(List<Control> controls)
        {
            foreach (Control c in controls)
            {
                c.Visible = false;
            }
        }

        private void MakeDisable(List<Control> controls)
        {
            foreach (Control c in controls)
            {
                c.Enabled = false;
            }
        }

        private void MakeEnable(List<Control> controls)
        {
            foreach (Control c in controls)
            {
                c.Enabled = true;
            }
        }

        private Employee GetEmpByUsernamePassword(string username, string password)
        {
            foreach (Employee emp in myEvent.GetEmployees())
            {
                if (emp.AppUsername == username && emp.AppPassword == password)
                {
                    return emp;
                }
            }
            return null;
        }

        private void button1_Click(object sender, EventArgs e) // button1 == btnLogIn, textBox1 == username, textbox2 == password
        {
            // check if username & password match
            Employee temp = GetEmpByUsernamePassword(textBox1.Text, textBox2.Text);
            if (temp != null)
            {
                currentUser = temp;
                pnlMainMenu.Visible = true;
                pnlLogIn.Visible = false;
                lbGreeting.Text = Greeting();
                MakeInvisible(new List<Control> { pnlEventManagement, pnlMyAccount, tbcEntranceCheck, tbcServicePoint, tbcStockManagement, tbcEmployeeManagement });

                // set controls' text on MyAccount panel
                tbFirstNameEmp.Text = currentUser.FirstName;
                tbLastNameEmp.Text = currentUser.LastName;
                tbDobEmp.Text = currentUser.Dob.ToShortDateString();
                tbEmpNumber.Text = currentUser.Id.ToString();
                tbAddressEmp.Text = currentUser.Address;
                tbEmailEmp.Text = currentUser.Email;
                tbUsernameEmp.Text = currentUser.AppUsername;
                tbPasswordEmp.Text = currentUser.AppPassword;

                // set visibility & enablity of controls according to current user's level
                if (currentUser.Level == EmpLevel.Level1)
                {
                    pnlEventManagement.Visible = true;
                    MakeEnable(new List<Control> { btnEntranceCheckpoints, btnServicePoints, btnStockManagement, btnEmpManagement, btnEventManagement });
                    tabPage8.Enabled = true;
                    tabPage9.Enabled = true;
                    foreach (ServicePoint p in myEvent.ServicePoints)
                    {
                        if (p is SellingShop)
                        {
                            cbSellingShopList.Items.Add(p.PointId);
                        }
                        else if (p is LendingShop)
                        {
                            cbLendingShopList.Items.Add(p.PointId);
                        }
                    }
                }
                else if (currentUser.Level == EmpLevel.Level2)
                {
                    tbcStockManagement.Visible = true;
                    MakeDisable(new List<Control> { btnEventManagement, btnEmpManagement, btnEntranceCheckpoints });
                    MakeEnable(new List<Control> { btnStockManagement, btnServicePoints });

                    // setup Selling & Lending tabPage for shop manager
                    foreach (ServicePoint p in myEvent.SellingShops)
                    {
                        if (p.Manager == currentUser)
                        {
                            cbSellingShopList.Items.Add(p.PointId);
                            tabPage8.Enabled = true;
                            tbcServicePoint.SelectedTab = tabPage8;
                        }
                    }
                    foreach (ServicePoint p in myEvent.LendingShops)
                    {
                        if (p.Manager == currentUser)
                        {
                            cbLendingShopList.Items.Add(p.PointId);
                            tabPage9.Enabled = true;
                            tbcServicePoint.SelectedTab = tabPage9;
                        }
                    }
                }
                else
                {
                    if (currentUser.Place == WorkPlaceForLevel3.EntranceCheck)
                    {
                        tbcEntranceCheck.Visible = true;
                        MakeDisable(new List<Control> { btnServicePoints, btnStockManagement, btnEmpManagement, btnEventManagement });
                        btnEntranceCheckpoints.Enabled = true;
                    }
                    else if (currentUser.Place == WorkPlaceForLevel3.Shop)
                    {
                        tbcServicePoint.Visible = true;
                        MakeDisable(new List<Control> { btnEntranceCheckpoints, btnStockManagement, btnEventManagement, btnEmpManagement });
                        btnServicePoints.Enabled = true;

                        // generate picture & button of items for shops
                        GenerateDrinkItem();
                        GenerateFoodItem();
                        GenerateLendingItem();
                        GenerateSouvenirItem();

                        // setup lending & selling tabPage
                        ServicePoint shop = currentUser.GetShop();
                        if (shop is SellingShop)
                        {
                            cbSellingShopList.Items.Add(shop.PointId);
                            tabPage8.Enabled = true; // tabPage8 == selling shop
                            tbcServicePoint.SelectedTab = tabPage8;

                        }
                        else if (shop is LendingShop)
                        {
                            cbLendingShopList.Items.Add(shop.PointId);
                            tabPage9.Enabled = true; // tabPage9 == lending shop
                            tbcServicePoint.SelectedTab = tabPage9;
                        }
                    }
                    else
                    {
                        MakeDisable(new List<Control> { btnEntranceCheckpoints, btnServicePoints, btnStockManagement, btnEmpManagement, btnEventManagement });
                        pnlMyAccount.Visible = true;
                    }
                }
                // set cashier name
                if (tabPage8.Enabled)
                {
                    tbSellingCashierName.Text = currentUser.FirstName + " " + currentUser.LastName;
                }
                if (tabPage9.Enabled)
                {
                    tbLendingCashierName.Text = currentUser.FirstName + " " + currentUser.LastName;
                }

                // set combobox text
                if (cbLendingShopList.Items.Count != 0)
                {
                    cbLendingShopList.Text = cbLendingShopList.Items[0].ToString();
                }
                if (cbSellingShopList.Items.Count != 0)
                {
                    cbSellingShopList.Text = cbSellingShopList.Items[0].ToString();
                }
            }
            else
            {
                MessageBox.Show("Log in failed. Please check your username & password again!");
            }
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            pnlMainMenu.Visible = false;
            pnlLogIn.Visible = true;

            // clear tempOrder
            tempOrder = new Dictionary<EventItem, int>();

            //clear textbox 
            textBox1.Text = "";
            textBox2.Text = "";
            tbSellingCashierName.Text = "";
            tbLendingCashierName.Text = "";
            tbTotalPrice.Text = "";
            tbTotalFee.Text = "";

            //clear combobox shop list
            cbLendingShopList.Items.Clear();
            cbSellingShopList.Items.Clear();
            cbCampingSpots.Items.Clear();

            // disable tabPage
            tabPage8.Enabled = false;
            tabPage9.Enabled = false;
            currentUser = null;

            // clear listbox
            lbEmployees.Items.Clear();
            lbOrderOverview.Items.Clear();
            lbBorrowingItems.Items.Clear();
        }

        private void btnEntranceCheckpoints_Click(object sender, EventArgs e)
        {
            MakeInvisible(new List<Control>() { tbcServicePoint, tbcStockManagement, tbcEmployeeManagement, pnlEventManagement });
            tbcEntranceCheck.Visible = true;
        }

        private void btnServicePoints_Click(object sender, EventArgs e)
        {
            MakeInvisible(new List<Control> { tbcEntranceCheck, tbcStockManagement, tbcEmployeeManagement, pnlMyAccount, pnlEventManagement });
            tbcServicePoint.Visible = true;
            RefreshSellingForm();
            RefreshLendingForm();
        }

        private void rbtnWithRFID_CheckedChanged(object sender, EventArgs e)
        {
            lbVisitorDetail.Items.Clear();
            if (rbtnWithRFID.Checked)
            {
                btnRFIDCheckOut.Enabled = true;
                tbTicketNr.Enabled = false;
                btnCheckIn.Enabled = false;
                btnCheckOut.Enabled = false;
            }
            else
            {
                btnRFIDCheckOut.Enabled = false;
                tbTicketNr.Enabled = true;
                btnCheckIn.Enabled = true;
                btnCheckOut.Enabled = true;
            }
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            pnlNewOrder.Visible = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            pnlNewOrder.Visible = false;
        }

        private void btnStockManagement_Click(object sender, EventArgs e)
        {
            MakeInvisible(new List<Control> { tbcServicePoint, tbcEntranceCheck, tbcEmployeeManagement, pnlMyAccount, pnlEventManagement });
            tbcStockManagement.Visible = true;
        }

        private void rbtnUpdateExistingItem_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnUpdateExistingItem.Checked)
            {
                pnlUpdateExistingItem.Visible = true;
                pnlUpdateNewItem.Visible = false;
            }
            else
            {
                pnlUpdateExistingItem.Visible = false;
                pnlUpdateNewItem.Visible = true;
            }
            lbUpdateOverview.Items.Clear();
        }

        private void rbtnForRent_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnForRent.Checked)
            {
                pnlNewForRent.Visible = true;
                pnlNewForSale.Visible = false;
            }
            else
            {
                pnlNewForSale.Visible = true;
                pnlNewForRent.Visible = false;
            }
        }

        private void btnEmpManagement_Click(object sender, EventArgs e)
        {
            MakeInvisible(new List<Control> { tbcServicePoint, tbcEntranceCheck, tbcStockManagement, pnlMyAccount, pnlEventManagement });
            tbcEmployeeManagement.Visible = true;
        }

        private void btnEmpChangeAddress_Click(object sender, EventArgs e)
        {
            tbAddressEmp.Enabled = true;
        }

        private void btnEmpChangeEmail_Click(object sender, EventArgs e)
        {
            tbEmailEmp.Enabled = true;
        }

        private void btnEmpChangeUsername_Click(object sender, EventArgs e)
        {
            tbUsernameEmp.Enabled = true;
        }

        private void btnEmpChangePassword_Click(object sender, EventArgs e)
        {
            tbPasswordEmp.Enabled = true;
        }

        private void btnMyAccount_Click(object sender, EventArgs e)
        {
            MakeInvisible(new List<Control> { tbcEntranceCheck, tbcServicePoint, tbcStockManagement, tbcEmployeeManagement, pnlEventManagement });
            pnlMyAccount.Visible = true;
        }

        private void btnEventManagement_Click(object sender, EventArgs e)
        {
            MakeInvisible(new List<Control>() { tbcEntranceCheck, tbcServicePoint, tbcStockManagement, tbcEmployeeManagement, pnlMyAccount });
            pnlEventManagement.Visible = true;
        }

        private void cbEmpLevel_TextChanged(object sender, EventArgs e)
        {
            if (cbEmpLevel.Text == "level 3")
            {
                cbWorkPlace.Items.Clear();
                cbWorkPlace.Items.AddRange(new string[] { "entrance check", "shop", "other" });
                cbWorkPlace.Enabled = true;
            }
            else
            {
                cbWorkPlace.Items.Add("non applicable");
                cbWorkPlace.Text = "non applicable";
                cbWorkPlace.Enabled = false;
            }
        }

        private void cbWorkPlace_TextChanged(object sender, EventArgs e)
        {
            if (cbWorkPlace.Text == "shop")
            {
                lbelShopId.Visible = true;
                cbShopList.Visible = true;
                cbWorkPlace.Size = new System.Drawing.Size(65, 28);
                cbShopList.Items.Clear();
                foreach (ServicePoint point in myEvent.ServicePoints)
                {
                    if (point is SellingShop || point is LendingShop)
                    {
                        cbShopList.Items.Add(point.PointId);
                    }
                }
                cbShopList.Text = cbShopList.Items[0].ToString();
            }
            else
            {
                lbelShopId.Visible = false;
                cbShopList.Visible = false;
                cbShopList.Items.Clear();
                cbWorkPlace.Size = new System.Drawing.Size(140, 28);
            }
        }

        private void rbtnNewEmp_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnNewEmp.Checked)
            {
                pnlNewEmp.Visible = true;
                pnlExistingEmp.Visible = false;
            }
            else
            {
                pnlNewEmp.Visible = false;
                pnlExistingEmp.Visible = true;
            }
            RefreshExistingEmpForm();
            RefreshNewEmpForm();
        }

        private void RefreshNewEmpForm()
        {
            tbNewFirstName.Text = "";
            tbNewLastName.Text = "";
            dateTimeDOB.Text = dateTimeDOB.MaxDate.ToShortDateString();
            cbEmpLevel.Items.Clear();
            cbEmpLevel.Items.AddRange(new string[] { "level 1", "level 2", "level 3" });
            cbWorkPlace.Items.Clear();
            lbelShopId.Visible = false;
            cbShopList.Visible = false;
            cbWorkPlace.Enabled = false;
            tbNewAddress.Text = "";
            tbNewEmail.Text = "";
            tbUserName.Text = "";
            tbPassword.Text = "";
        }
        private void btnChangeEmp_Click(object sender, EventArgs e)
        {
            pnlChangeEmpLevel.Visible = true;
        }

        private void btnCancelChangeEmp_Click(object sender, EventArgs e)
        {
            RefreshExistingEmpForm();
        }

        private void btnSaveChangeEmp_Click(object sender, EventArgs e)
        {
            Employee emp = (Employee)myEvent.GetPersonById(Convert.ToInt32(tbExistingEmpNr.Text));
            if(cbNewEmpLevel.Text == "level 1")
            {
                emp.ChangeEmpLevel(EmpLevel.Level1);
                emp.ChangeWorkingPlaceForEmpLevel3(WorkPlaceForLevel3.NonApplicable);
                emp.ChangeShop(null);
            }
            else if (cbNewEmpLevel.Text == "level 2")
            {
                emp.ChangeEmpLevel(EmpLevel.Level2);
                emp.ChangeWorkingPlaceForEmpLevel3(WorkPlaceForLevel3.NonApplicable);
                emp.ChangeShop(null);
            }
            else
            {
                emp.ChangeEmpLevel(EmpLevel.Level3);
                if(cbNewEmpWorkPlace.Text == "entrance check")
                {
                    emp.ChangeWorkingPlaceForEmpLevel3(WorkPlaceForLevel3.EntranceCheck);
                    emp.ChangeShop(null);
                }
                else if(cbNewEmpWorkPlace.Text == "shop")
                {
                    emp.ChangeWorkingPlaceForEmpLevel3(WorkPlaceForLevel3.Shop);
                    emp.ChangeShop(myEvent.GetShopById(Convert.ToInt32(cbNewEmpShop.Text)));
                }
                else
                {
                    emp.ChangeWorkingPlaceForEmpLevel3(WorkPlaceForLevel3.Other);
                    emp.ChangeShop(null);
                }
            }
            LatitudeEvent.UpdateEmployeeInfo(emp);
            lbEmployees.Items.Clear();
            lbEmployees.Items.Add("******* Updated employee info *******");
            lbEmployees.Items.Add(emp.AsAString());
            RefreshExistingEmpForm();
        }

        private void RefreshExistingEmpForm()
        {
            pnlChangeEmpLevel.Enabled = false;
            cbNewEmpLevel.Items.Clear();
            cbNewEmpWorkPlace.Items.Clear();
            cbNewEmpShop.Items.Clear();
            tbExistingEmpNr.Text = "";
            tbOldFirstName.Text = "";
            tbOldLastName.Text = "";
            tbOldLevel.Text = "";
            tbOldShopId.Text = "";
            tbOldWorkPlace.Text = "";
            btnSearchEmp.Enabled = true;
        }

        private void cbNewEmpLevel_TextChanged(object sender, EventArgs e)
        {
            if (cbNewEmpLevel.Text == "level 3")
            {
                cbNewEmpWorkPlace.Items.Remove("non applicable");
                cbNewEmpWorkPlace.Enabled = true;
            }
            else
            {
                if (!cbNewEmpWorkPlace.Items.Contains("non applicable"))
                {
                    cbNewEmpWorkPlace.Items.Add("non applicable");
                }
                cbNewEmpWorkPlace.Text = "non applicable";
                cbNewEmpWorkPlace.Enabled = false;
                cbNewEmpShop.Enabled = false;
            }
        }

        private void cbNewEmpWorkPlace_TextChanged(object sender, EventArgs e)
        {
            if (cbNewEmpWorkPlace.Text == "shop")
            {
                cbNewEmpShop.Enabled = true;
                // set shop list for employee
                foreach (ServicePoint point in myEvent.ServicePoints)
                {
                    if (point is SellingShop || point is LendingShop)
                    {
                        cbNewEmpShop.Items.Add(point.PointId);
                    }
                }
            }
            else
            {
                cbNewEmpShop.Enabled = false;
                cbNewEmpShop.Items.Clear();
            }
        }

        private void btnEmpCancelChanges_Click(object sender, EventArgs e)
        {
            tbAddressEmp.Text = currentUser.Address;
            tbEmailEmp.Text = currentUser.Email;
            tbUsernameEmp.Text = currentUser.AppUsername;
            tbPasswordEmp.Text = currentUser.AppPassword;
            MakeDisable(new List<Control> { tbAddressEmp, tbEmailEmp, tbUsernameEmp, tbPasswordEmp });
        }

        private void btnEmpSaveChanges_Click(object sender, EventArgs e)
        {
            currentUser.ChangeAddress(tbAddressEmp.Text);
            currentUser.ChangeEmail(tbEmailEmp.Text);
            currentUser.ChangeUsername(tbUsernameEmp.Text);
            currentUser.ChangePassword(tbPasswordEmp.Text);
            LatitudeEvent.UpdateEmployeeInfo(currentUser);
            MakeDisable(new List<Control> { tbAddressEmp, tbEmailEmp, tbUsernameEmp, tbPasswordEmp });
        }

        private void btnShowAllEmp_Click(object sender, EventArgs e)
        {
            lbEmployees.Items.Clear();
            foreach (Employee emp in myEvent.GetEmployees())
            {
                lbEmployees.Items.Add(emp.AsAString());
            }
        }

        private void btnShowLevel1_Click(object sender, EventArgs e)
        {
            lbEmployees.Items.Clear();
            foreach (Employee emp in myEvent.GetEmployees())
            {
                if (emp.Level == EmpLevel.Level1)
                {
                    lbEmployees.Items.Add(emp.AsAString());
                }
            }
        }

        private void btnShowLevel2_Click(object sender, EventArgs e)
        {
            lbEmployees.Items.Clear();
            foreach (Employee emp in myEvent.GetEmployees())
            {
                if (emp.Level == EmpLevel.Level2)
                {
                    lbEmployees.Items.Add(emp.AsAString());
                }
            }
        }

        private void btnShowLevel3_Click(object sender, EventArgs e)
        {
            lbEmployees.Items.Clear();
            foreach (Employee emp in myEvent.GetEmployees())
            {
                if (emp.Level == EmpLevel.Level3)
                {
                    lbEmployees.Items.Add(emp.AsAString());
                }
            }
        }

        // execute button Log In when press enter at textbox Password
        private void textBox2_KeyDown(object sender, KeyEventArgs e) // textBox2 == textbox Password
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, new EventArgs());
            }
        }

        private void rbtnReserveNewCamp_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSpotCode.Checked)
            {
                pnlSpotCode.Visible = true;
                pnlReserveNewCamp.Visible = false;
                if (checkReserveCamp.Checked)
                {
                    campFee = EventPriceList.CampFeePerPerson;
                }
            }
            else
            {
                pnlSpotCode.Visible = false;
                pnlReserveNewCamp.Visible = true;
                if (checkReserveCamp.Checked)
                {
                    if (rbtnGroupTicket.Checked)
                    {
                        campFee = EventPriceList.CampFee + EventPriceList.CampFeePerGroup;
                    }
                    else
                    {
                        campFee = EventPriceList.CampFee + EventPriceList.CampFeePerPerson;
                    }
                }
            }
            amountToPay = balance + ticketPrice + campFee;
            tbTotalToPay.Text = string.Format("{0:n2}", amountToPay);
        }

        private void rbtnGroupTicket_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnGroupTicket.Checked)
            {
                rbtnSpotCode.Enabled = false;
                rbtnReserveNewCamp.Checked = true;
            }
            else
            {
                rbtnSpotCode.Enabled = true;
            }
        }

        private void btnShowAllVisitors_Click(object sender, EventArgs e)
        {
            lbVisitors.Items.Clear();
            foreach (Visitor v in myEvent.GetVisitors())
            {
                foreach (string s in Regex.Split(v.AsAString(), "\n"))
                {
                    lbVisitors.Items.Add(s);
                }
            }
            tbNrOfVisitors.Text = myEvent.GetCurrentNrOfVisitors().ToString();
        }

        private void btnRFIDCheckOut_Click(object sender, EventArgs e)
        {
            try
            {
                rfid_CheckOut.Open();
            }
            catch (PhidgetException)
            {
                MessageBox.Show("Cannot recognize RFID");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            try
            {
                currentVisitor = myEvent.GetVisitorByTicketNr(Convert.ToInt32(tbTicketNr.Text));

                if (currentVisitor != null)
                {
                    if (currentVisitor.CheckIn())
                    {
                        rfid_CheckIn.Open();
                        tbTicketNr.Text = "";
                        lbVisitorDetail.Items.Clear();
                        lbVisitorDetail.Items.Add("Visitor with ticket nr. " + currentVisitor.TicketNr + " checked in");
                    }
                    else
                    {
                        MessageBox.Show("Visitor with ticket nr. " + currentVisitor.TicketNr + " already checked in");
                    }
                }
                else
                {
                    MessageBox.Show("Ticket nr. " + tbTicketNr.Text + " is not valid");
                }

            }
            catch (FormatException)
            {
                MessageBox.Show("Ticket nr. should be a number");
            }
            catch (PhidgetException)
            {
                MessageBox.Show("Cannot recognize RFID");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnShowCheckInVisitors_Click(object sender, EventArgs e)
        {
            lbVisitors.Items.Clear();
            lbVisitors.Items.Add("      ***** Checked-in visitors *****");
            for (int i = myEvent.GetVisitors().Count - 1; i >= 0; i--)
            {
                Visitor v = myEvent.GetVisitors()[i];
                if (v.IsCheckedIn)
                {
                    string temp = "";
                    temp = v.AsAString() + "\n\t+RFID: " + v.Rfid;
                    foreach (string s in Regex.Split(temp, "\n"))
                    {
                        lbVisitors.Items.Add(s);
                    }
                }
            }
            tbNrOfVisitors.Text = myEvent.GetCurrentNrOfCheckInVisitors().ToString();
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            try
            {
                Visitor v = myEvent.GetVisitorByTicketNr(Convert.ToInt32(tbTicketNr.Text));
                if (v != null)
                {
                    v.CheckOut();
                    LatitudeEvent.UpdateCheckOutVisitor(v);
                    lbVisitorDetail.Items.Clear();
                    lbVisitorDetail.Items.Add("Visitor with ticket nr. " + v.TicketNr + " checked out");
                    btnShowCheckInVisitors_Click(this, new EventArgs());
                }
                else
                {
                    MessageBox.Show("Ticket nr. " + tbTicketNr.Text + " is not valid");
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Ticket nr. should be a number");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateNewVisitor_Click(object sender, EventArgs e)
        {
            if (tbVisitorFirstName.Text == "" || tbVisitorLastName.Text == "")
            {
                MessageBox.Show("Please input visitor's name");
            }
            else
            {
                try
                {
                    // create new visitor
                    Visitor v;
                    Gender g;
                    Ticket ticket;

                    if (rbtnNewMale.Checked) { g = Gender.Male; }
                    else { g = Gender.Female; }

                    if (rbtnRegularTicket.Checked) { ticket = Ticket.RegularAtGate; }
                    else if (rbtnVipTicket.Checked) { ticket = Ticket.VipAtGate; }
                    else { ticket = Ticket.GroupAtGate; }

                    if (tbBalance.Text == "")
                    {
                        v = new Visitor(LatitudeEvent.GetNextVisitorNr() + 1, tbVisitorFirstName.Text, tbVisitorLastName.Text, datePickerVisitor.Value, g, ticket, 0);
                    }
                    else
                    {
                        v = new Visitor(LatitudeEvent.GetNextVisitorNr() + 1, tbVisitorFirstName.Text, tbVisitorLastName.Text, datePickerVisitor.Value, g, ticket, Convert.ToDouble(tbBalance.Text));
                    }


                    // add visitor to camping spot
                    if (checkReserveCamp.Checked)
                    {
                        if (rbtnSpotCode.Checked)
                        {
                            if (tbSpotCode.Text == "")
                            {
                                MessageBox.Show("Please insert a spot code!");
                                return;
                            }
                            else
                            {
                                CampingSpot spot;
                                spot = myEvent.GetSpotBySpotCode(tbSpotCode.Text);
                                if (spot != null)
                                {
                                    if (!v.ReserveCampingSpot(spot))
                                    {
                                        MessageBox.Show("Reservation failed! Camping spot with spot code " + tbSpotCode.Text + " is full. Please check again!");
                                        return;
                                    }
                                    else
                                    {
                                        // update database
                                        LatitudeEvent.InsertNewVisitor(v);
                                        LatitudeEvent.UpdateCampingSpotReservation(v);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Reservation failed! Camping spot with spot code " + tbSpotCode.Text + " does not exist. Please check again!");
                                    return;
                                }
                            }
                        }

                        else
                        {
                            if (cbCampingSpots.Text != "")
                            {
                                v.ReserveCampingSpot(myEvent.GetSpotById(cbCampingSpots.Text));
                                // update database
                                LatitudeEvent.InsertNewVisitor(v);
                                LatitudeEvent.UpdateCampingSpotReservation(v);
                            }
                            else
                            {
                                MessageBox.Show("Please select a camping spot!");
                                return;
                            }
                        }
                    }

                    myEvent.AddPersonToEvent(v);

                    // write new visitor's detail to listbox
                    tbNrOfVisitors.Text = "";
                    lbVisitors.Items.Clear();
                    lbVisitors.Items.Add("*************** New visitor ***************");
                    foreach (string s in Regex.Split(v.AsAString(), "\n"))
                    {
                        lbVisitors.Items.Add(s);
                    }

                    // print ticket
                    v.PrintTicket();

                    // clear form
                    tbVisitorFirstName.Text = "";
                    tbVisitorLastName.Text = "";
                    tbBalance.Text = "";
                    tbSpotCode.Text = "";
                    cbCampingSpots.Items.Clear();
                    rbtnRegularTicket.Checked = true;
                    rbtnSpotCode.Checked = true;
                    checkReserveCamp.Checked = false;
                    pnlReserveCamping.Enabled = false;
                    balance = 0;
                    ticketPrice = EventPriceList.RegularAtGateTicket;
                    campFee = 0;
                    amountToPay = ticketPrice + campFee + balance;
                    tbTotalToPay.Text = string.Format("{0:n2}", amountToPay);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void checkReserveCamp_CheckedChanged(object sender, EventArgs e)
        {
            if (checkReserveCamp.Checked)
            {
                pnlReserveCamping.Enabled = true;
                cbCampingSpots.Items.Clear();
                foreach (CampingSpot spot in myEvent.Spots)
                {
                    if (spot.Renters.Count() == 0)
                    {
                        cbCampingSpots.Items.Add(spot.SpotId);
                    }
                }
                if (rbtnSpotCode.Checked)
                {
                    campFee = EventPriceList.CampFeePerPerson;
                }
                else 
                {
                    if (rbtnGroupTicket.Checked)
                    {
                        campFee = EventPriceList.CampFee + EventPriceList.CampFeePerGroup;
                    }
                    else
                    {
                        campFee = EventPriceList.CampFee + EventPriceList.CampFeePerPerson;
                    }
                }
            }
            else
            {
                pnlReserveCamping.Enabled = false;
                campFee = 0;
            }
            amountToPay = campFee + balance + ticketPrice;
            tbTotalToPay.Text = string.Format("{0:n2}", amountToPay);
        }

        private void tbTicketNrLost_TextChanged(object sender, EventArgs e)
        {
            if (tbTicketNrLost.Text == "")
            {
                lbLostRFIDInfo.Items.Clear();
            }
        }

        private void tbcCheckAtGate_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentVisitor = null;
        }

        private void btnAssignNewRFID_Click(object sender, EventArgs e)
        {
            try
            {
                currentVisitor = myEvent.GetVisitorByTicketNr(Convert.ToInt32(tbTicketNrLost.Text));
                if (currentVisitor != null)
                {
                    rfid_Lost.Open();
                }
                else
                {
                    lbLostRFIDInfo.Items.Add("No visitor found with ticket nr. " + tbTicketNrLost.Text);
                }

            }
            catch (FormatException)
            {
                MessageBox.Show("Ticket nr. should be a number");
            }
            catch (PhidgetException)
            {
                MessageBox.Show("Cannot recognize RFID");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnShowAllSpots_Click(object sender, EventArgs e)
        {
            lbSpotsInfo.Items.Clear();
            foreach (CampingSpot spot in myEvent.Spots)
            {
                lbSpotsInfo.Items.Add(spot.AsAString());
            }
        }

        private void btnScanRFIDSpot_Click(object sender, EventArgs e)
        {
            if (btnScanRFIDSpot.Text == "Scan RFID")
            {
                rfid_Camping.Open();
                btnScanRFIDSpot.Text = "Stop";
                btnAssignVisitorToSpot.Enabled = true;
                btnShowSpotInfo.Enabled = true;
            }
            else
            {
                rfid_Camping.Close();
                btnScanRFIDSpot.Text = "Scan RFID";
                tbStatusSpotCheck.Text = "";
                cbSpotId.Items.Clear();
                lbSpotsInfo.Items.Clear();
                btnAssignVisitorToSpot.Enabled = false;
                btnShowSpotInfo.Enabled = false;
                tbCampingFee.Text = "";
            }

        }

        private void tbcEntranceCheck_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentVisitor = null;
            if (btnScanRFIDSpot.Text == "Stop")
            {
                btnScanRFIDSpot_Click(this, new EventArgs());
            }
        }

        private void tbStatusSpotCheck_TextChanged(object sender, EventArgs e)
        {
            if (tbStatusSpotCheck.Text == "yes")
            {
                btnAssignVisitorToSpot.Enabled = false;
            }
            else
            {
                btnAssignVisitorToSpot.Enabled = true;
            }
        }

        private void btnAssignVisitorToSpot_Click(object sender, EventArgs e)
        {
            if (cbSpotId.Text == "")
            {
                MessageBox.Show("Please select a spot id");
            }
            else
            {
                if (currentVisitor.ReserveCampingSpot(myEvent.GetSpotById(cbSpotId.Text)))  
                {
                    tbStatusSpotCheck.Text = "yes";
                    cbSpotId.Enabled = false;
                    LatitudeEvent.UpdateCampingSpotReservation(currentVisitor);
                    lbSpotsInfo.Items.Clear();
                    foreach (string s in Regex.Split(currentVisitor.AsAString(), "\n"))
                    {
                        lbSpotsInfo.Items.Add(s);
                    }
                }
                else
                {
                    MessageBox.Show("Cannot assign this visitor to camp spot " + cbSpotId.Text);
                }
            }
        }

        private void btnShowSpotInfo_Click(object sender, EventArgs e)
        {
            lbSpotsInfo.Items.Clear();
            if (cbSpotId.Text != "")
            {
                foreach (string s in Regex.Split(myEvent.GetSpotById(cbSpotId.Text).ShowDetailRenters(), "\n"))
                {
                    lbSpotsInfo.Items.Add(s);
                }
            }
            else
            {
                lbSpotsInfo.Items.Add("No spot id selected");
            }
        }

        private void btnScanRFIDSell_Click(object sender, EventArgs e)
        {
            rfid_Buying.Open();
        }

        private void RefreshSellingForm()
        {
            btnScanRFIDSell.Enabled = true;
            cbSellingShopList.Enabled = true;
            pnlNewOrder.Enabled = false;
            tbCustomerInfo.Text = "";
            tbCustomerBalance.Text = "";
            lbOrderOverview.Items.Clear();
            tbTotalPrice.Text = "";
            tbSearchSellItem.Text = "";
            currentVisitor = null;
            tempOrder.Clear();
            rfid_Buying.Close();
            newSellOrder = null;
            UnloadTabpage(tabDrink);
            UnloadTabpage(tabFood);
            UnloadTabpage(tabSouvenir);
            GenerateDrinkItem();
            GenerateFoodItem();
            GenerateSouvenirItem();
            tbcFoodDrink.SelectedIndex = 0;
        }

        private void btnCancelOrder_Click(object sender, EventArgs e)
        {
            if (newSellOrder != null)
            {
                DialogResult cancelOrder = MessageBox.Show("Are you sure you want to cancel this order?", "Cancelling...", MessageBoxButtons.YesNo);
                if (cancelOrder == DialogResult.Yes)
                {
                    try
                    {
                        newSellOrder.CancelOrder();
                        RefreshSellingForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No order created");
            }
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            try
            {
                if (newSellOrder != null)
                {
                    newSellOrder.PayOrder();
                    if (newSellOrder.IsPaid)
                    {
                        DialogResult payOrder = MessageBox.Show("Do you want to print this receipt?", "Printing...", MessageBoxButtons.YesNo);
                        if (payOrder == DialogResult.Yes)
                        {
                            newSellOrder.PrintOrder();
                        }
                       
                        // update database
                        LatitudeEvent.InsertNewOrder(newSellOrder);

                        // refresh form
                        RefreshSellingForm();
                    }
                }
                else
                {
                    MessageBox.Show("Order is empty");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOverallReport_Click(object sender, EventArgs e)
        {
            btnSaveReport.Enabled = true;

            double revenueFromSellingShops = LatitudeEvent.CalculateRevenueFromSellingShops();
            double revenueFromLendingShops = LatitudeEvent.CalculateRevenueFromLendingShops();
            double revenueFromVendingMachines = LatitudeEvent.CalculateRevenueFromVendingMachines();
            double revenueFromSellingLending = revenueFromLendingShops + revenueFromSellingShops + revenueFromVendingMachines;
            double revenueFromTickets = myEvent.CalculateTotalRevenueFromTickets();
            double revenueFromCamping = myEvent.CalculateRevenueFromCamping();
            double revenueFromFine = LatitudeEvent.CalculateRevenueFromFine();

            lbEventInfo.Items.Clear();
            lbEventInfo.Items.Add("\tLATITUDE MUSIC EVENT REPORT");
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add("* Report time: " + DateTime.Now.ToString());
            lbEventInfo.Items.Add("* Event time: " + myEvent.EventTime.ToShortDateString() + " - " + myEvent.EventTime.AddDays(2).ToShortDateString());
            lbEventInfo.Items.Add("* Maximum number of visitors: " + myEvent.MaxNrVisitors);
            lbEventInfo.Items.Add("* Total visitors: " + myEvent.GetCurrentNrOfVisitors());
            lbEventInfo.Items.Add("* Total checked-in visitors: " + myEvent.GetCurrentNrOfCheckInVisitors());
            lbEventInfo.Items.Add(string.Format("* Total revenue: €{0:n}", revenueFromCamping + revenueFromTickets + revenueFromSellingLending + revenueFromFine));
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add("\t\tRevenue breakdown:");
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add(string.Format("\t1. From selling tickets: €{0:n}", revenueFromTickets));
            lbEventInfo.Items.Add(string.Format("\t\t- regular tickets: €{0:n}", myEvent.CalculateRevenueFromTickets()[Ticket.RegularAtGate] + myEvent.CalculateRevenueFromTickets()[Ticket.RegularOnline]));
            lbEventInfo.Items.Add(string.Format("\t\t- VIP tickets: €{0:n}", myEvent.CalculateRevenueFromTickets()[Ticket.VipAtGate] + myEvent.CalculateRevenueFromTickets()[Ticket.VipOnline]));
            lbEventInfo.Items.Add(string.Format("\t\t- group tickets: €{0:n}", myEvent.CalculateRevenueFromTickets()[Ticket.GroupAtGate] + myEvent.CalculateRevenueFromTickets()[Ticket.GroupOnline]));
            lbEventInfo.Items.Add("");
            lbEventInfo.Items.Add(string.Format("\t2. From selling & lending items: €{0:n}", revenueFromSellingLending));
            lbEventInfo.Items.Add(string.Format("\t\t- selling shops: €{0:n}", revenueFromSellingShops));
            foreach (ServicePoint shop in myEvent.SellingShops)
            {
                lbEventInfo.Items.Add(string.Format("\t\t\t+ selling shop id {0}: €{1:n}", shop.PointId, LatitudeEvent.CalculateRevenueFromAServicePoint(shop)));
            }
            lbEventInfo.Items.Add(string.Format("\t\t- lending shops: €{0:n}", revenueFromLendingShops));
            foreach (ServicePoint shop in myEvent.LendingShops)
            {
                lbEventInfo.Items.Add(string.Format("\t\t\t+ lending shop id {0}: €{1:n}", shop.PointId, LatitudeEvent.CalculateRevenueFromAServicePoint(shop)));
            }
            lbEventInfo.Items.Add(string.Format("\t\t- vending machines: €{0:n}", revenueFromVendingMachines));
            foreach (ServicePoint shop in myEvent.VendingMachines)
            {
                lbEventInfo.Items.Add(string.Format("\t\t\t+ vending machine id {0}: €{1:n}", shop.PointId, LatitudeEvent.CalculateRevenueFromAServicePoint(shop)));
            }
            lbEventInfo.Items.Add("");
            lbEventInfo.Items.Add(string.Format("\t3. From camping spots: €{0:n}", revenueFromCamping));
            foreach (CampingSpot spot in myEvent.Spots)
            {
                lbEventInfo.Items.Add(string.Format("\t\t- camping spot id {0}: €{1:n}", spot.SpotId, spot.CalculateRevenueFromCampingSpot()));
            }
            lbEventInfo.Items.Add("");
            lbEventInfo.Items.Add(string.Format("\t4. From fine paid by visitors: €{0:n}", revenueFromFine));
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add("\t\tVisitors' balance: ");
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add("Ticket\tType\tTop up\t Exp.*\t Fine**\tBalance");
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            foreach (Visitor v in myEvent.GetVisitors())
            {
                string type;
                if(v.TicketType == Ticket.GroupAtGate || v.TicketType == Ticket.GroupOnline)
                {
                    type = "group";
                }
                else if (v.TicketType == Ticket.RegularAtGate || v.TicketType == Ticket.RegularOnline)
                {
                    type = "reg.";
                }
                else
                {
                    type = "vip";
                }
                lbEventInfo.Items.Add(string.Format("{0}\t{1}\t{2,7:n}\t{3,7:n}\t{4,7:n}\t{5,7:n} ", 
                    v.TicketNr, type, LatitudeEvent.CalculateTotalTopUp(v), LatitudeEvent.CalculateTotalBuyingRenting(v), LatitudeEvent.CalculateTotalFine(v), v.Balance));
            }
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add(string.Format("TOTAL:\t\t{0,7:n}\t{1,7:n}\t{2,7:n}\t{3,7:n}", LatitudeEvent.CalculateTotalTopUp(), revenueFromSellingLending, revenueFromFine, LatitudeEvent.CalculateTotalBalance()));
            lbEventInfo.Items.Add("----------------------------------------------------------------------------");
            lbEventInfo.Items.Add("* Exp.: Total expenditure on buying & renting event items");
            lbEventInfo.Items.Add("(exclude spending on ticket & camping spot)");
            lbEventInfo.Items.Add("** Fine: Fine paid for lost/ damanged borrowing(s)");
        }

        private void tbcServicePoint_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbcServicePoint.SelectedIndex == 1) // tabLending selected
            {
                if (newSellOrder != null)
                {
                    tbcServicePoint.SelectedIndex = 0;
                    MessageBox.Show("Current order should be cancelled or paid before moving to another tab");
                }
                else
                {
                    RefreshSellingForm();
                }
            }
            else
            {
                if (newRentingOrder != null)
                {
                    tbcServicePoint.SelectedIndex = 1;
                    MessageBox.Show("Current order should be cancelled or paid before moving to another tab");
                }
                else
                {
                    RefreshLendingForm();
                }
            }
        }

        private void btnScanRFIDRent_Click(object sender, EventArgs e)
        {
            rfid_Renting.Open();
        }

        private void RefreshLendingForm()
        {
            btnScanRFIDRent.Enabled = true;
            cbLendingShopList.Enabled = true;
            lbBorrowingItems.Items.Clear();
            tbTotalFee.Text = "";
            rfid_Renting.Close();
            tbBorrowerName.Text = "";
            tbCustomerBalanceLending.Text = "";
            tbSkuReturn.Text = "";
            tbQuantityReturn.Text = "";
            MakeDisable(new List<Control>() {lbBorrowingItems, btnReturnAll, tbSkuReturn, tbQuantityReturn, btnReturnSelectedItem,
                        btnPayFine, btnCancelLending, btnPayRentingFee, tbcLending});
            tempOrder.Clear();
            currentVisitor = null;
            newRentingOrder = null;
            UnloadTabpage(tabLending);
            GenerateLendingItem();
        }

        private void btnCancelLending_Click(object sender, EventArgs e)
        {
            if (newRentingOrder != null)
            {
                DialogResult cancelOrder = MessageBox.Show("Are you sure you want to cancel this order?", "Cancelling...", MessageBoxButtons.YesNo);
                if (cancelOrder == DialogResult.Yes)
                {
                    newRentingOrder.CancelOrder();
                    RefreshLendingForm();
                }
            }
            else
            {
                MessageBox.Show("No order to cancel");
            }
        }

        private void btnPayRentingFee_Click(object sender, EventArgs e)
        {
            try
            {
                newRentingOrder.PayOrder();
                if (newRentingOrder.IsPaid)
                {
                    DialogResult payOrder = MessageBox.Show("Do you want to print this receipt?", "Printing...", MessageBoxButtons.YesNo);
                    if (payOrder == DialogResult.Yes)
                    {
                        newRentingOrder.PrintOrder();
                    }
                    
                    // update database
                    LatitudeEvent.InsertNewOrder(newRentingOrder);

                    // refresh form
                    RefreshLendingForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbSkuReturn_TextChanged(object sender, EventArgs e)
        {
            if (tbSkuReturn.Text != "")
            {
                btnReturnSelectedItem.Enabled = true;
                btnPayFine.Enabled = true;
                btnReturnAll.Enabled = false;
            }
            else
            {
                btnReturnSelectedItem.Enabled = false;
                btnPayFine.Enabled = false;
                btnReturnAll.Enabled = true;
            }
        }

        private void btnReturnAll_Click(object sender, EventArgs e)
        {
            if (currentVisitor.Borrowings.Count != 0)
            {
                while (currentVisitor.Borrowings.Count != 0)
                {
                    ItemForRent item = (ItemForRent)currentVisitor.Borrowings.ElementAt(0).Key;
                    int quantity = currentVisitor.Borrowings.ElementAt(0).Value;
                    currentVisitor.ReturnItem(item, quantity);
                    item.AddToStock(quantity);
                    LatitudeEvent.InsertNewReturn(currentVisitor, item, quantity);
                }
                MessageBox.Show("All borrowings have been returned");
            }
            else
            {
                MessageBox.Show("No borrowings found");
            }

            RefreshLendingForm();
        }

        private void btnReturnSelectedItem_Click(object sender, EventArgs e)
        {
            try
            {
                ItemForRent item = (ItemForRent)myEvent.GetItemBySku(Convert.ToInt32(tbSkuReturn.Text));
                int quantity = Convert.ToInt32(tbQuantityReturn.Text);
                currentVisitor.ReturnItem(item, quantity);
                item.AddToStock(quantity);
                LatitudeEvent.InsertNewReturn(currentVisitor, item, quantity);
                lbBorrowingItems.Items.Clear();
                if (currentVisitor.Borrowings.Count != 0)
                {
                    lbBorrowingItems.Items.Add("Current borrowings:");
                    lbBorrowingItems.Items.Add("--------------------------");
                    foreach (KeyValuePair<EventItem, int> pair in currentVisitor.Borrowings)
                    {
                        lbBorrowingItems.Items.Add(string.Format("Sku {0} - {1} - quantity {2}", pair.Key.Sku, pair.Key.Name, pair.Value));
                    }
                    DialogResult returnItem = MessageBox.Show("Do you want to return more items?", "Returning...", MessageBoxButtons.YesNo);
                    if (returnItem == DialogResult.No)
                    {
                        RefreshLendingForm();
                    }
                    else
                    {
                        tbQuantityReturn.Text = "";
                        tbSkuReturn.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("All borrowings have been returned");
                    RefreshLendingForm();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("SKU and quantity should be numbers");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPayFine_Click(object sender, EventArgs e)
        {
            try
            {
                ItemForRent item = (ItemForRent)myEvent.GetItemBySku(Convert.ToInt32(tbSkuReturn.Text));
                int quantity = Convert.ToInt32(tbQuantityReturn.Text);
                double amount = currentVisitor.PayFine(item, quantity);
                MessageBox.Show(string.Format("Fine (€{0:0.00}) has been paid", amount));
                LatitudeEvent.InsertNewFine(currentVisitor, item, quantity);
                lbBorrowingItems.Items.Clear();
                if (currentVisitor.Borrowings.Count != 0)
                {
                    lbBorrowingItems.Items.Add("Current borrowings:");
                    lbBorrowingItems.Items.Add("--------------------------");
                    foreach (KeyValuePair<EventItem, int> pair in currentVisitor.Borrowings)
                    {
                        lbBorrowingItems.Items.Add(string.Format("Sku {0} - {1} - quantity {2}", pair.Key.Sku, pair.Key.Name, pair.Value));
                    }
                    DialogResult returnItem = MessageBox.Show("Do you want to return more items?", "Returning...", MessageBoxButtons.YesNo);
                    if (returnItem == DialogResult.No)
                    {
                        RefreshLendingForm();
                    }
                    else
                    {
                        tbQuantityReturn.Text = "";
                        tbSkuReturn.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("All borrowings have been returned");
                    RefreshLendingForm();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("SKU and quantity should be numbers");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbSearchSellItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbSearchSellItem.Text != "")
                {
                    try
                    {
                        UnloadTabpage(tabDrink);
                        UnloadTabpage(tabFood);
                        UnloadTabpage(tabSouvenir);
                        if (rbtnSearchItemByName.Checked)
                        {
                            Dictionary<SaleType, List<EventItem>> temp = new Dictionary<SaleType, List<EventItem>>();
                            temp.Add(SaleType.Drink, new List<EventItem>());
                            temp.Add(SaleType.Food, new List<EventItem>());
                            temp.Add(SaleType.Souvenir, new List<EventItem>());
                            foreach (KeyValuePair<SaleType, List<EventItem>> pair in myEvent.GetItemsForSale())
                            {
                                foreach (EventItem item in pair.Value)
                                {
                                    if (item.Name.Contains(tbSearchSellItem.Text))
                                    {
                                        temp[pair.Key].Add(item);
                                    }
                                }
                            }
                            // draw results in tab
                            foreach (KeyValuePair<SaleType, List<EventItem>> pair in temp)
                            {
                                if (pair.Value.Count != 0)
                                {
                                    if (pair.Key == SaleType.Drink)
                                    {
                                        GeneratePictureBox(pair.Value, tabDrink);
                                        GenerateButtonAdd(pair.Value, tabDrink);
                                        GenerateButtonRemove(pair.Value, tabDrink);
                                    }
                                    else if (pair.Key == SaleType.Food)
                                    {
                                        GeneratePictureBox(pair.Value, tabFood);
                                        GenerateButtonAdd(pair.Value, tabFood);
                                        GenerateButtonRemove(pair.Value, tabFood);
                                    }
                                    else
                                    {
                                        GeneratePictureBox(pair.Value, tabSouvenir);
                                        GenerateButtonAdd(pair.Value, tabSouvenir);
                                        GenerateButtonRemove(pair.Value, tabSouvenir);
                                    }
                                }
                            }

                        }
                        else
                        {
                            List<EventItem> temp = new List<EventItem>();
                            foreach (ItemForSale item in myEvent.GetAllItemForSale())
                            {
                                if (item.Sku == Convert.ToInt32(tbSearchSellItem.Text))
                                {
                                    temp.Add(item);
                                }
                            }
                            if (temp.Count != 0)
                            {
                                if (((ItemForSale)temp[0]).ItemType == SaleType.Food)
                                {
                                    GeneratePictureBox(temp, tabFood);
                                    GenerateButtonAdd(temp, tabFood);
                                    GenerateButtonRemove(temp, tabFood);
                                }
                                else if (((ItemForSale)temp[0]).ItemType == SaleType.Drink)
                                {
                                    GeneratePictureBox(temp, tabDrink);
                                    GenerateButtonAdd(temp, tabDrink);
                                    GenerateButtonRemove(temp, tabDrink);
                                }
                                else
                                {
                                    GeneratePictureBox(temp, tabSouvenir);
                                    GenerateButtonAdd(temp, tabSouvenir);
                                    GenerateButtonRemove(temp, tabSouvenir);
                                }
                            }
                        }
                        // select tab containing results
                        if (tabDrink.Controls.Count != 0)
                        {
                            tbcFoodDrink.SelectedIndex = 0;
                        }
                        else if (tabFood.Controls.Count != 0)
                        {
                            tbcFoodDrink.SelectedIndex = 1;
                        }
                        else if (tabSouvenir.Controls.Count != 0)
                        {
                            tbcFoodDrink.SelectedIndex = 2;
                        }
                        else
                        {
                            MessageBox.Show("No items found");
                        }

                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Sku should be a number");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please input a value to search");
                }
            }
        }

        public static void UnloadTabpage(TabPage page)
        {
            while (page.Controls.Count > 0)
            {
                page.Controls[0].Dispose();
            }
        }

        private void tbSearchSellItem_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchSellItem.Text == "")
            {
                UnloadTabpage(tabDrink);
                UnloadTabpage(tabFood);
                UnloadTabpage(tabSouvenir);
                GenerateDrinkItem();
                GenerateFoodItem();
                GenerateSouvenirItem();
            }
        }

        private void btnNameSearch_Click(object sender, EventArgs e)
        {
            if (btnNameSearch.Text != "")
            {
                lbUpdateOverview.Items.Clear();
                foreach (EventItem item in myEvent.Items)
                {
                    if (item.Name.Contains(tbNameSearch.Text))
                    {
                        foreach (string s in Regex.Split(item.AsAString(), "\n"))
                        {
                            lbUpdateOverview.Items.Add(s);
                        }
                    }
                }
                tbNameSearch.Text = "";
            }
            else
            {
                MessageBox.Show("Please input product's name to search");
            }
        }

        private void tbNameSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnNameSearch_Click(this, new EventArgs());
            }
        }

        private void btnUpdateStock_Click(object sender, EventArgs e)
        {
            if (tbItemSku.Text != "")
            {
                EventItem item = myEvent.GetItemBySku(Convert.ToInt32(tbItemSku.Text));
                if (item != null)
                {
                    if (tbQuantityAdded.Text != "")
                    {
                        item.AddToStock(Convert.ToInt32(tbQuantityAdded.Text));
                    }
                    if (tbQuantityRemoved.Text != "")
                    {
                        item.RemoveFromStock(Convert.ToInt32(tbQuantityRemoved.Text));
                    }
                    if (tbSetNewPriceFee.Text != "")
                    {
                        if (item is ItemForRent)
                        {
                            ((ItemForRent)item).SetNewRentingFee(Convert.ToDouble(tbSetNewPriceFee.Text));
                        }
                        else
                        {
                            ((ItemForSale)item).SetNewPrice(Convert.ToDouble(tbSetNewPriceFee.Text));
                        }
                    }
                    LatitudeEvent.UpdateStock(item);
                    lbUpdateOverview.Items.Clear();
                    lbUpdateOverview.Items.Add("************ Updated item ************");
                    foreach (string s in Regex.Split(item.AsAString(), "\n"))
                    {
                        lbUpdateOverview.Items.Add(s);
                    }
                }
                else
                {
                    MessageBox.Show("Item with sku " + tbItemSku.Text + " does not exist");
                }
                tbItemSku.Text = "";
                tbQuantityAdded.Text = "";
                tbQuantityRemoved.Text = "";
                tbSetNewPriceFee.Text = "";
            }
            else
            {
                MessageBox.Show("Please input item SKU");
            }
        }

        private void IntKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != (char)8)
                e.KeyChar = (char)0;
        }

        private void DoubleKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != (char)8)
                e.KeyChar = (char)0;
        }

        private void btnCreateNewItem_Click(object sender, EventArgs e)
        {
            if (tbNewName.Text == "" || tbNewQuantityInStock.Text == "" || tbNewMinQuantity.Text == "" || tbCostPerUnit.Text == "" ||
                tbNewImgLocation.Text == "" || (rbtnForSale.Checked && tbNewSellPrice.Text == "") || (rbtnForRent.Checked && tbNewRentingFee.Text == ""))
            {
                 MessageBox.Show("Fields marked with * are required");
            }
            else
            {
                EventItem item;
                SaleType saleType;
                bool isSuitableForVM;
                if (rbtnForSale.Checked)
                {
                    // sale type
                    if (rbtnFood.Checked)
                    {
                        saleType = SaleType.Food;
                    }
                    else if (rbtnDrink.Checked)
                    {
                        saleType = SaleType.Drink;
                    }
                    else
                    {
                        saleType = SaleType.Souvenir;
                    }

                    // suitable for VM?
                    if (checkIsSuitableForVM.Checked)
                    {
                        isSuitableForVM = true;
                    }
                    else
                    {
                        isSuitableForVM = false;
                    }

                    // create sale item
                    item = new ItemForSale(tbNewName.Text, tbNewImgLocation.Text, Convert.ToDouble(tbCostPerUnit.Text), Convert.ToInt32(tbNewQuantityInStock.Text),
                        Convert.ToInt32(tbNewMinQuantity.Text), Convert.ToDouble(tbNewSellPrice.Text), saleType, isSuitableForVM);
                }
                else
                {
                    // create renting item
                    item = new ItemForRent(tbNewName.Text, tbNewImgLocation.Text, Convert.ToDouble(tbCostPerUnit.Text), Convert.ToInt32(tbNewQuantityInStock.Text),
                        Convert.ToInt32(tbNewMinQuantity.Text), Convert.ToDouble(tbNewRentingFee.Text));
                }

                myEvent.AddItemToEvent(item);
                LatitudeEvent.InsertNewItem(item);

                // show overview in list box
                lbUpdateOverview.Items.Clear();
                lbUpdateOverview.Items.Add("************ New item added ************");
                foreach (string s in Regex.Split(item.AsAString(), "\n"))
                {
                    lbUpdateOverview.Items.Add(s);
                }

                // refresh form
                tbNewName.Text = "";
                tbNewQuantityInStock.Text = "";
                tbNewMinQuantity.Text = "";
                tbCostPerUnit.Text = "";
                rbtnForSale.Checked = true;
                tbNewImgLocation.Text = "";
                tbNewSellPrice.Text = "";
                rbtnFood.Checked = true;
                checkIsSuitableForVM.Checked = false;
                tbNewRentingFee.Text = "";
            }
        }

        private void btnSelectImgLocation_Click(object sender, EventArgs e)
        {
            ofdSelectImage.Filter = "Image Files | *.png";
            if (this.ofdSelectImage.ShowDialog() == DialogResult.Cancel)
            {
                MessageBox.Show("Image not selected");
            }
            else
            {
                string filePath = this.ofdSelectImage.FileName;
                string referencePath = @"D:\FONTYS\PROP\git\prop\latittude_CSharp\latittude\bin\Debug\";
                tbNewImgLocation.Text = MakeRelative(filePath, referencePath);
            }
        }

        private static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }

        private void btnShowAllItems_Click(object sender, EventArgs e)
        {
            lbStockDetail.Items.Clear();
            WriteToStockDetailListBox(myEvent.Items);
        }

        private void btnShowAllInStock_Click(object sender, EventArgs e)
        {
            lbStockDetail.Items.Clear();
            List<EventItem> temp = new List<EventItem>();
            foreach(EventItem item in myEvent.Items)
            {
                if (item.QuantityInStock > 0)
                {
                    temp.Add(item);
                }
            }
            WriteToStockDetailListBox(temp);
        }

        private void btnShowAllOutOfStock_Click(object sender, EventArgs e)
        {
            lbStockDetail.Items.Clear();
            List<EventItem> temp = new List<EventItem>();
            foreach (EventItem item in myEvent.Items)
            {
                if(item.QuantityInStock == 0)
                {
                    temp.Add(item);
                }
            }
            WriteToStockDetailListBox(temp);
        }

        private void btnShowAllNeedOrder_Click(object sender, EventArgs e)
        {
            lbStockDetail.Items.Clear();
            List<EventItem> temp = new List<EventItem>();
            foreach (EventItem item in myEvent.Items)
            {
                if (item.QuantityInStock < item.QuantityMin)
                {
                    temp.Add(item);
                }
            }
            WriteToStockDetailListBox(temp);
        }

        private void btnSortStock_Click(object sender, EventArgs e)
        {
            if (checkSortByType.Checked)
            {
                List<EventItem> tempFood = myEvent.GetItemsForSale()[SaleType.Food];
                List<EventItem> tempDrink = myEvent.GetItemsForSale()[SaleType.Drink];
                List<EventItem> tempSouvenir = myEvent.GetItemsForSale()[SaleType.Souvenir];
                List<EventItem> tempRent = myEvent.GetItemsForRent();

                if (rbtnSkuCompare.Checked)
                {
                    lbStockDetail.Items.Clear();

                    lbStockDetail.Items.Add("****************** ITEM FOR SALE ******************");
                    lbStockDetail.Items.Add("             ********** Food **********");
                    tempFood.Sort();
                    WriteToStockDetailListBox(tempFood);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add("             ********** Drink **********");
                    tempDrink.Sort();
                    WriteToStockDetailListBox(tempDrink);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add("            ********** Souvenir **********");
                    tempSouvenir.Sort();
                    WriteToStockDetailListBox(tempSouvenir);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add("***************** ITEM FOR RENT ******************");
                    tempRent.Sort();
                    WriteToStockDetailListBox(tempRent);

                }
                else if (rbtnQuantityInStockCompare.Checked)
                {
                    lbStockDetail.Items.Clear();
                    EventItemComparer compare = new EventItemComparer();

                    lbStockDetail.Items.Add("****************** ITEM FOR SALE ******************");
                    lbStockDetail.Items.Add("             ********** Food **********");
                    tempFood.Sort(compare);
                    WriteToStockDetailListBox(tempFood);

                    lbStockDetail.Items.Add("");
                    EventItemComparer compareDrink = new EventItemComparer();
                    lbStockDetail.Items.Add("             ********** Drink **********");
                    tempDrink.Sort(compare);
                    WriteToStockDetailListBox(tempDrink);

                    lbStockDetail.Items.Add("");
                    EventItemComparer compareSouvenir = new EventItemComparer();
                    lbStockDetail.Items.Add("            ********** Souvenir **********");
                    tempSouvenir.Sort(compare);
                    WriteToStockDetailListBox(tempSouvenir);

                    lbStockDetail.Items.Add("");
                    EventItemComparer compareRent = new EventItemComparer();
                    lbStockDetail.Items.Add("***************** ITEM FOR RENT ******************");
                    tempRent.Sort(compare);
                    WriteToStockDetailListBox(tempRent);
                }
                else
                {
                    lbStockDetail.Items.Clear();
                    lbStockDetail.Items.Add("****************** ITEM FOR SALE ******************");
                    lbStockDetail.Items.Add("             ********** Food **********");
                    SortItemByQuantitySold(tempFood);
                    WriteToStockDetailListBox(tempFood);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add("             ********** Drink **********");
                    SortItemByQuantitySold(tempDrink);
                    WriteToStockDetailListBox(tempDrink);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add("            ********** Souvenir **********");
                    SortItemByQuantitySold(tempSouvenir);
                    WriteToStockDetailListBox(tempSouvenir);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add("***************** ITEM FOR RENT ******************");
                    SortItemByRentedTimes(tempRent);
                    WriteToStockDetailListBox(tempRent);
                }
            }
            else
            {
                if (rbtnSkuCompare.Checked)
                {
                    myEvent.Items.Sort();
                    lbStockDetail.Items.Clear();
                    lbStockDetail.Items.Add("****************** ALL EVENT ITEMS ******************");
                    WriteToStockDetailListBox(myEvent.Items);
                }
                else if (rbtnQuantityInStockCompare.Checked)
                {
                    lbStockDetail.Items.Clear();
                    lbStockDetail.Items.Add("****************** ALL EVENT ITEMS ******************");
                    EventItemComparer compare = new EventItemComparer();
                    myEvent.Items.Sort(compare);
                    WriteToStockDetailListBox(myEvent.Items);
                }
                else
                {
                    lbStockDetail.Items.Clear();
                    lbStockDetail.Items.Add("****************** ALL EVENT ITEMS ******************");
                    lbStockDetail.Items.Add(" ****************** Item for sale ******************");
                    List<EventItem> tempSale = myEvent.GetAllItemForSale();
                    SortItemByQuantitySold(tempSale);
                    WriteToStockDetailListBox(tempSale);

                    lbStockDetail.Items.Add("");
                    lbStockDetail.Items.Add(" ****************** Item for rent ******************");
                    List<EventItem> tempRent = myEvent.GetItemsForRent();
                    SortItemByRentedTimes(tempRent);
                    WriteToStockDetailListBox(tempRent);
                }
            }
        }

        private void WriteToStockDetailListBox(List<EventItem> myList)
        {
            foreach (EventItem item in myList)
            {
                foreach (string s in Regex.Split(item.AsAString(), "\n"))
                {
                    lbStockDetail.Items.Add(s);
                }
                if (item is ItemForRent)
                {
                    lbStockDetail.Items.Add(string.Format("\t+ total rented times: {0}", LatitudeEvent.GetQuantitySoldOrTimesRented(item)));
                }
                else
                {
                    lbStockDetail.Items.Add(string.Format("\t+ total quantity sold: {0}", LatitudeEvent.GetQuantitySoldOrTimesRented(item)));
                }
            }
        }

        private void tbcStockManagement_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tbcStockManagement.SelectedIndex == 0)
            {
                // clear tab Stock detail
                lbStockDetail.Items.Clear();
            }
            else
            {
                // clear tab Update Stock
                lbUpdateOverview.Items.Clear();
            }
        }

        private void btnSearchEmp_Click(object sender, EventArgs e)
        {
            Person emp = myEvent.GetPersonById(Convert.ToInt32(tbExistingEmpNr.Text));
            if (emp != null && emp is Employee)
            {
                pnlChangeEmpLevel.Enabled = true;
                btnSearchEmp.Enabled = false;
                cbNewEmpLevel.Items.AddRange(new string[] { "level 1", "level 2", "level 3" });
                cbNewEmpWorkPlace.Items.AddRange(new string[] { "entrance check", "shop", "other" });
                tbOldFirstName.Text = emp.FirstName;
                tbOldLastName.Text = emp.LastName;
                if(((Employee)emp).Level == EmpLevel.Level1)
                {
                    tbOldLevel.Text = "level 1";
                    tbOldWorkPlace.Text = "non applicable";
                }
                else if (((Employee)emp).Level == EmpLevel.Level2)
                {
                    tbOldLevel.Text = "level 2";
                    tbOldWorkPlace.Text = "non applicable";

                }
                else
                {
                    tbOldLevel.Text = "level 3";
                    if(((Employee)emp).Place == WorkPlaceForLevel3.EntranceCheck)
                    {
                        tbOldWorkPlace.Text = "entrance check";
                    }
                    else if (((Employee)emp).Place == WorkPlaceForLevel3.Shop)
                    {
                        tbOldWorkPlace.Text = "shop";
                        tbOldShopId.Text = ((Employee)emp).GetShop().PointId.ToString();
                    }
                    else
                    {
                        tbOldWorkPlace.Text = "other";
                    }
                }
            }
            else
            {
                MessageBox.Show("No employee with id " + tbExistingEmpNr.Text + " found");
            }
        }

        private void tbExistingEmpNr_TextChanged(object sender, EventArgs e)
        {
            if(tbExistingEmpNr.Text == "")
            {
                RefreshExistingEmpForm();
            }
        }

        private void tbExistingEmpNr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnSearchEmp.Enabled)
            {
                btnSearchEmp_Click(this, new EventArgs());
            }
        }

        private void btnCreateNewEmp_Click(object sender, EventArgs e)
        {
            if(tbNewFirstName.Text == "" || tbNewLastName.Text == "" || cbEmpLevel.Text == "" || cbWorkPlace.Text == "" || tbNewAddress.Text == "" ||
                tbNewEmail.Text == "" || tbUserName.Text == "" || tbPassword.Text == "")
            {
                MessageBox.Show("Missing required information");
            }
            else
            {
                try
                {
                    Person emp = null;
                    Gender gender;
                    if (rbtnMale.Checked) { gender = Gender.Male; }
                    else { gender = Gender.Female; }

                    EmpLevel level;
                    if (cbEmpLevel.Text == "level 1") { level = EmpLevel.Level1; }
                    else if (cbEmpLevel.Text == "level 2") { level = EmpLevel.Level2; }
                    else { level = EmpLevel.Level3; }

                    WorkPlaceForLevel3 workPlace;
                    if (cbWorkPlace.Text == "shop") { workPlace = WorkPlaceForLevel3.Shop; }
                    else if (cbWorkPlace.Text == "entrance check") { workPlace = WorkPlaceForLevel3.EntranceCheck; }
                    else if (cbWorkPlace.Text == "other") { workPlace = WorkPlaceForLevel3.Other; }
                    else { workPlace = WorkPlaceForLevel3.NonApplicable; }

                    if (cbEmpLevel.Text == "level 1" || cbEmpLevel.Text == "level 2")
                    {
                        emp = new Employee(LatitudeEvent.GetNextEmployeeNr() + 1,  tbNewFirstName.Text, tbNewLastName.Text, Convert.ToDateTime(dateTimeDOB.Text), gender, tbNewAddress.Text,
                            tbNewEmail.Text, tbUserName.Text, tbPassword.Text, level);
                    }
                    else
                    {
                        if (cbWorkPlace.Text == "shop")
                        {
                            emp = new Employee(LatitudeEvent.GetNextEmployeeNr() + 1, tbNewFirstName.Text, tbNewLastName.Text, Convert.ToDateTime(dateTimeDOB.Text), gender, tbNewAddress.Text,
                                tbNewEmail.Text, tbUserName.Text, tbPassword.Text, myEvent.GetShopById(Convert.ToInt32(cbShopList.Text)));     
                        }
                        else
                        {
                            emp = new Employee(LatitudeEvent.GetNextEmployeeNr() + 1, tbNewFirstName.Text, tbNewLastName.Text, Convert.ToDateTime(dateTimeDOB.Text), gender, tbNewAddress.Text,
                            tbNewEmail.Text, tbUserName.Text, tbPassword.Text, workPlace);
                        }
                    }

                    myEvent.AddPersonToEvent(emp);
                    LatitudeEvent.InsertNewEmployee((Employee)emp);

                    lbEmployees.Items.Clear();
                    lbEmployees.Items.Add("******* New employee info *******");
                    lbEmployees.Items.Add(emp.AsAString());

                    RefreshNewEmpForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnSaveReport_Click(object sender, EventArgs e)
        {
            btnSaveReport.Enabled = false;
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream("../../../documents/reports/report_" + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".txt", FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);
                foreach(var item in lbEventInfo.Items)
                {
                    sw.WriteLine(item.ToString());
                }
                lbEventInfo.Items.Clear();
                lbEventInfo.Items.Add("Report's now ready in document folder");
            }
            catch (IOException)
            {
                MessageBox.Show("Something wrong with files");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if(sw != null)
                {
                    sw.Close();
                }
            }
        }

        private void cbSpotId_TextChanged(object sender, EventArgs e)
        {
            if(tbStatusSpotCheck.Text == "not yet")
            {
                if(cbSpotId.Text == "")
                {
                    tbCampingFee.Text = "0.00";
                }
                else
                {
                    if(currentVisitor.TicketType == Ticket.GroupAtGate || currentVisitor.TicketType == Ticket.GroupOnline)
                    {
                        tbCampingFee.Text = string.Format("{0:n2}", EventPriceList.CampFee + EventPriceList.CampFeePerGroup);
                    }
                    else
                    {
                        if(myEvent.GetSpotById(cbSpotId.Text).Renters.Count == 0)
                        {
                            tbCampingFee.Text = string.Format("{0:n2}", EventPriceList.CampFee + EventPriceList.CampFeePerPerson);
                        }
                        else
                        {
                            tbCampingFee.Text = string.Format("{0:n2}", EventPriceList.CampFeePerPerson);
                        }
                    }
                }
            }
            else
            {
                tbCampingFee.Text = "";
            }
        }

        private void btnAssignVisitorToSpot_EnabledChanged(object sender, EventArgs e)
        {
            if (!btnAssignVisitorToSpot.Enabled)
            {
                tbCampingFee.Text = "";
            }
        }

        private void tbBalance_TextChanged(object sender, EventArgs e)
        {
            if(tbBalance.Text == "")
            {
                balance = 0;
            }
            else
            {
                balance = Convert.ToDouble(tbBalance.Text);
            }
            amountToPay = balance + ticketPrice + campFee;
            tbTotalToPay.Text = string.Format("{0:n2}", amountToPay);
        }

        private void rbtnRegularTicket_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnRegularTicket.Checked)
            {
                ticketPrice = EventPriceList.RegularAtGateTicket;
                rbtnSpotCode.Checked = true;
            }
            else if (rbtnVipTicket.Checked)
            {
                ticketPrice = EventPriceList.VIPAtGateTicket;
                rbtnSpotCode.Checked = true;
            }
            else if(rbtnGroupTicket.Checked)
            {
                ticketPrice = EventPriceList.GroupAtGateTicket;
            }
            amountToPay = balance + ticketPrice + campFee;
            tbTotalToPay.Text = string.Format("{0:n2}", amountToPay);
        }

        private int CompareItemByQuantitySold(EventItem first, EventItem second)
        {
            if (first is ItemForSale && second is ItemForSale)
            {
                return LatitudeEvent.GetQuantitySoldOrTimesRented(second) - LatitudeEvent.GetQuantitySoldOrTimesRented(first);
            }
            else
            {
                throw new LatitudeException("Cannot compare these 2 items by quantity sold");
            }
        }

        private int CompareItemByTimesRented(EventItem first, EventItem second)
        {
            if (first is ItemForRent && second is ItemForRent)
            {
                return LatitudeEvent.GetQuantitySoldOrTimesRented(second) - LatitudeEvent.GetQuantitySoldOrTimesRented(first);
            }
            else
            {
                throw new LatitudeException("Cannot compare these 2 items by rented times");
            }
        }

        private void SortItemByQuantitySold(List<EventItem> myList)
        {
            myList.Sort(new Comparison<EventItem>(CompareItemByQuantitySold));
        }

        private void SortItemByRentedTimes(List<EventItem> myList)
        {
            myList.Sort(new Comparison<EventItem>(CompareItemByTimesRented));
        }

    }
}
