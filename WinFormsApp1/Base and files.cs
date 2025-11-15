// PROGA.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace RowingBaseAccounting


{
    public class Employee : IFileEntity
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } // "Manager", "Analyst", "Admin"
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; } = true;

        public Employee()
        {
            HireDate = DateTime.Now;
        }

        public string ToFileString()
        {
            return $"{Id}|{Login}|{Password}|{Name}|{Role}|{Phone}|{Email}|{HireDate:dd.MM.yyyy}|{IsActive}";
        }

        public void FromFileString(string data)
        {
            var parts = data.Split('|');
            Id = int.Parse(parts[0]);
            Login = parts[1];
            Password = parts[2];
            Name = parts[3];
            Role = parts[4];
            Phone = parts[5];
            Email = parts[6];
            HireDate = DateTime.ParseExact(parts[7], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            IsActive = bool.Parse(parts[8]);
        }

        public bool HasPermission(string permission)
        {
            return Role switch
            {
                "Admin" => true,
                "Manager" => permission != "create_admin",
                "Analyst" => permission == "view_reports",
                _ => false
            };
        }
    }

    // Менеджер сотрудников
    public static class EmployeeManager
    {
        private static List<Employee> employees = new List<Employee>();

        // УБРАН статический конструктор, чтобы не создавать дубликатов

        public static void AddEmployee(Employee employee)
        {
            employee.Id = employees.Count > 0 ? employees.Max(e => e.Id) + 1 : 1;
            employees.Add(employee);
        }

        public static Employee Authenticate(string login, string password)
        {
            return employees.FirstOrDefault(e =>
                e.Login == login &&
                e.Password == password &&
                e.IsActive);
        }

        public static List<Employee> GetEmployees() => employees;

        public static Employee GetEmployee(int id) => employees.FirstOrDefault(e => e.Id == id);

        public static bool UpdateEmployee(Employee employee)
        {
            var existing = GetEmployee(employee.Id);
            if (existing != null)
            {
                existing.Name = employee.Name;
                existing.Role = employee.Role;
                existing.Phone = employee.Phone;
                existing.Email = employee.Email;
                existing.IsActive = employee.IsActive;
                return true;
            }
            return false;
        }

        public static bool ChangePassword(int employeeId, string newPassword)
        {
            var employee = GetEmployee(employeeId);
            if (employee != null)
            {
                employee.Password = newPassword;
                return true;
            }
            return false;
        }
    }


    // Класс базы
    public class RowingBase
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public RowingBase(int number, string name, string address, string phone, string email)
        {
            Number = number;
            Name = name;
            Address = address;
            Phone = phone;
            Email = email;
        }
    }

    public interface IFileEntity
    {
        string ToFileString();
        void FromFileString(string data);
    }

    // Менеджер для работы с файлами
    public static class FileManager
    {
        public static void SaveToFile<T>(List<T> items, string filename) where T : IFileEntity
        {
            var lines = items.Select(item => item.ToFileString()).ToList();
            File.WriteAllLines(filename, lines);
        }

        public static List<T> LoadFromFile<T>(string filename, Func<T> creator) where T : IFileEntity
        {
            var items = new List<T>();

            if (!File.Exists(filename)) return items;

            foreach (var line in File.ReadAllLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var item = creator();
                item.FromFileString(line);
                items.Add(item);
            }

            return items;
        }
    }
}
