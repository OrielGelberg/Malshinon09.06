using Malshinon09._06.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malshinon09._06.models;


namespace Malshinon09._06.DAL
{
    internal class AlertsDAL
    {
        
            public static void InsertAlert(int targetId, string alertType, string reason, string timeWindow = null)
            {
                string sql = $@"
            INSERT INTO Alerts (TargetId, AlertType, Reason, TimeWindow)
            VALUES ({targetId}, '{Escape(alertType)}', '{Escape(reason)}', '{Escape(timeWindow ?? "")}');";

                DBConnection.Execute(sql);
                Logger.Log($"Alert created: {alertType} for Target ID {targetId} - {reason}");
            }

            public static List<Alert> GetAll()
            {
                string sql = "SELECT * FROM Alerts ORDER BY CreatedAt DESC;";
                var results = DBConnection.Execute(sql);
                return results.Select(Map).ToList();
            }

            public static List<Alert> GetByTargetId(int targetId)
            {
                string sql = $"SELECT * FROM Alerts WHERE TargetId = {targetId} ORDER BY CreatedAt DESC;";
                var results = DBConnection.Execute(sql);
                return results.Select(Map).ToList();
            }

            public static void CheckAndTriggerAlerts(int targetId)
            {
                // Check for high-risk target (20+ reports)
                CheckHighRiskTarget(targetId);

                // Check for burst reports (3+ in 15 minutes)
                CheckBurstReports(targetId);
            }

            private static void CheckHighRiskTarget(int targetId)
            {
                int reportCount = ReportsDAL.GetReportCountForTarget(targetId);

                if (reportCount >= 20)
                {
                    // Check if we already have a high-risk alert for this target
                    string checkSql = $"SELECT COUNT(*) as count FROM Alerts WHERE TargetId = {targetId} AND AlertType = 'HIGH_RISK';";
                    var result = DBConnection.Execute(checkSql).FirstOrDefault();
                    int existingAlerts = result != null ? Convert.ToInt32(result["count"]) : 0;

                    if (existingAlerts == 0)
                    {
                        var person = PeopleDAL.GetById(targetId);
                        string reason = $"Target '{person?.FullName}' has been mentioned in {reportCount} reports (threshold: 20)";
                        InsertAlert(targetId, "HIGH_RISK", reason);
                    }
                }
            }

            private static void CheckBurstReports(int targetId)
            {
                DateTime now = DateTime.Now;
                DateTime fifteenMinutesAgo = now.AddMinutes(-15);

                var recentReports = ReportsDAL.GetByTargetInWindow(targetId, fifteenMinutesAgo, now);

                if (recentReports.Count >= 3)
                {
                    var person = PeopleDAL.GetById(targetId);
                    string timeWindow = $"{fifteenMinutesAgo:HH:mm} - {now:HH:mm}";
                    string reason = $"Target '{person?.FullName}' received {recentReports.Count} reports in 15-minute window";
                    InsertAlert(targetId, "BURST_REPORTS", reason, timeWindow);
                }
            }

            private static Alert Map(Dictionary<string, object> row)
            {
                return new Alert
                {
                    Id = Convert.ToInt32(row["Id"]),
                    TargetId = Convert.ToInt32(row["TargetId"]),
                    AlertType = row["AlertType"]?.ToString(),
                    Reason = row["Reason"]?.ToString(),
                    TimeWindow = row["TimeWindow"]?.ToString(),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                };
            }

            private static string Escape(string text)
            {
                return text?.Replace("'", "''");
            }
        }

    
}
