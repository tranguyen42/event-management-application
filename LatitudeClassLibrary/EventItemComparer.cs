using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatitudeClassLibrary
{
    public class EventItemComparer: IComparer<EventItem>
    {
        public int Compare(EventItem a, EventItem b)
        {
            return b.QuantityInStock - a.QuantityInStock; // descending order
        }
    }
}
