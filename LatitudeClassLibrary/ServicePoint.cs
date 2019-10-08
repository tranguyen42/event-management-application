using System.Collections.Generic;
using System;

namespace LatitudeClassLibrary
{
    public abstract class ServicePoint
    {
        // fields
        private int pointId;
        private List<Order> orders;
        private Employee manager;

        // property
        public int PointId { get { return pointId; } }
        public Employee Manager { get { return manager; } }

        // constructors
        public ServicePoint(int id, Employee manager)
        {
            this.pointId = id;
            orders = new List<Order>();
            if (manager.Level == EmpLevel.Level2)
            {
                this.manager = manager;
            }
            else
            {
                manager = null;
            }
        }
    }
}
