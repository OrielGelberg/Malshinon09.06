using Malshinon09._06.DAL;
using Malshinon09._06.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malshinon09._06.models;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Principal;
using System.Net.Sockets;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.X509;

namespace Malshinon09._06
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to the Report Management System(Malshinon)!\n");
            while (true)
            {
                Menu();
                var choice = Console.ReadLine();
                while (choice != null)
                {
                    // Console.WriteLine(choice);
                    switch (choice)
                    {
                        case "1":
                            SubmitReport();
                            break;
                        case "2":
                            ImportCsv();
                            break;
                        case "3":
                            ShowSecretCodeByName();
                            break;
                        case "4":
                            AnalysisDashboard();
                            break;
                        case "5":
                            PrintAllPeople();
                            break;
                        case "6":
                            PrintallReports();
                            break;
                        case "7":
                            PrintLogToTXT();
                            Console.WriteLine("Exiting...");
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                    Menu();
                    choice = Console.ReadLine();

                }
            }
            //Report Submission Flow
            //○ Identify reporter and target(by name or secret code)
            //○ Input and submit report text
            //○ Automatically handle new people
            void SubmitReport()
            {
                Console.WriteLine("\n=== SUBMIT INTELLIGENCE REPORT ===");

                // Get reporter
                Console.Write("Enter your name or secret code: ");
                string reporterInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(reporterInput))
                {
                    Console.WriteLine("Reporter identification cannot be empty.\n");
                    return;
                }
                //     int reporterId = PeopleDAL.GetOrCreatePerson(reporterInput);
                // Get target
                Console.Write("Enter target's name or secret code: ");
                string targetInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(targetInput))
                {
                    Console.WriteLine("Target identification cannot be empty.\n");
                    return;
                }

                // Get report text
                Console.Write("Enter your intelligence report: ");
                string reportText = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(reportText))
                {
                    Console.WriteLine("Report text cannot be empty.\n");
                    return;
                }

                try
                {
                    int reporterId = PeopleDAL.GetOrCreatePerson(reporterInput);
                    int targetId = PeopleDAL.GetOrCreatePerson(targetInput);

                    if (reporterId == targetId)
                    {
                        Console.WriteLine("Warning: You cannot report on yourself!\n");
                        return;
                    }

                    var report = new Report
                    {
                        ReporterId = reporterId,
                        TargetId = targetId,
                        ReportText = reportText,
                        SubmittedAt = DateTime.Now
                    };

                    ReportsDAL.Insert(report);
                    AlertsDAL.CheckAndTriggerAlerts(targetId);

                    Console.WriteLine(" Report submitted successfully!\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to submit report: {ex.Message}\n");
                }



            }

            //            Secret Code Management
            //○ Show secret code of known individuals by full name
            //○ Generate and store code for new individuals

            void ShowSecretCodeByName()
            {
                Console.WriteLine(
                    "Enter the full name of the person to retrieve their secret code:");
                var fullName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    Console.WriteLine("Full name cannot be empty.\n");
                    return;
                }
                var person = PeopleDAL.GetByFullName(fullName);
                if (person == null)
                {
                    Console.WriteLine($"No person found with the name '{fullName}'.\n");
                    return;
                }
                Console.WriteLine($"Secret code for {fullName}: {person.Secret_code}\n");
                Logger.Log($"ShowSecretCodeByName: Retrieved secret code for {fullName}");
                Console.WriteLine(
                    $"Secret code for {fullName}: {person.Secret_code}\n");

            }
            //            Analysis Dashboard
            //○ Show list of potential recruits(based on thresholds)
            //○ Show list of dangerous targets
            //○ Show triggered alerts
            void AnalysisDashboard()
            {
                Console.WriteLine("\n=== ANALYSIS DASHBOARD ===");

                // Show potential recruits
                Console.WriteLine(" POTENTIAL RECRUITS (10+ reports, 100+ avg chars):");
                var recruits = PeopleDAL.GetPotentialRecruits();
                if (recruits.Any())
                {
                    foreach (var recruit in recruits)
                    {
                        var reports = ReportsDAL.GetByReporterId(recruit.Id);
                        double avgLength = reports.Any() ? reports.Average(r => r.ReportText.Length) : 0;
                        Console.WriteLine($"- {recruit.FullName} ({recruit.Secret_code}): {reports.Count} reports, {avgLength:F1} avg chars");
                    }
                }
                else
                {
                    Console.WriteLine("No potential recruits found.");
                }

                Console.WriteLine();

                // Show high-risk targets
                Console.WriteLine(" HIGH-RISK TARGETS (20+ mentions):");
                var highRiskTargets = PeopleDAL.GetHighRiskTargets();
                if (highRiskTargets.Any())
                {
                    foreach (var target in highRiskTargets)
                    {
                        int mentionCount = ReportsDAL.GetReportCountForTarget(target.Id);
                        Console.WriteLine($"- {target.FullName} ({target.Secret_code}): {mentionCount} mentions");
                    }
                }
                else
                {
                    Console.WriteLine("No high-risk targets found.");
                }

                Console.WriteLine();

                // Show recent alerts
                Console.WriteLine(" RECENT ALERTS:");
                var alerts = AlertsDAL.GetAll().Take(10);
                if (alerts.Any())
                {
                    foreach (var alert in alerts)
                    {
                        var target = PeopleDAL.GetById(alert.TargetId);
                        Console.WriteLine($"- [{alert.CreatedAt:MM/dd HH:mm}] {alert.AlertType}: {target?.FullName} - {alert.Reason}");
                    }
                }
                else
                {
                    Console.WriteLine("No alerts found.");
                }

                Console.WriteLine();
            }


            //            CSV Import
            //○ Load CSV data into the database
            //○ Validate structure and process rows safely
            void ImportCsv()
            {
                Console.Write("CSV file path: ");
                var path = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) { Console.WriteLine("File not found.\n"); return; }
                int count = 0;
                //  using var reader = new StreamReader(path);
                using (var reader = new StreamReader(path))
                {
                    string header = reader.ReadLine();

                    if (header == null)
                    {
                        Console.WriteLine("CSV is empty.\n");
                        return;
                    }

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(',');
                        if (parts.Length < 4) continue;

                        var reporter = parts[0];
                        var target = parts[1];
                        var text = parts[2];

                        if (!DateTime.TryParse(parts[3], null, System.Globalization.DateTimeStyles.AssumeLocal, out var ts))
                            continue;

                        if (string.IsNullOrWhiteSpace(reporter) || string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(text))
                            continue;

                        int reporterId = PeopleDAL.GetOrCreatePerson(reporter);
                        int targetId = PeopleDAL.GetOrCreatePerson(target);

                        ReportsDAL.InsertReport(reporterId, targetId, text, ts);
                        count++;
                        AlertsDAL.CheckAndTriggerAlerts(targetId);
                    }
                }

                Logger.Log($"CSVImport: Imported {count} reports from {path}");
                Console.WriteLine($"Imported {count} reports.\n");
            }
           void PrintAllPeople()
           {
            var people = PeopleDAL.GetAll();

            foreach (var person in people)
            {
                Console.WriteLine($"{person.FullName} ({person.Secret_code})");
            }
           }
            void PrintallReports()
            {
                var reports = ReportsDAL.GetAll();

                foreach (var report in reports)
                {
                    var reporter = PeopleDAL.GetById(report.ReporterId);
                    var target = PeopleDAL.GetById(report.TargetId);
                    Console.WriteLine($"[{report.SubmittedAt:MM/dd HH:mm}] {reporter?.FullName} -> {target?.FullName}: {report.ReportText}");
                }
            }
            void PrintLogToTXT()
            {
                string logContent = Logger.Read();
                string logFilePath = "log_output.txt";
                File.WriteAllText(logFilePath, logContent);
                Console.WriteLine($"Log written to {logFilePath}");
            }

            void Menu()
            {
                Console.WriteLine("1. Submit a new report");
                Console.WriteLine("2. import reports from CSV file");
                Console.WriteLine("3.Show Secret code by name");
                Console.WriteLine("4.Analysis dashborad");
                Console.WriteLine("5.Print all people");
                Console.WriteLine("6.Print all reports");
                Console.WriteLine("7.Exit");
                Console.Write("Choose an option: ");
            }


        }
          
    }
}
