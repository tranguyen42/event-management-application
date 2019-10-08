using System;
using System.Collections.Generic;

namespace LatitudeClassLibrary
{
    [Serializable]
    public class RentingOrder : Order
    {
        // constructors
        public RentingOrder(int orderNr, Visitor customer, Employee cashier, Dictionary<EventItem, int> orderLineItem, ServicePoint shop) 
            : base(orderNr, customer, cashier, orderLineItem, shop) { }
        

        // methods
        public override double GetTotal()
        {
            double amount = 0;
            foreach(KeyValuePair<EventItem, int> pair in OrderLineItem)
            {
                amount += ((ItemForRent)pair.Key).RentingFee * pair.Value;
            }
            if (Customer.TicketType == Ticket.VipAtGate || Customer.TicketType == Ticket.VipOnline)
            {
                return amount * 0.8;
            }
            return amount;
        }

        public override void PayOrder()
        {
            base.PayOrder();
            if (IsPaid)
            {
                foreach (KeyValuePair<EventItem, int> pair in OrderLineItem)
                {
                     Customer.BorrowItem((ItemForRent)pair.Key, pair.Value);
                } 
            }
        }
    }
}
