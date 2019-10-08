using System;
using System.Collections.Generic;
using System.IO;

namespace LatitudeClassLibrary
{
    
    public class SellingOrder: Order
    {
        // constructors
        public SellingOrder(int orderNr, Visitor customer, Employee cashier, Dictionary<EventItem, int> orderLineItem, ServicePoint shop) 
            : base(orderNr, customer, cashier, orderLineItem, shop) { }

        public SellingOrder(int orderNr, Visitor customer, ServicePoint shop) : base(orderNr, customer, shop) { }

        // methods
        public override double GetTotal()
        {
            double amount = 0;
            foreach (KeyValuePair<EventItem, int> pair in OrderLineItem)
            {
                amount += ((ItemForSale)pair.Key).Price * pair.Value;
            }
            if (Customer.TicketType == Ticket.VipAtGate || Customer.TicketType == Ticket.VipOnline)
            {
                return amount * 0.8;
            }
            return amount;
        }
    }
}
