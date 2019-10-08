using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LatitudeClassLibrary
{
    
    public enum Ticket
    {
        RegularOnline, RegularAtGate, VipOnline, VipAtGate, GroupOnline, GroupAtGate
    }
    [Serializable]
    public class Visitor : Person
    {
        // fields
        private static Random randomNr = new Random();
        private static List<int> issuedTickets = new List<int>();
        private static List<int> eventAccounts = new List<int>();

        private string rfid;
        private int eventAccount;
        private double balance;
        private int ticketNr;
        private bool isCheckedIn;
        private Dictionary<EventItem, int> borrowings;
        private Ticket ticketType;
        private string spotCode;
        private bool hasReserveCamp;
        private CampingSpot reservedSpot;

        // properties
        public string Rfid { get { return rfid; } }
        public int EventAccount { get { return eventAccount; } }
        public double Balance { get { return balance; } }
        public int TicketNr { get { return ticketNr; } }
        public bool IsCheckedIn { get { return isCheckedIn; } }
        public Dictionary<EventItem, int> Borrowings { get { return borrowings; } }
        public Ticket TicketType { get { return ticketType; } }
        public string SpotCode { get { return spotCode; } }
        public bool HasReserveCamp { get { return hasReserveCamp; } }
        public CampingSpot ReservedSpot { get { return reservedSpot; } }

        // constructors
      
        public Visitor(int id, string first, string last, DateTime dob, Gender gender, Ticket ticketType, double balance)
            : base(id, first, last, dob, gender)  // visitor buying ticket at gate, no camp reserved
        {
            this.eventAccount = GenerateEventAccountNr();
            this.ticketType = ticketType;
            isCheckedIn = false;
            borrowings = new Dictionary<EventItem, int>();
            this.balance = balance;
            rfid = "";
            ticketNr = GenerateTicketNr();
            spotCode = "";
            hasReserveCamp = false;
            reservedSpot = null;
        }
        
        public Visitor(int id, string first, string last, Gender gender, int ticketNr, Ticket ticketType, double balance)
            : base(id, first, last, gender)
        {// constructor for visitors who already bought tickets online, event account is unknown, no camping reserved
            this.eventAccount = GenerateEventAccountNr();

            this.ticketType = ticketType;
            isCheckedIn = false;
            borrowings = new Dictionary<EventItem, int>();
            this.balance = balance;
            this.rfid = "";

            this.ticketNr = ticketNr;
            issuedTickets.Add(ticketNr);

            spotCode = "";
            hasReserveCamp = false;
            reservedSpot = null; 
        }

        public Visitor(int id, string first, string last, Gender gender, int ticketNr, Ticket ticketType, int eventAccount, double balance)
            : base(id, first, last, gender)
        {// constructor for visitors who already bought tickets online, event account is known, no camping reserved
            this.eventAccount = eventAccount;
            eventAccounts.Add(eventAccount);

            this.ticketType = ticketType;
            isCheckedIn = false;
            borrowings = new Dictionary<EventItem, int>();
            this.balance = balance;
            this.rfid = "";

            this.ticketNr = ticketNr;
            issuedTickets.Add(ticketNr);

            spotCode = "";
            hasReserveCamp = false;
            reservedSpot = null;
        }
       
        // methods

        private int GenerateTicketNr()
        {
            int newTicketNr = randomNr.Next(100000, 999999);
            while (issuedTickets.Contains(newTicketNr))
            {
                newTicketNr = randomNr.Next(100000, 999999);
            }
            issuedTickets.Add(newTicketNr);
            return newTicketNr;
        }

        private int GenerateEventAccountNr()
        {
            int  newEventAccount = randomNr.Next(10000, 99999);
            while (eventAccounts.Contains(newEventAccount))
            {
                newEventAccount = randomNr.Next(10000, 99999);
            }
            eventAccounts.Add(newEventAccount);
            return newEventAccount;
        }

        public void Deposit (double amount)
        {
            balance += amount;
        }

        public bool Withdraw (double amount)
        {
            if (amount <= balance)
            {
                balance -= amount;
                return true;
            }
            return false;
        }

        public void CheckOut()
        {
            if (isCheckedIn)
            {
                if (borrowings.Count == 0)
                {
                    isCheckedIn = false;
                    rfid = "";
                }
                else
                {
                    throw new LatitudeException("Visitor not allowed to check out");
                }
            }
            else
            {
                throw new LatitudeException("Visitor with ticket nr. " + ticketNr + " not checked in yet. Something went wrong");
            }
        }

        public bool CheckIn()
        {
            if (!isCheckedIn)
            {
                isCheckedIn = true;
                return true;
            }

            return false;
        }

        public void AssignNewRfid(string newRfid)
        {
            rfid = newRfid;
        }

        public double PayFine(ItemForRent item, int quantity)
        {
            if (item is ItemForRent)
            {
                if (borrowings.ContainsKey(item) && borrowings[item] >= quantity)
                {
                    double amount = item.CostPerUnit * quantity;
                    if (amount <= balance)
                    {
                        balance -= amount;
                        borrowings[item] -= quantity;
                        if (borrowings[item] == 0) { borrowings.Remove(item); }
                        return amount;
                    }
                    else
                    {
                        throw new LatitudeException(string.Format("Balance is not enough. Fine (€{0:0.00}) has not been paid", amount));
                    }
                }
                else if (!borrowings.ContainsKey(item))
                {
                    throw new LatitudeException("Item not in borrowings");
                }
                else
                {
                    throw new LatitudeException("Quantity input exceeds actual borrowing quantity");
                }
            }
            else
            {
                throw new LatitudeException("Selected item is not for rent");
            }
        }

        public void ReturnItem(ItemForRent item, int quantity)
        {
            if (item is ItemForRent)
            {
                if (borrowings.ContainsKey(item) && borrowings[item] >= quantity)
                {
                    borrowings[item] -= quantity;
                    if (borrowings[item] == 0) { borrowings.Remove(item); }
                    //item.AddToStock(quantity);
                }
                else if (!borrowings.ContainsKey(item))
                {
                    throw new LatitudeException("Item not in borrowings");
                }
                else
                {
                    throw new LatitudeException("Quantity input exceeds actual borrowing quantity");
                }
            }
            else
            {
                throw new LatitudeException("Selected item is not for rent");
            }
        }

        internal void BorrowItem(ItemForRent item, int quantity)
        {
            if (borrowings.ContainsKey(item))
            {
                borrowings[item] += quantity;
            }
            else
            {
                borrowings.Add(item, quantity);
            }
        }

        public bool ReserveCampingSpot(CampingSpot spot)
        {
            if (spot.AddMoreRenter(this))
            {
                hasReserveCamp = true;
                reservedSpot = spot;
                if (spot.Renters[0] == this)
                {
                    spotCode = spot.SpotCode;
                }
                return true;
            }
            hasReserveCamp = false;
            return false;
        }

        public override string AsAString()
        {
            string ticket = "";
            if (ticketType == Ticket.GroupOnline || ticketType == Ticket.GroupAtGate)
            {
                ticket = "group ticket ";
            }
            else if (ticketType == Ticket.RegularAtGate || ticketType == Ticket.RegularOnline)
            {
                ticket = "regular ticket ";
            }
            else
            {
                ticket = "vip ticket ";
            }

            string reserve = "";
            if (hasReserveCamp)
            {
                reserve = "has reserved camp " + reservedSpot.SpotId;
            }
            else
            {
                reserve = "no camp reserved";
            }

            if (balance != 0)
            {
                return string.Format("{0}\n\t+{1} nr. {2}\n\t+event account {3} with balance €{4:.00}\n\t+{5}", base.AsAString(), ticket, ticketNr, eventAccount, balance, reserve);
            }
            else
            {
                return string.Format("{0}\n\t+{1} nr. {2}\n\t+event account {3}\n\t+{4}", base.AsAString(), ticket, ticketNr, eventAccount, reserve);
            }
        }

        public void PrintTicket()
        {
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream("../../../documents/ticketPrinted/ticket_" + ticketNr + ".txt", FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);

                sw.WriteLine("      Latitude Music Festival         ");
                sw.WriteLine("--------------------------------------");
                sw.WriteLine("Visitor name    :   " + FirstName + " " + LastName);
                sw.WriteLine("Date of birth   :   " + Dob.ToShortDateString());
                sw.WriteLine("Event account   :   " + eventAccount);
                sw.WriteLine("Ticket number   :   " + ticketNr);
                if (hasReserveCamp)
                {
                    sw.WriteLine("Camping spot    :   " + reservedSpot.SpotId);
                    if (spotCode != "")
                    {
                        sw.WriteLine("Spot code       :   " + spotCode);
                    }
                }
            }
            catch (IOException)
            {
                throw new LatitudeException("Something wrong with printing");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
        }

    }
}
