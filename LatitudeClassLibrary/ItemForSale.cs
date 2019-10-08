using System;

namespace LatitudeClassLibrary
{ 
   
    public enum SaleType
    {
        Drink, Food, Souvenir
    }
    [Serializable]
    public class ItemForSale : EventItem
    {
       
        // fields
        private double price;
        private SaleType itemType;
        private bool isSuitableForVM;

        // properties
        public double Price { get { return price; } }
        public SaleType ItemType { get { return itemType; } }
        public bool IsSuitableForVM { get { return isSuitableForVM; } }

        // constructor

        public ItemForSale(string name, int sku, string imgPath, double costPerUnit, int quantityInStock, int quantityMin,
            double price, SaleType itemType, bool isSuitable) : base(name, sku, imgPath, costPerUnit, quantityInStock, quantityMin)
        {
            this.price = price;
            this.itemType = itemType;
            this.isSuitableForVM = isSuitable;
        }

        public ItemForSale(string name, string imgPath, double costPerUnit, int quantityInStock, int quantityMin,
           double price, SaleType itemType, bool isSuitable) : base(name, imgPath, costPerUnit, quantityInStock, quantityMin)
        {
            this.price = price;
            this.itemType = itemType;
            this.isSuitableForVM = isSuitable;
        }

        // methods
        public override string AsAString()
        {
            string type = "";
            if (itemType == SaleType.Drink) { type = "drink"; }
            else if (itemType == SaleType.Food) { type = "food"; }
            else { type = "souvenir"; }
            return base.AsAString() + string.Format("\n\t+ type: {0}, selling price: €{1:.00}/ unit", type, price);
        }

        public void SetNewPrice(double amount)
        {
            price = amount;
        }
    }
}
