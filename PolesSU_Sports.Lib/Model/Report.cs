using System;

namespace PolesSU_Sports.Lib.Model
{
    public class DashboardStats
    {
        public int FacultiesCount { get; set; }
        public int TrainersCount { get; set; }
        public int StudentsCount { get; set; }
        public int SectionsCount { get; set; }
        public int AttendanceMonthCount { get; set; }
    }

    public class FacultyStats
    {
        public string FacultyName { get; set; }
        public int StudentsCount { get; set; }
        public string Groups { get; set; }
    }

    public class TrainerStats
    {
        public string TrainerID { get; set; }
        public string TrainerName { get; set; }
        public string Qualification { get; set; }
        public int SectionsCount { get; set; }
        public string Sections { get; set; }
    }
}