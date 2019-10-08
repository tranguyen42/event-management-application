using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace LatitudeClassLibrary
{
    public class LogFile
    {
        // fields
        private int fileId;
        private DateTime createdTime;
        private DateTime endedTime;
        private int nrOfTransactions;
        private List<KeyValuePair<int, double>> eventAccountTopUp;

        // properties
        public int FileId { get { return fileId; } }
        public DateTime CreatedTime { get { return createdTime; } }
        public DateTime EndedTime { get { return endedTime; } }
        public int NrOfTransactions { get { return nrOfTransactions; } }
        public List<KeyValuePair<int, double>> EventAccountTopUp { get { return eventAccountTopUp; } }

        // constructor
        public LogFile(int id) // new LogFile from bank
        {
            fileId = id;
            eventAccountTopUp = new List<KeyValuePair<int, double>>();
        }
      
        // methods
        public bool ReadLogFile(string fileName)
        {
            FileStream fs = new FileStream("../../../documents/logFiles/" + fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            try
            {
                string info = sr.ReadLine();
                this.createdTime = DateTime.ParseExact(sr.ReadLine(), "yyyy/MM/dd/HH:mm:ss", CultureInfo.InvariantCulture);
                this.endedTime = DateTime.ParseExact(sr.ReadLine(), "yyyy/MM/dd/HH:mm:ss", CultureInfo.InvariantCulture);
                this.nrOfTransactions = Convert.ToInt32(sr.ReadLine());
                info = sr.ReadLine();
                while (info != null)
                {    
                    string[] splitedInfo = info.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int eventAccount = Convert.ToInt32(splitedInfo[0]);
                    double amount = Convert.ToDouble(splitedInfo[1]);
                    this.eventAccountTopUp.Add(new KeyValuePair<int, double>(eventAccount, amount));
                    info = sr.ReadLine();
                }
                return true;
                
            }
            catch(IOException)
            {
                throw new LatitudeException("Something wrong with log file");
            }
            catch (FormatException ex)
            {
                throw new LatitudeException("Log file has changed its format: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new LatitudeException("Something went wrong: " + ex.Message);
            }
            finally
            {
                if(sr != null)
                {
                    sr.Close();
                }
            }
        }

        public void MoveFile(string fileName)
        {
            // move to another folder after being read
            System.IO.File.Move("../../../documents/logFiles/" + fileName, "../../../documents/logFiles/oldLogFiles/" + fileName);
        }
    }
}
