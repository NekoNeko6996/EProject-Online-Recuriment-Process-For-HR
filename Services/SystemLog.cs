using Newtonsoft.Json;
using Sem3EProjectOnlineCPFH.Models.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Services
{
    public class SystemLog
    {
        private static readonly string LogFilePath = HttpContext.Current.Server.MapPath("~/App_Data/Log/UserActivityLog.json");
        private const int MaxLogEntries = 1000;
        private static readonly object _lock = new object(); // Dùng để tránh xung đột ghi file

        public static void WriteLog(string user, string action, string controller, string ip, string isSuccess, string message = "")
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    User = string.IsNullOrEmpty(user) ? "Guest" : user,
                    Action = action,
                    Controller = controller,
                    IPAddress = ip,
                    IsSuccess = isSuccess,
                    Message = message
                };

                lock (_lock) // Tránh lỗi khi nhiều request ghi log cùng lúc
                {
                    List<LogEntry> logs = new List<LogEntry>();

                    if (File.Exists(LogFilePath))
                    {
                        string existingJson = File.ReadAllText(LogFilePath);
                        if (!string.IsNullOrWhiteSpace(existingJson))
                        {
                            logs = JsonConvert.DeserializeObject<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                        }
                    }

                    logs.Add(logEntry);

                    if (logs.Count > MaxLogEntries)
                    {
                        logs = logs.Skip(logs.Count - MaxLogEntries).ToList();
                    }

                    File.WriteAllText(LogFilePath, JsonConvert.SerializeObject(logs, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error writing log: " + ex.Message);
            }
        }

        public static List<LogEntry> ReadLog()
        {
            List<LogEntry> logs = new List<LogEntry>();
            try
            {
                if (File.Exists(LogFilePath))
                {
                    string existingJson = File.ReadAllText(LogFilePath);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        logs = JsonConvert.DeserializeObject<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error reading log: " + ex.Message);
            }
            return logs;
        }

        public static List<LogEntry> ReadLogByDate(string date)
        {
            try
            {
                var logs = ReadLog();
                return logs.Where(log => log.Timestamp.StartsWith(date)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error filtering log: " + ex.Message);
                return new List<LogEntry>();
            }
        }
    }
}
