using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DBConnection _dbConnection;

        public IndexModel(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // ✅ Поля для ВХОДА (универсальные: студенты + тренеры)
        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public string Password { get; set; }

        // ✅ Поля для РЕГИСТРАЦИИ (только студенты)
        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public string StudentCardNumber { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string MiddleName { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        [BindProperty]
        public int FacultyID { get; set; }

        [BindProperty]
        public string GroupName { get; set; }

        [BindProperty]
        public int Course { get; set; }

        [BindProperty]
        public bool IsRegisterMode { get; set; } = false;

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public List<Faculty> Faculties { get; set; }

        public IActionResult OnGet()
        {
            // ✅ Проверка авторизации
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(role))
            {
                return RedirectToPage(GetRedirectPage(role));
            }

            // ✅ Загружаем факультеты для формы регистрации
            Faculties = _dbConnection.GetFaculties();
            return Page();
        }

        // ✅ ВХОД (для студентов и тренеров)
        public IActionResult OnPostLogin()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "⚠️ Введите логин и пароль";
                Faculties = _dbConnection.GetFaculties();
                return Page();
            }

            try
            {
                string passwordHash = HashPassword(Password);

                // ✅ Аутентификация через библиотеку
                var account = _dbConnection.Authenticate(Login, passwordHash);

                if (account != null && account.IsActive)
                {
                    // ✅ Сохраняем в сессии
                    HttpContext.Session.SetString("UserLogin", account.Login);
                    HttpContext.Session.SetString("UserRole", account.Role.ToString());
                    HttpContext.Session.SetInt32("UserID", account.AccountID);

                    // ✅ Обновляем LastLogin
                    _dbConnection.UpdateLastLogin(account.AccountID);

                    // ✅ Перенаправляем по роли
                    return RedirectToPage(GetRedirectPage(account.Role.ToString()));
                }

                ErrorMessage = "❌ Неверный логин или пароль";
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Ошибка: " + ex.Message;
            }

            Faculties = _dbConnection.GetFaculties();
            return Page();
        }

        // ✅ РЕГИСТРАЦИЯ (только для студентов)
        public IActionResult OnPostRegister()
        {
            // ✅ Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(StudentCardNumber) ||
                string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName) ||
                FacultyID == 0 || string.IsNullOrWhiteSpace(GroupName))
            {
                ErrorMessage = "⚠️ Заполните все обязательные поля";
                IsRegisterMode = true;
                Faculties = _dbConnection.GetFaculties();
                return Page();
            }

            // ✅ Валидация пароля
            if (Password.Length < 6)
            {
                ErrorMessage = "❌ Пароль должен быть не менее 6 символов";
                IsRegisterMode = true;
                Faculties = _dbConnection.GetFaculties();
                return Page();
            }

            // ✅ Проверка совпадения паролей
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "❌ Пароли не совпадают";
                IsRegisterMode = true;
                Faculties = _dbConnection.GetFaculties();
                return Page();
            }

            // ✅ Валидация номера билета (7 цифр: ГГММXXX)
            if (!IsValidStudentCardNumber(StudentCardNumber))
            {
                ErrorMessage = "❌ Неверный формат номера билета. Пример: 2409001 (24-09-001)";
                IsRegisterMode = true;
                Faculties = _dbConnection.GetFaculties();
                return Page();
            }

            try
            {
                // ✅ Проверяем, нет ли уже студента с таким билетом
                var existingStudent = _dbConnection.ExecuteQuery(
                    "SELECT StudentCardNumber FROM Students WHERE StudentCardNumber = @CardNumber",
                    new[] { new SqlParameter("@CardNumber", StudentCardNumber) });

                if (existingStudent.Rows.Count > 0)
                {
                    ErrorMessage = "❌ Студент с таким номером билета уже существует";
                    IsRegisterMode = true;
                    Faculties = _dbConnection.GetFaculties();
                    return Page();
                }

                // ✅ Проверяем, нет ли уже аккаунта с таким логином
                var existingAccount = _dbConnection.ExecuteQuery(
                    "SELECT AccountID FROM Accounts WHERE Login = @Login",
                    new[] { new SqlParameter("@Login", StudentCardNumber) });

                if (existingAccount.Rows.Count > 0)
                {
                    ErrorMessage = "❌ На этот номер билета уже зарегистрирован аккаунт";
                    IsRegisterMode = true;
                    Faculties = _dbConnection.GetFaculties();
                    return Page();
                }

                // ✅ ТРАНЗАКЦИЯ: создаём Студента и Аккаунт
                using (var connection = _dbConnection.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Создаём студента
                            _dbConnection.ExecuteCommand(@"
                                INSERT INTO Students 
                                (StudentCardNumber, LastName, FirstName, MiddleName, Email, Phone, 
                                 FacultyID, GroupName, Course, EnrollmentDate, BirthDate)
                                VALUES 
                                (@StudentCardNumber, @LastName, @FirstName, @MiddleName, @Email, @Phone,
                                 @FacultyID, @GroupName, @Course, GETDATE(), NULL)",
                                new[]
                                {
                                    new SqlParameter("@StudentCardNumber", StudentCardNumber),
                                    new SqlParameter("@LastName", LastName),
                                    new SqlParameter("@FirstName", FirstName),
                                    new SqlParameter("@MiddleName", (object)MiddleName ?? DBNull.Value),
                                    new SqlParameter("@Email", (object)Email ?? DBNull.Value),
                                    new SqlParameter("@Phone", (object)Phone ?? DBNull.Value),
                                    new SqlParameter("@FacultyID", FacultyID),
                                    new SqlParameter("@GroupName", GroupName),
                                    new SqlParameter("@Course", Course)
                                }, transaction);

                            // 2. Создаём аккаунт (Логин = номер билета)
                            var newAccount = new Account
                            {
                                Login = StudentCardNumber,
                                PasswordHash = HashPassword(Password),
                                Role = AccountRole.Student,
                                StudentCardNumber = StudentCardNumber,
                                IsActive = true
                            };

                            _dbConnection.CreateAccount(newAccount, transaction);

                            // ✅ Коммит транзакции
                            transaction.Commit();

                            SuccessMessage = $"✅ Регистрация успешна! Добро пожаловать, {FirstName}! Ваш логин: {StudentCardNumber}";
                            IsRegisterMode = false;
                            ClearForm();
                        }
                        catch
                        {
                            // ✅ Откат при ошибке
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Ошибка регистрации: " + ex.Message;
            }

            Faculties = _dbConnection.GetFaculties();
            return Page();
        }

        // ✅ Перенаправление по роли
        private string GetRedirectPage(string role) => role switch
        {
            "Trainer" => "/Trainer/Index",
            "Student" => "/Student/Index",
            _ => "/Index"
        };

        // ✅ Хеширование пароля (SHA256 — как в десктопе)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // ✅ Валидация номера студенческого билета (7 цифр: ГГММXXX)
        private bool IsValidStudentCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return false;

            // Удаляем все нецифровые символы
            string cleanNumber = new string(cardNumber.Where(char.IsDigit).ToArray());

            // Должно быть ровно 7 цифр
            if (cleanNumber.Length != 7) return false;

            // Первые 2 цифры — год (20-99)
            int year = int.Parse(cleanNumber.Substring(0, 2));
            if (year < 20 || year > 99) return false;

            // Следующие 2 цифры — месяц (01-12)
            int month = int.Parse(cleanNumber.Substring(2, 2));
            if (month < 1 || month > 12) return false;

            // Последние 2 цифры — порядковый номер (01-99)
            int sequence = int.Parse(cleanNumber.Substring(5, 2));
            if (sequence < 1 || sequence > 99) return false;

            return true;
        }

        // ✅ Очистка формы
        private void ClearForm()
        {
            Login = "";
            Password = "";
            ConfirmPassword = "";
            StudentCardNumber = "";
            LastName = "";
            FirstName = "";
            MiddleName = "";
            Email = "";
            Phone = "";
            FacultyID = 0;
            GroupName = "";
            Course = 1;
        }
    }
}