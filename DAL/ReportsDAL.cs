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
    
internal static class ReportsDAL
   {


        public static void Insert(Report report)
        {
            string sql = $@"
            INSERT INTO Reports (ReporterId, TargetId, ReportText, SubmittedAt)
            VALUES ({report.ReporterId}, {report.TargetId}, '{Escape(report.ReportText)}', '{report.SubmittedAt:yyyy-MM-dd HH:mm:ss}');";

            DBConnection.Execute(sql);

            
           // PeopleDAL.UpdateReportCount(report.ReporterId);
           // PeopleDAL.UpdateMentionCount(report.TargetId);

            Logger.Log($"Report submitted: Reporter ID {report.ReporterId}  Target ID {report.TargetId}");
        }

        public static void InsertReport(int reporterId, int targetId, string reportText, DateTime submittedAt)
        {
            var report = new Report
            {
                ReporterId = reporterId,
                TargetId = targetId,
                ReportText = reportText,
                SubmittedAt = submittedAt
            };
            Insert(report);
        }

        public static List<Report> GetAll()
        {
            string sql = "SELECT * FROM Reports ORDER BY SubmittedAt DESC;";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        public static List<Report> GetByTargetId(int targetId)
        {
            string sql = $"SELECT * FROM Reports WHERE TargetId = {targetId} ORDER BY SubmittedAt DESC;";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        public static List<Report> GetByTargetInWindow(int targetId, DateTime from, DateTime to)
        {
            string sql = $@"
            SELECT * FROM Reports 
            WHERE TargetId = {targetId} 
            AND SubmittedAt BETWEEN '{from:yyyy-MM-dd HH:mm:ss}' AND '{to:yyyy-MM-dd HH:mm:ss}'
            ORDER BY SubmittedAt DESC;";

            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        public static List<Report> GetByReporterId(int reporterId)
        {
            string sql = $"SELECT * FROM Reports WHERE ReporterId = {reporterId} ORDER BY SubmittedAt DESC;";
            var results = DBConnection.Execute(sql);
            return results.Select(Map).ToList();
        }

        public static int GetReportCountForTarget(int targetId)
        {
            string sql = $"SELECT COUNT(*) as count FROM Reports WHERE TargetId = {targetId};";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result != null ? Convert.ToInt32(result["count"]) : 0;
        }

        public static double GetAverageReportLength(int reporterId)
        {
            string sql = $"SELECT AVG(LENGTH(ReportText)) as avgLength FROM Reports WHERE ReporterId = {reporterId};";
            var result = DBConnection.Execute(sql).FirstOrDefault();
            return result != null && result["avgLength"] != null ? Convert.ToDouble(result["avgLength"]) : 0;
        }
        
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
            return text?.Replace("'", "''");
        }
  
    
    }


}

