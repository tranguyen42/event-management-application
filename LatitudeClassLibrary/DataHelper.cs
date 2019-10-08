using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;


namespace LatitudeClassLibrary
{
    public class DataHelper
    {
        // fields
        private MySqlConnection connection;
        private string connectionInfo = "server=studmysql01.fhict.local;" +
                         "database=dbi399963;" +
                         "user id=dbi399963;" +
                         "password=1234567;" +
                         "connect timeout=30;" +
                         "SslMode=none";

        // constructor

        // methods
        public void OpenConnection()
        {
            connection = new MySqlConnection(connectionInfo);
            connection.Open();
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        public void ExecuteQueries(string myQuery)
        {
            MySqlCommand cmd = new MySqlCommand(myQuery, connection);
            cmd.ExecuteNonQuery();
        }

        public MySqlDataReader DataReader(string myQuery)
        {
            MySqlCommand cmd = new MySqlCommand(myQuery, connection);
            MySqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public object ExecuteScalar(string myQuery)
        {
            MySqlCommand cmd = new MySqlCommand(myQuery, connection);
            return cmd.ExecuteScalar();
        }
        
    }
}
