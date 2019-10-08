using System;
using System.Collections.Generic;

namespace LatitudeClassLibrary
{
  
    public abstract class EventItem : IComparable<EventItem>
    {
        // fields
        private string name;
        private int sku;
        private string imgPath;
        private double costPerUnit;
        private int quantityInStock;
        private int quantityMin; // if quantiyInstock <= quantityMin, item needs restocking

        private static List<int> skuList = new List<int>();
        private static Random randomSku = new Random();


        // properties
        public string Name { get { return name; } }
        public int Sku { get { return sku; } }
        public string ImgPath { get { return imgPath; } }
        public double CostPerUnit { get { return costPerUnit; } }
        public int QuantityInStock { get { return quantityInStock; } }
        public int QuantityMin { get { return quantityMin; } }

        // constructor
        public EventItem(string name, int sku, string imgPath, double costPerUnit, int quantityInStock, int quantityMin)
        {// item that has sku 
            this.name = name;
            this.sku = sku;
            skuList.Add(sku);
            this.imgPath = imgPath;
            this.costPerUnit = costPerUnit;
            this.quantityInStock = quantityInStock;
            this.quantityMin = quantityMin;
        }

        public EventItem(string name, string imgPath, double costPerUnit, int quantityInStock, int quantityMin)
        {// item that has no sku
            this.name = name;
            this.sku = GenerateSku();
            this.imgPath = imgPath;
            this.costPerUnit = costPerUnit;
            this.quantityInStock = quantityInStock;
            this.quantityMin = quantityMin;
        }

        // method
        private int GenerateSku()
        {
            sku = randomSku.Next(1, 100);
            while (skuList.Contains(sku))
            {
                sku = randomSku.Next(1, 100);
            }
            skuList.Add(sku);
            return sku;
        }

        public void AddToStock(int quantity)
        {
            quantityInStock += quantity;
        }

        public bool RemoveFromStock(int quantity)
        {
            if(quantity <= quantityInStock)
            {
                quantityInStock -= quantity;
                return true;
            }
            return false;
        }

        public virtual string AsAString()
        {
            return string.Format("Sku {0} - {1}, {2} available unit(s), ", sku, name, quantityInStock);
        }

        // recognize 2 EventItems are the same
        public override int GetHashCode()
        {
            return sku.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            EventItem other = (EventItem)obj;
            return other != null && other.sku == this.sku;
        }

        // compare EvenItem by sku
        public int CompareTo(EventItem other)
        {
            return this.sku - other.sku;
        }
    }
}
