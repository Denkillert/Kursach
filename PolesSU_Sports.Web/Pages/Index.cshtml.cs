using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.Model;
using System.Text;
using System.Security.Cryptography;
using PolesSU_Sports.Lib.DB;

public class IndexModel : PageModel
{
    // ✅ Поля для ВХОДА (универсальные)
    [BindProperty] public string Login { get; set; }  // ✅ Вернули! Для студентов = номер билета, для тренеров = логин от админа
    [BindProperty] public string Password { get; set; }

    // ✅ Поля для РЕГИСТРАЦИИ (только студенты)
    [BindProperty] public string ConfirmPassword { get; set; }
    [BindProperty] public string StudentCardNumber { get; set; }
    [BindProperty] public string LastName { get; set; }
    [BindProperty] public string FirstName { get; set; }
    [BindProperty] public string MiddleName { get; set; }
    [BindProperty] public string Email { get; set; }
    [BindProperty] public string Phone { get; set; }
    [BindProperty] public int FacultyID { get; set; }
    [BindProperty] public string GroupName { get; set; }
    [BindProperty] public int Course { get; set; }

    [BindProperty] public bool IsRegisterMode { get; set; } = false;
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public List<Faculty> Faculties { get; set; }

    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (!string.IsNullOrEmpty(role))
            return RedirectToPage(GetRedirectPage(role));

        Faculties = DBConnection.Instance.GetFaculties();
        return Page();
    }

    // ✅ ВХОД (универсальный: для студентов и тренеров)
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

            // ✅ Аутентификация по логину (для студентов это номер билета, для тренеров — выданный логин)
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

    // ✅ РЕГИСТРАЦИЯ (только для студентов, логин = номер билета)
    public IActionResult OnPostRegister()
    {
        if (string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(StudentCardNumber) ||
            string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName) ||
            FacultyID == 0 || string.IsNullOrWhiteSpace(GroupName))
        {
            ErrorMessage = "⚠️ Заполните все обязательные поля";
            IsRegisterMode = true;
            Faculties = DBConnection.Instance.GetFaculties();
            return Page();
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "❌ Пароль должен быть не менее 6 символов";
            IsRegisterMode = true;
            Faculties = DBConnection.Instance.GetFaculties();
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "❌ Пароли не совпадают";
            IsRegisterMode = true;
            Faculties = DBConnection.Instance.GetFaculties();
            return Page();
        }

        if (!IsValidStudentCardNumber(StudentCardNumber))
        {
            ErrorMessage = "❌ Неверный формат номера билета. Пример: 2409001 (24-09-001)";
            IsRegisterMode = true;
            Faculties = DBConnection.Instance.GetFaculties();
            return Page();
        }

        try
        {
            // Проверяем, нет ли уже студента с таким билетом
            var existingStudent = DBConnection.Instance.ExecuteQuery(
                "SELECT StudentCardNumber FROM Students WHERE StudentCardNumber = @CardNumber",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", StudentCardNumber) });

            if (existingStudent.Rows.Count > 0)
            {
                ErrorMessage = "❌ Студент с таким номером билета уже существует";
                IsRegisterMode = true;
                Faculties = DBConnection.Instance.GetFaculties();
                return Page();
            }

            // Проверяем, нет ли уже аккаунта с таким логином (=номером билета)
            var existingAccount = DBConnection.Instance.ExecuteQuery(
                "SELECT AccountID FROM Accounts WHERE Login = @Login",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@Login", StudentCardNumber) });

            if (existingAccount.Rows.Count > 0)
            {
                ErrorMessage = "❌ На этот номер билета уже зарегистрирован аккаунт";
                IsRegisterMode = true;
                Faculties = DBConnection.Instance.GetFaculties();
                return Page();
            }

            // ТРАНЗАКЦИЯ: создаём Студента и Аккаунт
            using (var connection = DBConnection.Instance.GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Создаём студента
                        DBConnection.Instance.ExecuteCommand(@"
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
                            Login = StudentCardNumber,  // ✅ Для студентов логин = номер билета
                            PasswordHash = HashPassword(Password),
                            Role = AccountRole.Student,
                            StudentCardNumber = StudentCardNumber,
                            IsActive = true
                        };

                        DBConnection.Instance.CreateAccount(newAccount, transaction);
                        transaction.Commit();

                        SuccessMessage = $"✅ Регистрация успешна! Добро пожаловать, {FirstName}! Ваш логин: {StudentCardNumber}";
                        IsRegisterMode = false;
                        ClearForm();
                    }
                    catch
                    {
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

        Faculties = DBConnection.Instance.GetFaculties();
        return Page();
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

    private bool IsValidStudentCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return false;
        string cleanNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (cleanNumber.Length != 7) return false;
        int year = int.Parse(cleanNumber.Substring(0, 2));
        if (year < 20 || year > 99) return false;
        int month = int.Parse(cleanNumber.Substring(2, 2));
        if (month < 1 || month > 12) return false;
        int sequence = int.Parse(cleanNumber.Substring(5, 3));
        if (sequence < 1 || sequence > 999) return false;
        return true;
    }

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