using System;

namespace PolesSU_Sports.Lib.Model
{
    public class Trainer
    {
        public string DocumentNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Qualification { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }

        // Полное ФИО
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}