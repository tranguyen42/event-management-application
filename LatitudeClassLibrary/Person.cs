using System;
using System.Collections.Generic;

namespace LatitudeClassLibrary
{
    public abstract class Person
    {
        // fields
        private int id;
        private string firstName;
        private string lastName;
        private DateTime dob;
        private Gender gender;
        private string address;
        private string email;
        private int age;
        private static List<int> issuedId = new List<int>();
        private static Random randomId = new Random();

        // properties
        public int Id { get { return id; } }
        public int Age { get { return (int)Math.Floor((DateTime.Now - dob).TotalDays /365); } }
        public string FirstName { get { return firstName; } }
        public string LastName { get { return lastName; } }
        public DateTime Dob { get { return dob; } }
        public string Address { get { return address; } }
        public string Email { get { return email; } }
        public Gender Gender { get { return gender; } }

        // constructors
        public Person(int id, string first, string last, DateTime dob, Gender gender, string address, string email)
        {
            this.id = id;
            issuedId.Add(id);
            this.firstName = first;
            this.lastName = last;
            this.dob = dob;
            this.gender = gender;
            this.address = address;
            this.email = email;
        }

        public Person(int id, string first, string last, DateTime dob, Gender gender)
        {
            this.id = id;
            issuedId.Add(id);
            this.firstName = first;
            this.lastName = last;
            this.dob = dob;
            this.gender = gender;
            this.address = "";
            this.email = "";
        }

        public Person(int id, string first, string last, Gender gender)
        {
            this.id = id;
            issuedId.Add(id);
            this.firstName = first;
            this.lastName = last;
            this.gender = gender;
            this.address = "";
            this.email = "";
        }

        // methods

        public virtual string AsAString()
        {
            return string.Format("Id: {0} - {1} {2}, ", id, firstName, lastName);
        }

        public void ChangeAddress(string newAddress)
        {
            address = newAddress;
        }

        public void ChangeEmail(string newEmail)
        {
            email = newEmail;
        }
    }
}
