using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using System.Security.Cryptography;
using System.Text;

namespace PolesSU_Sports.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty] public string Login { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string StudentCardNumber { get; set; } // Для регистрации
        [BindProperty] public bool IsRegisterMode { get; set; } = false;

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(role))
                return RedirectToPage(GetRedirectPage(role));
            return Page();
        }

        // ✅ ВХОД
        public IActionResult OnPostLogin()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "⚠️ Введите логин и пароль";
                return Page();
            }

            try
            {
                string passwordHash = HashPassword(Password);
                var account = DBConnection.Instance.Authenticate(Login, passwordHash);

                if (account != null && account.IsActive)
                {
                    HttpContext.Session.SetString("UserLogin", account.Login);
                    HttpContext.Session.SetString("UserRole", account.Role.ToString());
                    HttpContext.Session.SetInt32("UserID", account.AccountID);

                    DBConnection.Instance.UpdateLastLogin(account.AccountID);
                    return RedirectToPage(GetRedirectPage(account.Role.ToString()));
                }

                ErrorMessage = "❌ Неверный логин или пароль";
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Ошибка: " + ex.Message;
            }
            return Page();
        }

        // ✅ РЕГИСТРАЦИЯ (только для студентов)
        public IActionResult OnPostRegister()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(StudentCardNumber))
            {
                ErrorMessage = "⚠️ Заполните все поля";
                IsRegisterMode = true;
                return Page();
            }

            // ✅ ВАЛИДАЦИЯ НОМЕРА БИЛЕТА (7 цифр: ГГММXXX)
            if (!IsValidStudentCardNumber(StudentCardNumber))
            {
                ErrorMessage = "❌ Неверный формат номера билета. Пример: 2409001 (24-09-001)";
                IsRegisterMode = true;
                return Page();
            }

            try
            {
                // Проверяем, что студент с таким билетом существует
                var student = DBConnection.Instance.ExecuteQuery(
                    "SELECT StudentCardNumber, LastName, FirstName FROM Students WHERE StudentCardNumber = @CardNumber",
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", StudentCardNumber) });

                if (student.Rows.Count == 0)
                {
                    ErrorMessage = "❌ Студент с таким номером билета не найден в базе. Обратитесь к менеджеру.";
                    IsRegisterMode = true;
                    return Page();
                }

                // Проверяем, что логин свободен
                if (DBConnection.Instance.IsLoginExists(Login))
                {
                    ErrorMessage = "❌ Такой логин уже занят";
                    IsRegisterMode = true;
                    return Page();
                }

                // Проверяем, нет ли уже аккаунта у этого студента
                var existingAccount = DBConnection.Instance.ExecuteQuery(
                    "SELECT AccountID FROM Accounts WHERE StudentCardNumber = @CardNumber",
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", StudentCardNumber) });

                if (existingAccount.Rows.Count > 0)
                {
                    ErrorMessage = "❌ На этот номер билета уже зарегистрирован аккаунт";
                    IsRegisterMode = true;
                    return Page();
                }

                // Создаём аккаунт
                var newAccount = new Account
                {
                    Login = Login,
                    PasswordHash = HashPassword(Password),
                    Role = AccountRole.Student,
                    StudentCardNumber = StudentCardNumber,
                    IsActive = true
                };

                DBConnection.Instance.CreateAccount(newAccount);

                SuccessMessage = $"✅ Регистрация успешна! Добро пожаловать, {student.Rows[0]["FirstName"]}! Теперь войдите в систему.";
                IsRegisterMode = false;
                Login = "";
                Password = "";
                StudentCardNumber = "";
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Ошибка регистрации: " + ex.Message;
            }
            return Page();
        }

        // ✅ ВАЛИДАЦИЯ НОМЕРА СТУДЕНЧЕСКОГО БИЛЕТА
        private bool IsValidStudentCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return false;

            // Удаляем все нецифровые символы (дефисы, пробелы)
            string cleanNumber = new string(cardNumber.Where(char.IsDigit).ToArray());

            // Должно быть ровно 7 цифр
            if (cleanNumber.Length != 7) return false;

            // Первые 2 цифры - год (от 20 до 99)
            int year = int.Parse(cleanNumber.Substring(0, 2));
            if (year < 20 || year > 99) return false;

            // Следующие 2 цифры - месяц (01-12)
            int month = int.Parse(cleanNumber.Substring(2, 2));
            if (month < 1 || month > 12) return false;

            // Последние 3 цифры - порядковый номер (001-999)
            int sequence = int.Parse(cleanNumber.Substring(5, 3));
            if (sequence < 1 || sequence > 999) return false;

            return true;
        }

        private string GetRedirectPage(string role) => role switch
        {
            "Trainer" => "/Trainer/Index",
            "Student" => "/Student/Index",
            _ => "/Index"
        };

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}