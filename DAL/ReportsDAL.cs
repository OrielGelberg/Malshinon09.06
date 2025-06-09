using Malshinon09._06.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using Malshinon09._06.utils;

namespace Malshinon09._06.DAL
{
    
internal static class ReportDAL
    {
        // הוספת דיווח חדש
        public static void Insert(Report report)
        {
            string sql = $@"
            INSERT INTO Reports (ReporterId, TargetId, ReportText, SubmittedAt)
            VALUES ({report.ReporterId}, {report.TargetId}, '{Escape(report.ReportText)}', '{report.SubmittedAt:yyyy-MM-dd HH:mm:ss}');
        ";

            DBConnection.Execute(sql);
        }

        // שליפת כל הדיווחים
        public static List<Report> GetAll()
        {
            string sql = "SELECT * FROM Reports;";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        // שליפת דיווחים לפי מזהה מטרה
        public static List<Report> GetByTargetId(int targetId)
        {
            string sql = $"SELECT * FROM Reports WHERE TargetId = {targetId};";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        // שליפת דיווחים בטווח זמן
        public static List<Report> GetByTargetInWindow(int targetId, DateTime from, DateTime to)
        {
            string sql = $@"
            SELECT * FROM Reports 
            WHERE TargetId = {targetId} 
            AND SubmittedAt BETWEEN '{from:yyyy-MM-dd HH:mm:ss}' AND '{to:yyyy-MM-dd HH:mm:ss}';
        ";

            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        // Retrieve all reports by a specific reporter
        public static List<Report> GetByReporterId(int reporterId)
        {
            string sql = $"SELECT * FROM Reports WHERE ReporterId = {reporterId};";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        // ממיר תוצאה ממסד נתונים לאובייקט
        private static Report Map(Dictionary<string, object> row)
        {
            return new Report
            {
                Id = Convert.ToInt32(row["Id"]),
                ReporterId = Convert.ToInt32(row["ReporterId"]),
                TargetId = Convert.ToInt32(row["TargetId"]),
                ReportText = row["ReportText"]?.ToString(),
                SubmittedAt = Convert.ToDateTime(row["SubmittedAt"])
            };
        }

        private static string Escape(string text)
        {
            return text?.Replace("'", "''"); // הגנה בסיסית מפני שגיאת SQL
        }
    }


}

