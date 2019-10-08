using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;


namespace LatitudeClassLibrary
{
    public class DataForEvent
    {
        // fields
        private DataHelper myDataHelper = null;
        private Event myEvent;

        // properties
        public DataHelper MyDataHelper { get { return myDataHelper; } }
        public Event MyEvent { get { return myEvent; } }

        // constructor
        public DataForEvent(int id)
        {
            myDataHelper = new DataHelper();
            myEvent = new Event(id);
        }

        // private methods
        private void GetDateTimeCapacity()
        {
            string sql = "SELECT startDate, maxNrOfVisitors FROM event WHERE eventNr = " + myEvent.Id;
            try
            {
                myDataHelper.OpenConnection();

                MySqlDataReader reader = myDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    myEvent.EventTime = Convert.ToDateTime(reader["startDate"]);
                    myEvent.MaxNrVisitors = Convert.ToInt32(reader["maxNrOfVisitors"]);
                }

            }
            catch
            {
                throw new LatitudeException("Something wrong with the database");
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private List<Person> GetEmployees()
        {
            string sql = "SELECT * FROM employee";
            List<Person> temp = new List<Person>();
            try
            {
                MyDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    Gender g;
                    Person emp;
                    if (reader["gender"].ToString() == "F")
                    {
                        g = Gender.Female;
                    }
                    else
                    {
                        g = Gender.Male;
                    }
                    if (reader["level"].ToString() == "level_3")
                    {
                        WorkPlaceForLevel3 place;
                        if (reader["work_place"].ToString() == "shop")
                        {
                            place = WorkPlaceForLevel3.Shop;
                        }
                        else if (reader["work_place"].ToString() == "entrance check")
                        {
                            place = WorkPlaceForLevel3.EntranceCheck;
                        }
                        else
                        {
                            place = WorkPlaceForLevel3.Other;
                        }
                        emp = new Employee(Convert.ToInt32(reader["empNr"]), reader["first_name"].ToString(), reader["last_name"].ToString(),
                            Convert.ToDateTime(reader["dob"]), g, reader["address"].ToString(), reader["email"].ToString(), reader["userName"].ToString(),
                            reader["password"].ToString(), place);
                    }
                    else
                    {
                        EmpLevel lvl;
                        if (reader["level"].ToString() == "level_2")
                        {
                            lvl = EmpLevel.Level2;
                        }
                        else
                        {
                            lvl = EmpLevel.Level1;
                        }
                        emp = new Employee(Convert.ToInt32(reader["empNr"]), reader["first_name"].ToString(), reader["last_name"].ToString(),
                           Convert.ToDateTime(reader["dob"]), g, reader["address"].ToString(), reader["email"].ToString(), reader["userName"].ToString(),
                           reader["password"].ToString(), lvl);
                    }
                    temp.Add(emp);
                }
                return temp;
            }
            catch
            {
                return null;
            }
            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        private void AddShopIdForEmpLevel3()
        {
            string sql = "SELECT * FROM employee WHERE work_place = 'shop'";
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = MyDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    ((Employee)myEvent.GetPersonById(Convert.ToInt32(reader["empNr"]))).ChangeShop(myEvent.GetShopById(Convert.ToInt32(reader["SERVICE_POINT_id"])));
                }
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private List<ServicePoint> GetServicePoints()
        {
            string sql = "SELECT * FROM service_point";
            List<ServicePoint> temp = new List<ServicePoint>();
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    string type = reader["type"].ToString();
                    Person manager = myEvent.GetPersonById(Convert.ToInt32(reader["managerId"]));
                    if (type == "selling_shop")
                    {
                        temp.Add(new SellingShop(id, (Employee)manager));
                    }
                    else if (type == "renting_shop")
                    {
                        temp.Add(new LendingShop(id, (Employee)manager));
                    }
                    else
                    {
                        temp.Add(new VendingMachine(id, (Employee)manager));
                    }
                }
                return temp;
            }
            catch
            {
                return null;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }

        }

        private List<EventItem> GetEventItems()
        {
            string sql = "SELECT * FROM item left join item_for_rent on item.sku = item_for_rent.sku left join item_for_sale on item.sku = item_for_sale.sku";
            List<EventItem> temp = new List<EventItem>();
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                EventItem item;
                while (reader.Read())
                {
                    if (reader["type"].ToString() == "for rent")
                    {
                        item = new ItemForRent(reader["name"].ToString(), Convert.ToInt32(reader["sku"]), reader["imgPath"].ToString(),
                            Convert.ToDouble(reader["costPerUnit"]), Convert.ToInt32(reader["quantity_in_stock"]), Convert.ToInt32(reader["quantity_min"]),
                            Convert.ToDouble(reader["rentingFee"]));
                    }
                    else
                    {
                        SaleType st;
                        if (reader["forSaleType"].ToString() == "drink")
                        {
                            st = SaleType.Drink;
                        }
                        else if (reader["forSaleType"].ToString() == "food")
                        {
                            st = SaleType.Food;
                        }
                        else
                        {
                            st = SaleType.Souvenir;
                        }

                        bool isSuitable;
                        if (Convert.ToInt32(reader["isSuitableForVM"]) == 1)
                        {
                            isSuitable = true;
                        }
                        else
                        {
                            isSuitable = false;
                        }

                        item = new ItemForSale(reader["name"].ToString(), Convert.ToInt32(reader["sku"]), reader["imgPath"].ToString(),
                            Convert.ToDouble(reader["costPerUnit"]), Convert.ToInt32(reader["quantity_in_stock"]), Convert.ToInt32(reader["quantity_min"]),
                            Convert.ToDouble(reader["sellingPrice"]), st, isSuitable);

                    }
                    temp.Add(item);
                }
                return temp;
            }
            catch
            {
                return null;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private List<CampingSpot> GetCampingSpots()
        {
            string sql = "SELECT * FROM camping_spot";
            List<CampingSpot> temp = new List<CampingSpot>();
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    CampingSpot spot = new CampingSpot(reader["spotNr"].ToString(), reader["spotCode"].ToString());
                    temp.Add(spot);
                }
                return temp;
            }
            catch
            {
                return null;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private List<Person> GetVisitors()
        {
            string sql = "SELECT * FROM participant JOIN participant_event ON participant.participantNr = participant_event.participantNr WHERE eventNr = " + myEvent.Id;
            List<Person> temp = new List<Person>();
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);

                while (reader.Read())
                {
                    Person visitor;
                    Gender g;
                    if (reader["gender"].ToString() == "M")
                    {
                        g = Gender.Male;
                    }
                    else
                    {
                        g = Gender.Female;
                    }

                    Ticket ticket;
                    if (reader["ticketType"].ToString() == "vip online")
                    {
                        ticket = Ticket.VipOnline;
                    }
                    else if (reader["ticketType"].ToString() == "vip at gate")
                    {
                        ticket = Ticket.VipAtGate;
                    }
                    else if (reader["ticketType"].ToString() == "regular online")
                    {
                        ticket = Ticket.RegularOnline;
                    }
                    else if (reader["ticketType"].ToString() == "regular at gate")
                    {
                        ticket = Ticket.RegularAtGate;
                    }
                    else if (reader["ticketType"].ToString() == "group online")
                    {
                        ticket = Ticket.GroupOnline;
                    }
                    else
                    {
                        ticket = Ticket.GroupAtGate;
                    }

                    // create new visitor
                    if (reader["eventAccount"].ToString() == string.Empty)
                    {
                        visitor = new Visitor(Convert.ToInt32(reader["participantNr"]), reader["firstname"].ToString(), reader["lastName"].ToString(),
                            g, Convert.ToInt32(reader["ticketNr"]), ticket, Convert.ToDouble(reader["balance"]));
                        UpdateEventAccountVisitor((Visitor)visitor);
                    }
                    else
                    {
                        visitor = new Visitor(Convert.ToInt32(reader["participantNr"]), reader["firstname"].ToString(), reader["lastName"].ToString(),
                            g, Convert.ToInt32(reader["ticketNr"]), ticket, Convert.ToInt32(reader["eventAccount"]), Convert.ToDouble(reader["balance"]));
                    }
                    

                    // Add camping spot
                    if(reader["spotNr"].ToString() != string.Empty)
                    {
                        ((Visitor)visitor).ReserveCampingSpot(myEvent.GetSpotById(reader["spotNr"].ToString()));
                    }
                    temp.Add(visitor);
                }
                return temp;
            }
            catch
            {
                return null;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private void CheckInVisitors()
        {
            string sql = "SELECT * FROM participant_event where eventNr = " + myEvent.Id + " and isCheckedIn = 1";
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                Dictionary<int, string> rfidList = new Dictionary<int, string>();
                while (reader.Read())
                {
                    rfidList.Add(Convert.ToInt32(reader["ticketNr"]), reader["rfid"].ToString());
                }
                if (rfidList.Count != 0)
                {
                    foreach (KeyValuePair<int, string> pair in rfidList)
                    {
                        myEvent.GetVisitorByTicketNr(pair.Key).CheckIn();
                        myEvent.GetVisitorByTicketNr(pair.Key).AssignNewRfid(pair.Value);
                    }
                }
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private void UpdateEventAccountVisitor(Visitor v)
        {
            string newSql = "update participant_event set eventAccount = " + v.EventAccount + " where ticketNr = " + v.TicketNr;
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(newSql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private void AddBorrowings()
        {
            string sql = $"select ticketNr, sku, quantity from `order` join order_line_item on order.orderNr = order_line_item.orderNr where orderType = 'renting';";
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    myEvent.GetVisitorByTicketNr(Convert.ToInt32(reader["ticketNr"])).BorrowItem((ItemForRent)myEvent.GetItemBySku(Convert.ToInt32(reader["sku"])), Convert.ToInt32(reader["quantity"]));
                }
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        private void RemoveBorrowings()
        {
            string sql = $"select ticketNr, sku, quantity from returned_item";
            try
            {
                myDataHelper.OpenConnection();
                MySqlDataReader reader = myDataHelper.DataReader(sql);
                while (reader.Read())
                {
                    myEvent.GetVisitorByTicketNr(Convert.ToInt32(reader["ticketNr"])).ReturnItem((ItemForRent)myEvent.GetItemBySku(Convert.ToInt32(reader["sku"])), Convert.ToInt32(reader["quantity"]));
                }
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }
        
        // public methods
        public void SyncEventInfo()
        {
            GetDateTimeCapacity();

            // add employees
            List<Person> tempEmp = GetEmployees();
            if (tempEmp != null)
            {
                myEvent.AddPersonListToEvent(tempEmp);
            }

            // add camping spots
            myEvent.Spots = GetCampingSpots();

            // add service points
            List<ServicePoint> temp = GetServicePoints();
            if (temp != null)
            {
                foreach (ServicePoint p in temp)
                {
                    myEvent.AddServicePoint(p);
                }
            }

            // add shop id for emp level 3 working at shop
            AddShopIdForEmpLevel3();

            // add items to event
            List<EventItem> tempItems = GetEventItems();
            if (tempItems != null)
            {
                foreach (EventItem item in tempItems)
                {
                    myEvent.AddItemToEvent(item);
                }
            }

            // add visitors to event
            List<Person> tempVisitors = GetVisitors();
            if (tempVisitors != null)
            {
                myEvent.AddPersonListToEvent(tempVisitors);
            }
            CheckInVisitors();
            AddBorrowings();
            RemoveBorrowings();
        }

        public void UpdateCheckInVisitor(Visitor v)
        {
            string sql = $"update participant_event set RFID = '{v.Rfid}', isCheckedIn = 1 where ticketNr = {v.TicketNr}";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void UpdateCheckOutVisitor(Visitor v)
        {
            string sql = $"update participant_event set RFID = null, isCheckedIn = 0 where ticketNr = {v.TicketNr}";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void InsertNewVisitor(Visitor v)
        {
            string gen;
            if (v.Gender == Gender.Male)
            {
                gen = "M";
            }
            else
            {
                gen = "F";
            }

            string ticket;
            if(v.TicketType == Ticket.GroupAtGate)
            {
                ticket = "group at gate";
            }
            else if (v.TicketType == Ticket.RegularAtGate)
            {
                ticket = "regular at gate";
            }
            else
            {
                ticket = "vip at gate";
            }

            string sql = $"start transaction; insert into participant(participantNr, firstName, lastName, dob, gender) values ({v.Id}, '{v.FirstName}', '{v.LastName}', '{v.Dob.ToString("yyyy-MM-dd")}', '{gen}'); ";

            sql += $"insert into participant_event(ticketNr, eventAccount, ticketType, balance, participantNr, eventNr) values " +
                    $"({v.TicketNr}, {v.EventAccount}, '{ticket}', {v.Balance}, {v.Id}, {myEvent.Id}); ";
           
           
            if(v.Balance != 0)
            {
                sql += $"insert into `transaction` (source, dateTime, amount, ticketNr) values ('at gate', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {v.Balance}, {v.TicketNr}); ";
            }
            sql += "commit;";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }     
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void UpdateCampingSpotReservation(Visitor v)
        {
            string sql = $"start transaction; update participant_event set spotNr = '{v.ReservedSpot.SpotId}'";
            if(v.SpotCode != "")
            {
                sql += $", spotCode = '{v.SpotCode}'";
            }
            sql += $" where ticketNr = {v.TicketNr}; ";
            
            sql += $" update camping_spot set nrOfPersons = '{v.ReservedSpot.NrOfRenters}' where spotNr = '{v.ReservedSpot.SpotId}'; commit;";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
           
            catch
            {
                return;
            }
            
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void UpdateEmployeeInfo(Employee emp)
        {
            string level;
            if(emp.Level == EmpLevel.Level1)
            {
                level = "level_1";
            }
            else if(emp.Level == EmpLevel.Level2)
            {
                level = "level_2";
            }
            else
            {
                level = "level_3";
            }

            string place;
            if(emp.Place == WorkPlaceForLevel3.Shop)
            {
                place = "shop";
            }
            else if (emp.Place == WorkPlaceForLevel3.EntranceCheck)
            {
                place = "entrance check";
            }
            else if(emp.Place == WorkPlaceForLevel3.Other)
            {
                place = "other";
            }
            else
            {
                place = "non applicable";
            }

            string sql = $"update employee set level = '{level}', userName = '{emp.AppUsername}', password = '{emp.AppPassword}', address = '{emp.Address}', " +
                $"work_place = '{place}', email = '{emp.Email}'";
            if(emp.Shop != null)
            {
                sql += $", SERVICE_POINT_id = '{emp.ShopId}'";
            }
            else
            {
                sql += ", SERVICE_POINT_id = null";
            }
            sql += $" where empNr = {emp.Id}";

            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void InsertNewEmployee(Employee emp)
        {
            string gen;
            if (emp.Gender == Gender.Male)
            {
                gen = "M";
            }
            else
            {
                gen = "F";
            }

            string level;
            if (emp.Level == EmpLevel.Level1)
            {
                level = "level_1";
            }
            else if (emp.Level == EmpLevel.Level2)
            {
                level = "level_2";
            }
            else
            {
                level = "level_3";
            }

            string place;
            if (emp.Place == WorkPlaceForLevel3.Shop)
            {
                place = "shop";
            }
            else if (emp.Place == WorkPlaceForLevel3.EntranceCheck)
            {
                place = "entrance check";
            }
            else if (emp.Place == WorkPlaceForLevel3.Other)
            {
                place = "other";
            }
            else
            {
                place = "non applicable";
            }

            string sql = $"start transaction; insert into employee(empNr, first_name, last_name, dob, level, email, userName, password, gender, address, work_place) " +
                $"values ({emp.Id}, '{emp.FirstName}', '{emp.LastName}', '{emp.Dob.ToString("yyyy-MM-dd")}', '{level}', '{emp.Email}', '{emp.AppUsername}', '{emp.AppPassword}', " +
                $"'{gen}', '{emp.Address}', '{place}')";
            if(emp.Shop != null)
            {
                sql += $"; update employee set SERVICE_POINT_id = {emp.ShopId}";
            }
            else
            {
                sql += $"; update employee set SERVICE_POINT_id = null";
            }
            sql += $" where empNr = {emp.Id}; commit;";

            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void InsertNewLog(LogFile log)
        {
            string sql = $"start transaction; insert into atm_log_file(logNr, datetime_start, datetime_end, nrOfDeposits) values ({log.FileId}, '{log.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")}', " +
                $"'{log.EndedTime.ToString("yyyy-MM-dd HH:mm:ss")}', {log.NrOfTransactions}); ";
            foreach (KeyValuePair<int, double> pair in log.EventAccountTopUp)
            {
                sql += $"insert into transaction(source, dateTime, amount, logNr, ticketNr) values ('ATM', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {pair.Value}, {log.FileId}, {myEvent.GetVisitorByEventAccount(pair.Key).TicketNr}); ";
                sql += $"update participant_event set balance =  {myEvent.GetVisitorByEventAccount(pair.Key).Balance} where eventAccount = {pair.Key}; ";
            }
            sql += "commit;";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public int GetNextOrderNumber()
        {
            string sql = "select coalesce(max(orderNr), 99) from `order`"; // orderNr starts from 100
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToInt32(myDataHelper.ExecuteScalar(sql));
            }
           
            catch
            {
                return -1;
            }
            
            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public int GetNextLogFileId()
        {
            string sql = "select coalesce(max(logNr), 0) from atm_log_file"; // logNr starts from 1
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToInt32(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public int GetNextEmployeeNr()
        {
            string sql = "select coalesce(max(empNr), 199) from employee"; // EmpNr starts from 200
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToInt32(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public int GetNextVisitorNr()
        {
            string sql = "select coalesce(max(participantNr), 499) from participant"; // visitorNr starts from 499
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToInt32(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public void InsertNewOrder(Order newOrder)
        {
            string type;
            if(newOrder is SellingOrder)
            {
                type = "buying";
            }
            else
            {
                type = "renting";
            }

            string sql = "start transaction; insert into `order` (orderNr, orderTime, totalToPay, orderType, ticketNr, shopId) values " +
                $"({newOrder.OrderNr}, '{newOrder.OrderTime.ToString("yyyy-MM-dd HH:mm:ss")}', {newOrder.GetTotal()}, '{type}', {newOrder.Customer.TicketNr}, {newOrder.Shop.PointId}); ";

            for (int i = 0; i < newOrder.OrderLineItem.Count; i++)
            {
                sql += $"insert into order_line_item (lineNr, orderNr, sku, quantity) values ({i + 1}, {newOrder.OrderNr}, {newOrder.OrderLineItem.ElementAt(i).Key.Sku}, {newOrder.OrderLineItem.ElementAt(i).Value}); " ;
                sql += $"update item set quantity_in_stock = {newOrder.OrderLineItem.ElementAt(i).Key.QuantityInStock} where sku = {newOrder.OrderLineItem.ElementAt(i).Key.Sku}; ";
                if (newOrder is SellingOrder)
                {
                    if (newOrder.Cashier != null)
                    {
                        sql += $"update `order` set empNr = {newOrder.Cashier.Id} where orderNr = {newOrder.OrderNr}; ";
                    }
                    sql += $"insert into order_line_saleitem (lineNr, orderNr, price) values ({i + 1}, {newOrder.OrderNr}, {((ItemForSale)newOrder.OrderLineItem.ElementAt(i).Key).Price * newOrder.OrderLineItem.ElementAt(i).Value}); ";
                }
                else
                {
                    sql += $"insert into order_line_rentitem (lineNr, orderNr, fee) values ({i + 1}, {newOrder.OrderNr}, {((ItemForRent)newOrder.OrderLineItem.ElementAt(i).Key).RentingFee * newOrder.OrderLineItem.ElementAt(i).Value}); ";
                }
            }

            sql += $"update participant_event set balance = {newOrder.Customer.Balance} where ticketNr = {newOrder.Customer.TicketNr}; commit; ";

            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void InsertNewReturn(Visitor v, ItemForRent item, int quantity)
        {
            string sql = $"start transaction; insert into returned_item (ticketNr, sku, quantity, returnDate) values ({v.TicketNr}, {item.Sku}, {quantity}, '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'); ";
            sql += $"update item set quantity_in_stock = {item.QuantityInStock} where sku = {item.Sku}; commit;";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }
        
        public void InsertNewFine(Visitor v, ItemForRent item, int quantity)
        {
            string sql = $"insert into returned_item (ticketNr, sku, quantity, returnDate, paidFine) values ({v.TicketNr}, {item.Sku}, {quantity}, null, {item.CostPerUnit * quantity} )";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void UpdateStock(EventItem item)
        {
            string sql = $"start transaction; update item set quantity_in_stock = {item.QuantityInStock} where sku = {item.Sku}; ";
            if(item is ItemForRent)
            {
                sql += $"update item_for_rent set rentingFee = {((ItemForRent)item).RentingFee} where sku = {item.Sku}; ";
            }
            else
            {
                sql += $"update item_for_sale set sellingPrice = {((ItemForSale)item).Price} where sku = {item.Sku}; ";
            }
            sql += "commit; ";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public void InsertNewItem(EventItem item)
        {
            string type;
            if(item is ItemForRent)
            {
                type = "for rent";
            }
            else
            {
                type = "for sale";
            }
            string sql = $"start transaction; insert into item (sku, name, costPerUnit, quantity_in_stock, quantity_min, type, imgPath) values " +
                $"({item.Sku}, '{item.Name}', {item.CostPerUnit}, {item.QuantityInStock}, {item.QuantityMin}, '{type}', '{item.ImgPath}'); ";
            if(item is ItemForRent)
            {
                sql += $"insert into item_for_rent (sku, rentingFee) values ({item.Sku}, {((ItemForRent)item).RentingFee}); ";
            }
            else
            {
                string saleType;
                if (((ItemForSale)item).ItemType == SaleType.Drink)
                {
                    saleType = "drink";
                }
                else if (((ItemForSale)item).ItemType == SaleType.Food)
                {
                    saleType = "food";
                }
                else
                {
                    saleType = "souvenir";
                }

                int isSuitable;
                if (((ItemForSale)item).IsSuitableForVM)
                {
                    isSuitable = 1;
                }
                else
                {
                    isSuitable = 0;
                }
                sql += $"insert into item_for_sale (sku, forSaleType, sellingPrice, isSuitableForVM) values ({item.Sku}, '{saleType}', {((ItemForSale)item).Price}, {isSuitable}); ";
            }
            sql += "commit; ";
            try
            {
                myDataHelper.OpenConnection();
                myDataHelper.ExecuteQueries(sql);
            }
            catch
            {
                return;
            }
            finally
            {
                myDataHelper.CloseConnection();
            }
        }

        public double CalculateRevenueFromAServicePoint(ServicePoint point)
        {
            string sql = $"select coalesce(sum(totalToPay), 0) from `order` group by shopId having shopId = {point.PointId}";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateRevenueFromSellingShops()
        {
            string sql = $"select coalesce(sum(totalToPay), 0) from `order` join service_point on `order`.shopId = service_point.id group by type having type = 'selling_shop'; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateRevenueFromLendingShops()
        {
            string sql = $"select coalesce(sum(totalToPay), 0) from `order` join service_point on `order`.shopId = service_point.id group by type having type = 'renting_shop'; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateRevenueFromVendingMachines()
        {
            string sql = $"select coalesce(sum(totalToPay), 0) from `order` join service_point on `order`.shopId = service_point.id group by type having type = 'vending_machine'; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateRevenueFromFine()
        {
            string sql = $"select coalesce(sum(paidFine), 0) from returned_item; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateTotalTopUp(Visitor v)
        {
            string sql = $"select coalesce(sum(amount), 0) from transaction group by ticketNr having ticketNr = {v.TicketNr}; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateTotalTopUp()
        {
            string sql = $"select coalesce(sum(amount), 0) from transaction; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateTotalBuyingRenting(Visitor v)
        {
            string sql = $"select coalesce(sum(totalToPay), 0) from `order` group by ticketNr having ticketNr = {v.TicketNr}; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateTotalFine(Visitor v)
        {
            string sql = $"select coalesce(sum(paidFine), 0) from `returned_item` group by ticketNr having ticketNr = {v.TicketNr}; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public double CalculateTotalBalance()
        {
            string sql = $"select coalesce(sum(balance), 0) from participant_event group by eventNr having eventNr = {myEvent.Id}; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToDouble(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }

        public int GetQuantitySoldOrTimesRented(EventItem item)
        {
            string sql = $"select coalesce(sum(quantity), 0) from order_line_item group by sku having sku = {item.Sku}; ";
            try
            {
                MyDataHelper.OpenConnection();
                return Convert.ToInt32(myDataHelper.ExecuteScalar(sql));
            }

            catch
            {
                return -1;
            }

            finally
            {
                MyDataHelper.CloseConnection();
            }
        }
    }
}
