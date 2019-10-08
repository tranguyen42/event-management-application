using System;

namespace LatitudeClassLibrary
{
   
    public class ItemForRent : EventItem
    {
        // fields
        private double rentingFee;

        // properties
        public double RentingFee { get { return rentingFee; } }

        // constructor

        public ItemForRent(string name, int sku, string imgPath, double costPerUnit, int quantityInStock, int quantityMin, double rentingFee) 
            : base(name, sku, imgPath, costPerUnit, quantityInStock, quantityMin)
        {
            this.rentingFee = rentingFee;
        }
        public ItemForRent(string name, string imgPath, double costPerUnit, int quantityInStock, int quantityMin, double rentingFee) 
            : base(name, imgPath, costPerUnit, quantityInStock, quantityMin)
        {
            this.rentingFee = rentingFee;
        }

        // methods
        public override string AsAString()
        {
            return base.AsAString() + string.Format("\n\t+ renting fee: €{0:0.00}/ unit", rentingFee);
        }

        public void SetNewRentingFee(double amount)
        {
            rentingFee = amount;
        }
    }
}
