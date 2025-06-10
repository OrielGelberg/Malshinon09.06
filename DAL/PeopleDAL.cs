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
            string sql = $"SELECT * FROM People WHERE SecretCode = '{Escape(code)}';";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result is null ? null : Map(result);
        }

        public static Person GetByFullName(string name)
        {
            string sql = $"SELECT * FROM People WHERE FullName = '{Escape(name)}';";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result is null ? null : Map(result);
        }

        public static Person Insert(string fullName, string secretCode)
        {
            string sql = $"INSERT INTO People (FullName, SecretCode) VALUES ('{Escape(fullName)}', '{Escape(secretCode)}');";
            DBConnection.Execute(sql);
            return GetBySecretCode(secretCode);
        }

        public static List<Person> GetAll()
        {
            string sql = "SELECT * FROM People;";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        public static string GenerateSecretCode()
        {
             
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        public static int GetOrCreatePerson(string identifier)
        {
            // Try to get by secret code first
            var person = GetBySecretCode(identifier);
            if (person != null)
                return person.Id;

            // Try to get by full name
            person = GetByFullName(identifier);
            if (person != null)
                return person.Id;

            // Create new person
            string secretCode = GenerateSecretCode();
            person = Insert(identifier, secretCode);
            Logger.Log($"Created new person: {identifier} with code: {secretCode}");
            return person.Id;
        }

        public static void UpdateReportCount(int personId)
        {
            string sql = $"UPDATE People SET ReportCount = ReportCount + 1 WHERE Id = {personId};";
            DBConnection.Execute(sql);
        }

        public static void UpdateMentionCount(int personId)
        {
            string sql = $"UPDATE People SET MentionCount = MentionCount + 1 WHERE Id = {personId};";
            DBConnection.Execute(sql);
        }

        public static List<Person> GetPotentialRecruits()
        {
            string sql = @"
                SELECT p.*, 
                       COALESCE(AVG(LENGTH(r.ReportText)), 0) as AvgReportLength,
                       COUNT(r.Id) as TotalReports
                FROM People p
                LEFT JOIN Reports r ON p.Id = r.ReporterId
                GROUP BY p.Id
                HAVING COUNT(r.Id) >= 10 AND COALESCE(AVG(LENGTH(r.ReportText)), 0) >= 100;";

            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        public static List<Person> GetHighRiskTargets()
        {
            string sql = @"
                SELECT p.*, COUNT(r.Id) as MentionCount
                FROM People p
                LEFT JOIN Reports r ON p.Id = r.TargetId
                GROUP BY p.Id
                HAVING COUNT(r.Id) >= 20;";

            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        private static Person Map(Dictionary<string, object> row)
        {
            return new Person
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = row["FullName"]?.ToString(),
                Secret_code = row["SecretCode"]?.ToString(),
                num_reports = row.ContainsKey("ReportCount") ? Convert.ToInt32(row["ReportCount"]) : 0,
                num_mentions = row.ContainsKey("MentionCount") ? Convert.ToInt32(row["MentionCount"]) : 0,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }

        private static string Escape(string text)
        {
            return text?.Replace("'", "''");
        }

    }
}
