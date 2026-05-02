using System;

namespace PolesSU_Sports.Lib.Model
{
    public class Attendance
    {
        public int AttendanceID { get; set; }
        public string StudentCardNumber { get; set; }
        public string StudentName { get; set; }  
        public int ScheduleID { get; set; }
        public DateTime VisitDate { get; set; }
        public bool Status { get; set; }
        public string Notes { get; set; }
        public string SectionName { get; set; }  

        // Статус текстом
        public string StatusText => Status ? "Присутствовал" : "Отсутствовал";
    }
}