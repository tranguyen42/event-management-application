using System;
using System.Collections.Generic;
using System.IO;

namespace LatitudeClassLibrary
{
 
    public class Event
    {
        // fields
        private int id;
        private DateTime eventTime;
        private List<Person> persons;
        private List<ServicePoint> servicePoints;
        private List<CampingSpot> spots;
        private int maxNrVisitors;
        private List<EventItem> items;

        //private DataHelper myDataHelper;

        private List<ServicePoint> sellingShops;
        private List<ServicePoint> lendingShops;
        private List<ServicePoint> vendingMachines;

        // properties
        public int Id { get { return id; } }
        public DateTime EventTime { get { return eventTime; } internal set { eventTime = value; } }
        public int MaxNrVisitors { get { return maxNrVisitors; } internal set { maxNrVisitors = value; } }
        public List<ServicePoint> ServicePoints { get { return servicePoints; } }
        public List<CampingSpot> Spots { get { return spots; } internal set { spots = value; } }
        public List<EventItem> Items { get { return items; } }

        public List<ServicePoint> SellingShops { get { return sellingShops; } }
        public List<ServicePoint> LendingShops { get { return lendingShops; } }
        public List<ServicePoint> VendingMachines { get { return vendingMachines; } }

        // constructors
        public Event(int id)
        {
            this.id = id;
            persons = new List<Person>();
            servicePoints = new List<ServicePoint>();
            spots = new List<CampingSpot>();
            items = new List<EventItem>();
        }

        // event methods
        private List<ServicePoint> GetSellingShops()
        {
            List<ServicePoint> temp = new List<ServicePoint>();
            foreach (ServicePoint p in servicePoints)
            {
                if (p is SellingShop)
                {
                    temp.Add(p);
                }
            }
            return temp;
        }

        private List<ServicePoint> GetLendingShops()
        {
            List<ServicePoint> temp = new List<ServicePoint>();
            foreach (ServicePoint p in servicePoints)
            {
                if (p is LendingShop)
                {
                    temp.Add(p);
                }
            }
            return temp;
        }

        private List<ServicePoint> GetVendingMachines()
        {
            List<ServicePoint> temp = new List<ServicePoint>();
            foreach (ServicePoint p in servicePoints)
            {
                if (p is VendingMachine)
                {
                    temp.Add(p);
                }
            }
            return temp;
        }

        public void AddServicePoint(ServicePoint p)
        {
            if (GetShopById(p.PointId) == null)
            {
                servicePoints.Add(p);
                sellingShops = GetSellingShops();
                lendingShops = GetLendingShops();
                vendingMachines = GetVendingMachines();
            }
            else
            {
                throw new LatitudeException("Shop already added");
            }
        }

        public List<Visitor> GetVisitors()
        {
            List<Visitor> temp = new List<Visitor>();
            foreach(Person p in persons)
            {
                if(p is Visitor)
                {
                    temp.Add((Visitor)p);
                }
            }
            return temp;
        }

        public List<Employee> GetEmployees()
        {
            List<Employee> temp = new List<Employee>();
            foreach (Person p in persons)
            {
                if (p is Employee)
                {
                    temp.Add((Employee)p);
                }
            }
            return temp;
        }

        public Person GetPersonById(int id)
        {
            foreach(Person p in persons)
            {
                if (p.Id == id)
                {
                    return p;
                }
            }
            return null;
        }

        public ServicePoint GetShopById(int id)
        {
            foreach(ServicePoint p in servicePoints)
            {
                if (p.PointId == id)
                {
                    return p;
                }
            }
            return null;
        }

        public void AddPersonToEvent(Person p)
        {
            if (GetPersonById(p.Id) == null)
            {
                if (p is Visitor)
                {
                    if (((Visitor)p).TicketType == Ticket.GroupAtGate || ((Visitor)p).TicketType == Ticket.GroupOnline)
                    {
                        if (GetCurrentNrOfVisitors() + 6 <= maxNrVisitors)
                        {
                            persons.Add(p);
                        }
                        else
                        {
                            throw new LatitudeException("Event is full. Group ticket's no longer available");
                        }
                    }
                    else
                    {
                        if (GetCurrentNrOfVisitors() < maxNrVisitors)
                        {
                            persons.Add(p);
                        }
                        else
                        {
                            throw new LatitudeException("Event is full. All tickets sold out");
                        }
                    }
                }
                else if (p is Employee)
                {
                    persons.Add(p);
                }
                }
           else
           {
                throw new LatitudeException("Person with id " + p.Id + " already added to event");
           }        
        }

        internal void AddPersonListToEvent(List<Person> people)
        {
            foreach(Person p in people)
            {
                AddPersonToEvent(p);
            }
        }

        public int GetCurrentNrOfVisitors()
        {
            int currentNrOfVisitors = 0;
            foreach (Visitor v in GetVisitors())
            {
                if (v.TicketType == Ticket.GroupAtGate || v.TicketType == Ticket.GroupOnline)
                {
                    currentNrOfVisitors += 6;
                }
                else
                {
                    currentNrOfVisitors++;
                }
            }
            return currentNrOfVisitors;
        }

        public int GetCurrentNrOfCheckInVisitors()
        {
            int currentNrOfVisitors = 0;
            foreach (Visitor v in GetVisitors())
            {
                if (v.IsCheckedIn)
                {
                    if (v.TicketType == Ticket.GroupAtGate || v.TicketType == Ticket.GroupOnline)
                    {
                        currentNrOfVisitors += 6;
                    }
                    else
                    {
                        currentNrOfVisitors++;
                    }
                }
            }
            return currentNrOfVisitors;
        }

        public Dictionary<Ticket, double> CalculateRevenueFromTickets()
        {
            Dictionary<Ticket, double> temp = new Dictionary<Ticket, double>();
            double amountGroupAtGate = 0;
            double amountGroupOnline = 0;
            double amountRegularAtGate = 0;
            double amountRegularOnline = 0;
            double amountVipAtGate = 0;
            double amountVipOnline = 0;
            foreach(Visitor v in GetVisitors())
            {
                switch(v.TicketType)
                {
                    case Ticket.GroupAtGate:
                        amountGroupAtGate += EventPriceList.GroupAtGateTicket;
                        break;
                    case Ticket.GroupOnline:
                        amountGroupOnline += EventPriceList.GroupOnlineTicket;
                        break;
                    case Ticket.RegularAtGate:
                        amountRegularAtGate += EventPriceList.RegularAtGateTicket;
                        break;
                    case Ticket.RegularOnline:
                        amountRegularOnline += EventPriceList.RegularOnlineTicket;
                        break;
                    case Ticket.VipAtGate:
                        amountVipAtGate += EventPriceList.VIPAtGateTicket;
                        break;
                    case Ticket.VipOnline:
                        amountVipOnline += EventPriceList.VIPOnlineTicket;
                        break;   
                }
            }
            temp.Add(Ticket.GroupAtGate, amountGroupAtGate);
            temp.Add(Ticket.GroupOnline, amountGroupOnline);
            temp.Add(Ticket.RegularAtGate, amountRegularAtGate);
            temp.Add(Ticket.RegularOnline, amountRegularOnline);
            temp.Add(Ticket.VipAtGate, amountVipAtGate);
            temp.Add(Ticket.VipOnline, amountVipOnline);
            return temp;
        }

        public double CalculateRevenueFromCamping()
        {
            double amount = 0;
            foreach(CampingSpot spot in spots)
            {
                amount += spot.CalculateRevenueFromCampingSpot();
            }
            return amount;
        }

        public double CalculateTotalRevenueFromTickets()
        {
            return CalculateRevenueFromTickets()[Ticket.GroupAtGate] + CalculateRevenueFromTickets()[Ticket.GroupOnline] + CalculateRevenueFromTickets()[Ticket.RegularAtGate]
                + CalculateRevenueFromTickets()[Ticket.RegularOnline] + CalculateRevenueFromTickets()[Ticket.VipAtGate] + CalculateRevenueFromTickets()[Ticket.VipOnline];
        }
        
        public EventItem GetItemBySku(int sku)
        {
            foreach(EventItem i in items)
            {
                if (i.Sku == sku)
                {
                    return i;
                }
            }
            return null;
        }
        public void AddItemToEvent(EventItem i)
        {
            if(GetItemBySku(i.Sku) == null)
            {
                items.Add(i);
            }
            else
            {
                throw new LatitudeException("Item with sku " + i.Sku + " already added to event");
            }
        }

        public List<EventItem> GetItemsForRent()
        {
            List<EventItem> temp = new List<EventItem>();
            foreach (EventItem i in items)
            {
                if (i is ItemForRent)
                {
                    temp.Add(i);
                }
            }
            return temp;
        }

        public Dictionary<SaleType, List<EventItem>> GetItemsForSale()
        {
            Dictionary<SaleType, List<EventItem>> temp = new Dictionary<SaleType, List<EventItem>>();
            List<EventItem> tempDrink = new List<EventItem>();
            List<EventItem> tempFood = new List<EventItem>();
            List<EventItem> tempSouvenir = new List<EventItem>();
            foreach (EventItem i in items)
            {
                if (i is ItemForSale)
                {
                    if (((ItemForSale)i).ItemType == SaleType.Drink)
                    {
                        tempDrink.Add(i);
                    }
                    else if (((ItemForSale)i).ItemType == SaleType.Food)
                    {
                        tempFood.Add(i);
                    }
                    else
                    {
                        tempSouvenir.Add(i);
                    }
                }
            }
            temp.Add(SaleType.Drink, tempDrink);
            temp.Add(SaleType.Food, tempFood);
            temp.Add(SaleType.Souvenir, tempSouvenir);
            return temp;
        }

        public List<EventItem> GetAllItemForSale()
        {
            List<EventItem> temp = new List<EventItem>();
            foreach(EventItem item in items)
            {
                if (item is ItemForSale)
                {
                    temp.Add(item);
                }
            }
            return temp;
        }
        
        public CampingSpot GetSpotById(string id)
        {
            foreach (CampingSpot spot in spots)
            {
                if (spot.SpotId == id)
                {
                    return spot;
                }
            }
            return null;
        }

        public CampingSpot GetSpotBySpotCode(string code)
        {
            foreach (CampingSpot spot in spots)
            {
                if (spot.SpotCode == code)
                {
                    return spot;
                }
            }
            return null;
        }

        /*
        public CampingSpot GetSpotReservedByAVisitor(Visitor v)
        {
            foreach (CampingSpot spot in spots)
            {
                if (spot.Renters.Contains(v))
                {
                    return spot;
                }
            }

            return null;
        }
        */

        public Visitor GetVisitorByTicketNr(int ticketNr)
        {
            foreach (Visitor v in GetVisitors())
            {
                if (v.TicketNr == ticketNr)
                {
                    return v;
                }
            }
            return null;
        }

        public Visitor GetVisitorByRfid(string rfid)
        {
            foreach (Visitor v in GetVisitors())
            {
                if (v.Rfid == rfid)
                {
                    return v;
                }
            }
            return null;
        }

        public Visitor GetVisitorByEventAccount(int account)
        {
            foreach(Visitor v in GetVisitors())
            {
                if(v.EventAccount == account)
                {
                    return v;
                }
            }
            return null;
        }
    }
}
