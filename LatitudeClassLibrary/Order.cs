using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LatitudeClassLibrary
{
   
    public abstract class Order
    {
        // fields
        private int orderNr;
        private Visitor customer;
        private Employee cashier;
        private DateTime orderTime;
        private Dictionary<EventItem, int> orderLineItem;
        private static List<int> orderNrs = new List<int>();
        private bool isPaid;
        private ServicePoint shop;

        // properties
        public int OrderNr { get { return orderNr; } }
        public Visitor Customer { get { return customer; } }
        public Employee Cashier { get { return cashier; } }
        public DateTime OrderTime { get { return orderTime; } }
        public Dictionary<EventItem, int> OrderLineItem { get { return orderLineItem; } }
        public bool IsPaid { get { return isPaid; } }
        public ServicePoint Shop { get { return shop; } }

        // constructors
        public Order(int orderNr, Visitor customer, Employee cashier, Dictionary<EventItem, int> orderline, ServicePoint shop) // new order created from app
        {
            this.customer = customer;
            this.cashier = cashier;
            this.shop = shop;
            orderTime = DateTime.Now;
            this.orderLineItem = orderline;
            this.orderNr = orderNr;
            isPaid = false;
        }

        public Order(int orderNr, Visitor customer, ServicePoint shop) // new order created from vending machines
        {
            this.customer = customer;
            this.cashier = null;
            orderTime = DateTime.Now;
            orderLineItem = new Dictionary<EventItem, int>();
            this.orderNr = orderNr;
            isPaid = false;
            this.shop = shop;
        }

        // methods
        public abstract double GetTotal();

        public void AddItemToOrder(EventItem item, int quantity)
        {
            if (item.RemoveFromStock(quantity))
            {
                if (orderLineItem.ContainsKey(item))
                {
                    orderLineItem[item] += quantity;
                }
                else
                {
                    orderLineItem.Add(item, quantity);
                }
            }
            else
            {
                throw new LatitudeException("Not enough items in stock");
            }
        }

        public void RemoveItemFromOrder(EventItem item, int quantity)
        {
            if (orderLineItem.ContainsKey(item))
            {
                orderLineItem[item] -= quantity;
                item.AddToStock(quantity);
            }
            else
            {
                throw new LatitudeException("Item not in order");
            }
        }

        public virtual void PayOrder()
        {
            if (orderLineItem.Count != 0)
            {
                if (customer.Withdraw(GetTotal()))
                {
                    isPaid = true;
                }
                else
                {
                    foreach (EventItem item in orderLineItem.Keys)
                    {
                        item.AddToStock(orderLineItem[item]);
                    }
                    throw new LatitudeException("Balance not enough");
                }
            }
            else
            {
                throw new LatitudeException("Order is empty");
            }
        }
        
        public void CancelOrder()
        {
            if (orderLineItem != null)
            {
                foreach (EventItem item in orderLineItem.Keys)
                {
                    item.AddToStock(orderLineItem[item]);
                }
                orderLineItem = null;
            }
            else
            {
                throw new LatitudeException("Current order is empty");
            }
        }

        public virtual void PrintOrder()
        {
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream("../../../documents/orderPrinted/order_" + OrderNr + ".txt", FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);

                sw.WriteLine("         Latitude Music Festival          ");
                sw.WriteLine("------------------------------------------");
                sw.WriteLine("Shop id: " + shop.PointId);
                sw.WriteLine("Order nr.: " + OrderNr + " on " + orderTime.ToShortDateString() + " at " + orderTime.ToShortTimeString());
                if (cashier != null)
                {
                    sw.WriteLine("Cashier: " + cashier.FirstName + " " + cashier.LastName);
                }
                sw.WriteLine("Customer: " + customer.FirstName + " " + customer.LastName);
                sw.WriteLine("==========================================");
                if(this is SellingOrder)
                {
                    sw.WriteLine("Description                          Price");
                }
                else
                {
                    sw.WriteLine("Description                    Renting Fee");
                }
                sw.WriteLine("==========================================");
                foreach (KeyValuePair<EventItem, int> pair in orderLineItem)
                {
                    if (pair.Key is ItemForSale)
                    {
                        sw.WriteLine(string.Format("{0,2} x {1,-30} {2,6:n}", pair.Value, pair.Key.Name, pair.Value * ((ItemForSale)pair.Key).Price));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("{0,2} x {1,-30} {2,6:n}", pair.Value, pair.Key.Name, pair.Value * ((ItemForRent)pair.Key).RentingFee));
                    }
                }
                sw.WriteLine("");
                if(customer.TicketType == Ticket.VipAtGate || customer.TicketType == Ticket.VipOnline)
                {
                    sw.WriteLine(string.Format("*Discount for VIP ticket holder:   {0,7:n}", -0.25 * GetTotal())); // recalculate discount amount
                }
                sw.WriteLine("");
                sw.WriteLine("TOTAL TO PAY:                      €" + string.Format("{0,6:n}", GetTotal()));
                sw.WriteLine("");
                sw.WriteLine("-------------------------------------------");
                sw.WriteLine("Thank you for your visit! Have a nice day!");
                if (this is RentingOrder)
                {
                    sw.WriteLine("");
                    sw.WriteLine("Please return your borrowings before leaving the event");
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
