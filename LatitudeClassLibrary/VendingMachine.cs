using System.Collections.Generic;
using System;

namespace LatitudeClassLibrary
{
    
    public class VendingMachine : ServicePoint
    {
        // constructors
        public VendingMachine(int id, Employee manager) : base(id, manager) { }
    }
}
