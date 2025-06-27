using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için gerekli
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SchedulerDemo.Controllers;
using System;
using System.IO;

namespace SchedulerDemo.Services
{
    public class FileLoggerService
    {
        private readonly string _logFilePath;
        public FileLoggerService(IWebHostEnvironment env)
        {
            string logDir = Path.Combine(env.ContentRootPath, "Logs");
            Directory.CreateDirectory(logDir);
             _logFilePath = Path.Combine(logDir, "activity_log.txt");
        }
        public string GetLogFilePath()
        {
            return _logFilePath;
        }
        public void Log(DetailedLogEntry logEntry)
        {
            try
            {
                string jsonLog = JsonConvert.SerializeObject(logEntry);
                File.AppendAllText(_logFilePath, jsonLog + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not write to log file: {ex.Message}");
            }
        }
    }
}