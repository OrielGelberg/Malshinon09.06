using Malshinon09._06.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Malshinon09._06.utils;



namespace Malshinon09._06.DAL
{

    internal static class PeopleDAL
    {
        public static Person GetById(int id)
        {
            string sql = $"SELECT * FROM People WHERE Id = {id};";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result is null ? null : Map(result);
        }

        public static Person GetBySecretCode(string code)
        {
            string sql = $"SELECT * FROM People WHERE SecretCode = '{code}';";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result is null ? null : Map(result);
        }

        public static Person GetByFullName(string name)
        {
            string sql = $"SELECT * FROM People WHERE FullName = '{name}';";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result is null ? null : Map(result);
        }

        public static Person Insert(string fullName, string secretCode)
        {
            string sql = $"INSERT INTO People (FullName, SecretCode) VALUES ('{fullName}', '{secretCode}');";
            DBConnection.Execute(sql);

            // Get the inserted person by code
            return GetBySecretCode(secretCode);
        }
        private static List<Person> GetAll()
        {
            string sql = "SELECT * FROM People;";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }
        //add person to the database
        public static Person AddPerson(string fullName, string secretCode = null)
        {
            // Check if the person already exists
            var existingPerson = GetByFullName(fullName);
            if (existingPerson != null)
            {
                return existingPerson; // Return the existing person
            }
            // Insert the new person
            return Insert(fullName, secretCode);
        }


        //Get all reports about a specific person
         





        private static Person Map(Dictionary<string, object> row)
        {
            return new Person
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = row["FullName"]?.ToString(),
                Secret_code = row["SecretCode"]?.ToString(),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }


    }
}// Compare this snippet from utils/DBConnection.cs: 

