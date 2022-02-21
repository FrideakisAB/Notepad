using System;
using System.Collections.Generic;
using System.Text;

namespace DZNotepad
{
    public class User
    {
        public bool IsChanged { get; set; } = false;

        private int userId;
        public int UserId
        {
            get => userId;
            set { userId = value; IsChanged = true; }
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set { lastName = value; IsChanged = true; }
        }

        private string name;
        public string Name
        {
            get => name;
            set { name = value; IsChanged = true; }
        }

        private string middleName;
        public string MiddleName
        {
            get => middleName;
            set { middleName = value; IsChanged = true; }
        }

        private string login;
        public string Login
        {
            get => login;
            set { login = value; IsChanged = true; }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; IsChanged = true; }
        }
    }
}
