using System;

namespace PolesSU_Sports.Shared.Model
{
    public class Student
    {
        public string StudentCardNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int FacultyID { get; set; }
        public string FacultyName { get; set; }  // Для отображения
        public string GroupName { get; set; }
        public int Course { get; set; }
        public DateTime EnrollmentDate { get; set; }

        // Полное ФИО
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}