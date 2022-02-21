using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DZNotepad
{
    public class Entitys
    {
        private List<User> users;
        public List<User> Users
        {
            get
            {
                if (users == null)
                    loadUsersTable();

                return users;
            }
        }

        /// <summary>
        /// Производит выгрузку в БД всех сущностей
        /// </summary>
        public void SaveChanges()
        {
            DBContext.Command("DELETE FROM users");

            foreach (User user in users)
                DBContext.CommandReader($"INSERT INTO users VALUES({user.UserId}, '{user.LastName}', '{user.Name}', '{user.MiddleName}', '{user.Login}', '{user.Password}');");
        }

        private void loadUsersTable()
        {
            List<User> preUsers = new List<User>();

            SqliteDataReader reader = DBContext.CommandReader("SELECT * FROM users");

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    User user = new User { UserId = reader.GetInt32(0), LastName = reader.GetString(1),
                        Name = reader.GetString(2), MiddleName = reader.GetString(3),
                        Login = reader.GetString(4), Password = reader.GetString(5) };

                    user.IsChanged = false;

                    preUsers.Add(user);
                }
            }

            users = preUsers;
        }
    }
}
