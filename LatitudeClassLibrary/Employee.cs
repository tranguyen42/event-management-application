using System;

namespace LatitudeClassLibrary
{
    public enum EmpLevel
    {
        Level1, Level2, Level3
    }
    
    public enum WorkPlaceForLevel3
    {
        EntranceCheck, Shop, Other, NonApplicable
    }
   
    public class Employee : Person
    {
        // fields
        private string appUsername;
        private string appPassword;
        private EmpLevel level;
        private WorkPlaceForLevel3 place;
        private ServicePoint shop;

        // properties
        public EmpLevel Level { get { return level; } }
        public WorkPlaceForLevel3 Place { get { return place; } }
        public string AppUsername { get { return appUsername; } }
        public string AppPassword { get { return appPassword; } }
        public int ShopId { get { return shop.PointId; } }
        public ServicePoint Shop { get { return shop; } }

        // constructors
        public Employee(int id, string first, string last, DateTime dob, Gender gender, string address, string email, string username, 
            string password, WorkPlaceForLevel3 place) : base(id, first, last, dob, gender, address, email)
        {
            appUsername = username;
            appPassword = password;
            this.level = EmpLevel.Level3;
            this.place = place;
            this.shop = null;
        }

        public Employee(int id, string first, string last, DateTime dob, Gender gender, string address, string email, string username,
           string password, ServicePoint shop) : base(id, first, last, dob, gender, address, email)
        {
            appUsername = username;
            appPassword = password;
            this.level = EmpLevel.Level3;
            this.place = WorkPlaceForLevel3.Shop;
            this.shop = shop;
        }

        public Employee(int id, string first, string last, DateTime dob, Gender gender, string address, string email, string username,
           string password, EmpLevel level) : base(id, first, last, dob, gender, address, email)
        {
            appUsername = username;
            appPassword = password;
            this.level = level;
            this.shop = null;
            this.place = WorkPlaceForLevel3.NonApplicable;
        }

       
        // methods
        public override string AsAString()
        {
            string myLevel = "";
            if (level == EmpLevel.Level1) { myLevel = "level 1"; }
            else if (level == EmpLevel.Level2) { myLevel = "level 2"; }
            else { myLevel = "level 3"; }

            string myPlace = "";
            if (place == WorkPlaceForLevel3.EntranceCheck) { myPlace = "at entrance"; }
            else if (place == WorkPlaceForLevel3.Shop) { myPlace = "at shop "; }
            else if (place == WorkPlaceForLevel3.Other) { myPlace = "at other place"; }
            else { myPlace = ""; }

            string temp = "Emp " + base.AsAString() + myLevel;
            if (level == EmpLevel.Level3)
            {
                temp += ", working " + myPlace;
                if(place == WorkPlaceForLevel3.Shop)
                {
                    temp += "id " + shop.PointId.ToString();
                }
            }

            return temp;
        }

        public void ChangeUsername(string newName)
        {
            appUsername = newName;
        }

        public void ChangePassword (string newPass)
        {
            appPassword = newPass;
        }

        public void ChangeEmpLevel(EmpLevel newLevel)
        {
            level = newLevel;
        }

        public void ChangeWorkingPlaceForEmpLevel3(WorkPlaceForLevel3 newPlace)
        {
            place = newPlace;      
        }

        public void ChangeShop(ServicePoint newShop)
        {
            shop = newShop;
        }

        public ServicePoint GetShop()
        {
            return shop;
        }
    }
}
