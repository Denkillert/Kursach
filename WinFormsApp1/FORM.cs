using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            // Инициализация менеджеров
            RowingBaseAccounting.ServiceManager.InitializeServices();

            // Загрузка данных
            var clients = RowingBaseAccounting.FileManager.LoadFromFile("clients.txt", () => new RowingBaseAccounting.Client());
            foreach (var client in clients)
                RowingBaseAccounting.ClientManager.AddClient(client);

            var bookings = RowingBaseAccounting.FileManager.LoadFromFile("bookings.txt", () => new RowingBaseAccounting.Booking());
            RowingBaseAccounting.BookingManager.GetBookings().AddRange(bookings);

            // Загрузка сотрудников - ТОЛЬКО из файла, без дублирования
            var employees = RowingBaseAccounting.FileManager.LoadFromFile("employees.txt", () => new RowingBaseAccounting.Employee());
            foreach (var employee in employees)
            {
                // Проверяем, нет ли уже сотрудника с таким ID
                if (RowingBaseAccounting.EmployeeManager.GetEmployee(employee.Id) == null)
                {
                    RowingBaseAccounting.EmployeeManager.GetEmployees().Add(employee);
                }
            }

            // Если файл сотрудников пустой, создаем только администратора
            if (RowingBaseAccounting.EmployeeManager.GetEmployees().Count == 0)
            {
                var adminEmployee = new RowingBaseAccounting.Employee
                {
                    Id = 1,
                    Login = "admin",
                    Password = "admin123",
                    Name = "Администратор Системы",
                    Role = "Admin",
                    Phone = "8-029-692-70-05",
                    Email = "admin@rowingbase.by"
                };
                RowingBaseAccounting.EmployeeManager.GetEmployees().Add(adminEmployee);
                RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
            }
        }

        private void btnVisitor_Click(object sender, EventArgs e)
        {
            this.Hide();
            new VisitorLoginForm(this).ShowDialog();
            this.Show();
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            this.Hide();
            new StaffLoginForm(this).ShowDialog();
            this.Show();
        }

        private void btnVisitor_MouseEnter(object sender, EventArgs e)
        {
            btnVisitor.BackColor = Color.FromArgb(0, 133, 225);
        }

        private void btnVisitor_MouseLeave(object sender, EventArgs e)
        {
            btnVisitor.BackColor = Color.FromArgb(0, 153, 255);
        }

        private void btnStaff_MouseEnter(object sender, EventArgs e)
        {
            btnStaff.BackColor = Color.FromArgb(96, 195, 100);
        }

        private void btnStaff_MouseLeave(object sender, EventArgs e)
        {
            btnStaff.BackColor = Color.FromArgb(76, 175, 80);
        }
    }
}

