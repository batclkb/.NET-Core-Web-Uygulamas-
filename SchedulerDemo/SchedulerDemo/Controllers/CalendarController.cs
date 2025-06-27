using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System.Text;
using SchedulerDemo.Services; 

namespace SchedulerDemo.Controllers
{

    public class Appointment
    {
        public int AppointmentId { get; set; } 
        public string Text { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EmployeeID { get; set; }
        public bool AllDay { get; set; }
    }
    
    public class ChangeDetail
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
    public class DetailedLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public int AppointmentId { get; set; }
        public string AppointmentText { get; set; }
        public string UstaName { get; set; }
        public List<ChangeDetail> Changes { get; set; }
    }
    
    public class CalendarController : Controller
    {
        private string GetUstaNameById(int employeeId)
        {
            switch (employeeId)
            {
                case 1: return "Ahmet Usta";
                case 2: return "Mehmet Usta";
                case 3: return "Ziya Usta";
                default: return "Bilinmeyen";
            }
        }
        private readonly FileLoggerService _logger;

        private static List<Appointment> _appointments = new List<Appointment>
        {
            new Appointment { AppointmentId = 1, Text = "Çatı Kaynağı", Description="Villanın çatı katındaki demirlerin acil kaynak yapılması gerekiyor.", EmployeeID = 1, StartDate = new DateTime(2024,6,3,10,0,0), EndDate = new DateTime(2024,6,3,12,0,0) },
            new Appointment { AppointmentId = 2, Text = "Matkap Tamiri", Description="Hilti marka matkap çalışmıyor, motor kontrol edilecek.", EmployeeID = 2, StartDate = new DateTime(2024,6,4,11,0,0), EndDate = new DateTime(2024,6,4,13,0,0) },
            new Appointment { AppointmentId = 3, Text = "Bahçe Kapısı", EmployeeID = 1, StartDate = new DateTime(2025,6,15,9,0,0), EndDate = new DateTime(2025,6,16,12,0,0) },
            new Appointment { AppointmentId = 4, Text = "Elektrik Bakımı", EmployeeID = 2, StartDate = new DateTime(2025,6,17,14,0,0), EndDate = new DateTime(2025,6,17,16,0,0) },
            new Appointment { AppointmentId = 5, Text = "Pencere Montajı", EmployeeID = 3, StartDate = new DateTime(2025,6,20,9,0,0), EndDate = new DateTime(2025,6,20,11,0,0) },
            new Appointment { AppointmentId = 6, Text = "Boya İşleri", EmployeeID = 1, StartDate = new DateTime(2025,6,22,10,0,0), EndDate = new DateTime(2025,6,22,13,0,0) },
            new Appointment { AppointmentId = 7, Text = "Tavan Tamiri", EmployeeID = 3, StartDate = new DateTime(2025,6,25,13,0,0), EndDate = new DateTime(2025,6,25,15,0,0) },
            new Appointment { AppointmentId = 8, Text = "Mutfak Dolabı", EmployeeID = 2, StartDate = new DateTime(2025,6,28,10,0,0), EndDate = new DateTime(2025,6,28,12,0,0) },
            new Appointment { AppointmentId = 9, Text = "Parke Döşeme", EmployeeID = 1, StartDate = new DateTime(2025,6,30,9,0,0), EndDate = new DateTime(2025,6,30,11,0,0) },
            new Appointment { AppointmentId = 10, Text = "Su Kaçağı", EmployeeID = 3, StartDate = new DateTime(2025,6,26,10,0,0), EndDate = new DateTime(2025,6,26,12,0,0) },
        };
        public CalendarController(FileLoggerService logger)
        {
            _logger = logger;
        }
        // GET metodu: Takvimi ve verileri yükler.
        [HttpGet]
        public IActionResult GetAppointments(DataSourceLoadOptions loadOptions)
        {
            var loadResult = DataSourceLoader.Load(_appointments, loadOptions);
            return Json(loadResult);
        }
        [HttpGet]
        public IActionResult GetAppointmentLogs(int appointmentId)
        {
            if (appointmentId <= 0) return BadRequest("Geçersiz Randevu ID.");

            var logEntries = new List<DetailedLogEntry>();
            try
            {
                string logFilePath = _logger.GetLogFilePath();
                if (System.IO.File.Exists(logFilePath))
                {
                    var allLines = System.IO.File.ReadAllLines(logFilePath);
                    foreach (var line in allLines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var log = JsonConvert.DeserializeObject<DetailedLogEntry>(line);
                        if (log.AppointmentId == appointmentId)
                        {
                            logEntries.Add(log);
                        }
                    }
                }
                return Ok(logEntries.OrderByDescending(l => l.Timestamp));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Loglar okunurken bir hata oluştu: " + ex.Message);
            }
        }

        // POST metodu: Yeni veri ekleme
        [HttpPost]
        public IActionResult PostAppointment(string values)
        {
            var newAppointment = new Appointment();
            JsonConvert.PopulateObject(values, newAppointment);
            if (newAppointment.AppointmentId == 0)
                newAppointment.AppointmentId = _appointments.Any() ? _appointments.Max(a => a.AppointmentId) + 1 : 1;
            _appointments.Add(newAppointment);

            var ustaName = GetUstaNameById(newAppointment.EmployeeID);
            var log = new DetailedLogEntry
            {
                Timestamp = DateTime.Now,
                Action = "OLUŞTURULDU",
                AppointmentId = newAppointment.AppointmentId,
                AppointmentText = $"Randevu '{newAppointment.Text}'",
                UstaName = ustaName
            };
            _logger.Log(log);

            return Ok(newAppointment);
        }

        [HttpPut]
        public IActionResult PutAppointment(int key, string values)
        {
            var appointment = _appointments.FirstOrDefault(a => a.AppointmentId == key);
            if (appointment == null) return NotFound();

            var updatedData = JsonConvert.DeserializeObject<Appointment>(values);
            var changes = new List<ChangeDetail>();

            if (appointment.Text != updatedData.Text)
                changes.Add(new ChangeDetail { FieldName = "Başlık", OldValue = appointment.Text, NewValue = updatedData.Text });
            if (appointment.Description != updatedData.Description)
                changes.Add(new ChangeDetail { FieldName = "Açıklama", OldValue = appointment.Description, NewValue = updatedData.Description });

            if (appointment.StartDate != updatedData.StartDate)
                changes.Add(new ChangeDetail { FieldName = "Başlangıç Tarihi", OldValue = appointment.StartDate.ToString("g"), NewValue = updatedData.StartDate.ToString("g") });

            if (appointment.EndDate != updatedData.EndDate)
                changes.Add(new ChangeDetail { FieldName = "Bitiş Tarihi", OldValue = appointment.EndDate.ToString("g"), NewValue = updatedData.EndDate.ToString("g") });

            JsonConvert.PopulateObject(values, appointment);

            var ustaName = GetUstaNameById(appointment.EmployeeID);
            var log = new DetailedLogEntry
            {
                Timestamp = DateTime.Now,
                Action = changes.Any() ? "GÜNCELLENDİ" : "BİLGİ",
                AppointmentId = appointment.AppointmentId,
                AppointmentText = $"Randevu '{appointment.Text}'",
                Changes = changes.Any() ? changes : new List<ChangeDetail> { new ChangeDetail { FieldName = "Durum", NewValue = "Değişiklik yapılmadan kapatıldı." } },
                UstaName = ustaName
            };
            _logger.Log(log);

            return Ok(appointment);
        }

        // DELETE metodu: Takvimde mevcut veriyi siler
        [HttpDelete]
        public IActionResult DeleteAppointment(int key)
        {
           var appointment = _appointments.FirstOrDefault(a => a.AppointmentId == key);
           if (appointment != null)
           {
                var ustaName = GetUstaNameById(appointment.EmployeeID);
                var log = new DetailedLogEntry
                {
                    Timestamp = DateTime.Now,
                    Action = "SİLİNDİ",
                    AppointmentId = appointment.AppointmentId,
                    AppointmentText = $"Randevu '{appointment.Text}'",
                    UstaName = ustaName 
                };
                _logger.Log(log);
                _appointments.Remove(appointment);
            }
           return Ok();
        }
        public IActionResult Overview()
        {
            return View();
        }
    }
}