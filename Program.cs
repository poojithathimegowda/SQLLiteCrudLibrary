using System;
using System.Collections.Generic;

namespace SQLiteCrudLibrary
{
    
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=C:\\poojitha\\NewSQLFile.db;Version=3;";
            var userCrud = new CrudOperations<User>(connectionString);

            // Create Table
            userCrud.CreateTable();

            // Insert a new record
            var user = new User { Name = "Alice", Age = 30 };
            userCrud.Insert(user);

            // Read all records
            var users = userCrud.ReadAll();
            foreach (var u in users)
            {
                Console.WriteLine($"ID: {u.Id}, Name: {u.Name}, Age: {u.Age}");
            }

            // Update a record
            user.Id = users[0].Id; // Assume the first user in the list
            user.Name = "Alice Updated";
            userCrud.Update(user, user.Id);

            // Delete a record
            userCrud.Delete(user.Id);
        }
    }

}




