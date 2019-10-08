using System.Collections.Generic;
using System;
using System.Linq;

namespace LatitudeClassLibrary
{
    public class CampingSpot
    {
        // fields
        private string spotId;
        private bool isAvailable;
        private List<Visitor> renters;
        private string spotCode;
        private int nrOfRenters;

        // properties
        public string SpotId { get { return spotId; } }
        public bool IsAvailable { get { return isAvailable; } }
        public List<Visitor> Renters { get { return renters; } }
        public string SpotCode { get { return spotCode; } }
        public int NrOfRenters { get { return nrOfRenters; } }

        // constructors
        /*
        public CampingSpot(string id, string spotCode, List<Visitor> renters)
        {
            this.spotId = id;
            this.renters = renters;
            this.spotCode = spotCode;
            CheckAvailable();
        }
        */
        public CampingSpot(string id, string spotCode)
        {
            this.spotId = id;
            this.renters = new List<Visitor>();
            this.spotCode = spotCode;
            isAvailable = true;
            nrOfRenters = 0;
        }

        // methods
        private void CheckAvailable()
        {
            if (renters.Count() != 0)
            {
                if (renters.Count() == 6)
                {
                    nrOfRenters = 6;
                    isAvailable = false;
                }
                else if (renters[0].TicketType == Ticket.GroupAtGate || renters[0].TicketType == Ticket.GroupOnline)
                {
                    nrOfRenters = 6;
                    isAvailable = false;        
                }
                else
                {
                    nrOfRenters = renters.Count();
                    isAvailable = true;
                }
            }
            else
            {
                nrOfRenters = 0;
                isAvailable = true;
            }
        }
        internal bool AddMoreRenter(Visitor v)
        {
           if (isAvailable)
           {
                if(v.TicketType == Ticket.GroupAtGate || v.TicketType == Ticket.GroupOnline)
                {
                    if(renters.Count == 0)
                    {
                        renters.Add(v);
                        CheckAvailable();
                        return true;
                    }
                }
                else
                {
                    renters.Add(v);
                    CheckAvailable();
                    return true;
                }  
            }
           return false;
        }

        public string AsAString()
        {
            string available;
            if (isAvailable)
            {
                available = "still available";
            }
            else
            {
                available = "fully rented";
                if(renters[0].TicketType == Ticket.GroupAtGate || renters[0].TicketType == Ticket.GroupOnline)
                {
                    available += " by a group";
                }
            }
            return String.Format("Camping spot id {0}: {1} renter(s), {2}", spotId, nrOfRenters, available);
        }

        public string ShowDetailRenters()
        {
            string temp = AsAString();
            if (renters.Count != 0)
            {
                foreach(Visitor v in renters)
                {
                    temp += "\n\t+ " + v.FirstName + " " + v.LastName + " - ticket nr.: " + v.TicketNr;
                }
            }
            return temp;
        }

        public double CalculateRevenueFromCampingSpot()
        {
            double amount = 0;
            if(renters.Count != 0)
            {   
                if(renters[0].TicketType == Ticket.GroupAtGate || renters[0].TicketType == Ticket.GroupOnline)
                {
                    amount = EventPriceList.CampFee + EventPriceList.CampFeePerGroup;
                }
                else
                {
                    amount = EventPriceList.CampFee + EventPriceList.CampFeePerPerson * renters.Count;
                }
            }
            return amount;
        }
    }
}
