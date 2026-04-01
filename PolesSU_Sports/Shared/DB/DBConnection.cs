using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using PolesSU_Sports.Shared.Model;

namespace PolesSU_Sports.Shared.DB
{
    public class DBConnection
    {
        private static DBConnection _instance;
        private readonly string _connectionString;
        private SqlConnection _connection;

        private DBConnection()
        {
            string connStr = ConfigurationManager.ConnectionStrings["PolesSU_DB"].ConnectionString;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);
            builder.Encrypt = false;
            builder.TrustServerCertificate = true;
            builder.IntegratedSecurity = true;

            _connectionString = builder.ConnectionString;
            _connection = new SqlConnection(_connectionString);
        }

        public static DBConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DBConnection();
                }
                return _instance;
            }
        }

        public SqlConnection GetConnection()
        {
            if (_connection.State != ConnectionState.Open)
            {
                try
                {
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Не удалось подключиться к базе данных:\n{ex.Message}");
                }
            }
            return _connection;
        }

        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, GetConnection()))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка выполнения запроса: " + ex.Message);
            }
            return table;
        }

        public int ExecuteCommand(string query, SqlParameter[] parameters = null)
        {
            int result = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, GetConnection()))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка выполнения команды: " + ex.Message);
            }
            return result;
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            object result = null;
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, GetConnection()))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения значения: " + ex.Message);
            }
            return result;
        }

        //  МЕТОДЫ ДЛЯ РАБОТЫ С МОДЕЛЯМИ 

        public List<Student> GetStudents()
        {
            List<Student> students = new List<Student>();
            DataTable dt = ExecuteQuery(@"
                SELECT s.*, f.FacultyName 
                FROM Students s 
                JOIN Faculties f ON s.FacultyID = f.FacultyID 
                ORDER BY s.StudentCardNumber");

            foreach (DataRow row in dt.Rows)
            {
                students.Add(new Student
                {
                    StudentCardNumber = row["StudentCardNumber"].ToString(),
                    LastName = row["LastName"].ToString(),
                    FirstName = row["FirstName"].ToString(),
                    MiddleName = row["MiddleName"].ToString(),
                    BirthDate = Convert.ToDateTime(row["BirthDate"]),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString(),
                    FacultyID = Convert.ToInt32(row["FacultyID"]),
                    FacultyName = row["FacultyName"].ToString(),
                    GroupName = row["GroupName"].ToString(),
                    Course = Convert.ToInt32(row["Course"]),
                    EnrollmentDate = Convert.ToDateTime(row["EnrollmentDate"])
                });
            }
            return students;
        }

        public List<Trainer> GetTrainers()
        {
            List<Trainer> trainers = new List<Trainer>();
            DataTable dt = ExecuteQuery("SELECT * FROM Trainers ORDER BY LastName");

            foreach (DataRow row in dt.Rows)
            {
                trainers.Add(new Trainer
                {
                    TrainerID = Convert.ToInt32(row["TrainerID"]),
                    DocumentNumber = row["DocumentNumber"].ToString(),
                    LastName = row["LastName"].ToString(),
                    FirstName = row["FirstName"].ToString(),
                    MiddleName = row["MiddleName"].ToString(),
                    BirthDate = Convert.ToDateTime(row["BirthDate"]),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString(),
                    Qualification = row["Qualification"].ToString(),
                    Specialization = row["Specialization"].ToString(),
                    HireDate = Convert.ToDateTime(row["HireDate"])
                });
            }
            return trainers;
        }

        public List<Section> GetSections()
        {
            List<Section> sections = new List<Section>();
            DataTable dt = ExecuteQuery(@"
                SELECT sec.*, sp.SportName, t.LastName + ' ' + t.FirstName AS TrainerName
                FROM Sections sec
                JOIN Sports sp ON sec.SportID = sp.SportID
                JOIN Trainers t ON sec.TrainerID = t.TrainerID
                ORDER BY sec.SectionName");

            foreach (DataRow row in dt.Rows)
            {
                sections.Add(new Section
                {
                    SectionID = Convert.ToInt32(row["SectionID"]),
                    SectionName = row["SectionName"].ToString(),
                    SportID = Convert.ToInt32(row["SportID"]),
                    SportName = row["SportName"].ToString(),
                    TrainerID = Convert.ToInt32(row["TrainerID"]),
                    TrainerName = row["TrainerName"].ToString(),
                    MaxStudents = row["MaxStudents"] != DBNull.Value ? Convert.ToInt32(row["MaxStudents"]) : null,
                    PricePerMonth = Convert.ToDecimal(row["PricePerMonth"]),
                    Description = row["Description"].ToString(),
                    Requirements = row["Requirements"].ToString()
                });
            }
            return sections;
        }

        public List<Faculty> GetFaculties()
        {
            List<Faculty> faculties = new List<Faculty>();
            DataTable dt = ExecuteQuery("SELECT * FROM Faculties ORDER BY FacultyName");

            foreach (DataRow row in dt.Rows)
            {
                faculties.Add(new Faculty
                {
                    FacultyID = Convert.ToInt32(row["FacultyID"]),
                    FacultyName = row["FacultyName"].ToString(),
                    DeanName = row["DeanName"].ToString(),
                    Phone = row["Phone"].ToString()
                });
            }
            return faculties;
        }

        public DashboardStats GetDashboardStats()
        {
            DataTable dt = ExecuteQuery(@"
                SELECT 
                    (SELECT COUNT(*) FROM Faculties) AS Faculties,
                    (SELECT COUNT(*) FROM Trainers) AS Trainers,
                    (SELECT COUNT(*) FROM Students) AS Students,
                    (SELECT COUNT(*) FROM Sections) AS Sections,
                    (SELECT COUNT(*) FROM Attendance WHERE VisitDate >= DATEADD(MONTH, -1, GETDATE())) AS AttendanceMonth
            ");

            if (dt.Rows.Count > 0)
            {
                return new DashboardStats
                {
                    FacultiesCount = Convert.ToInt32(dt.Rows[0]["Faculties"]),
                    TrainersCount = Convert.ToInt32(dt.Rows[0]["Trainers"]),
                    StudentsCount = Convert.ToInt32(dt.Rows[0]["Students"]),
                    SectionsCount = Convert.ToInt32(dt.Rows[0]["Sections"]),
                    AttendanceMonthCount = Convert.ToInt32(dt.Rows[0]["AttendanceMonth"])
                };
            }
            return new DashboardStats();
        }

        // МЕТОДЫ ДЛЯ УДАЛЕНИЯ



        public bool CanDeleteStudent(string studentCardNumber)
        {
            try
            {
                // Проверяем, есть ли посещаемость
                object attendanceCount = ExecuteScalar(
                    "SELECT COUNT(*) FROM Attendance WHERE StudentCardNumber = @StudentCardNumber",
                    new[] { new SqlParameter("@StudentCardNumber", studentCardNumber) });

                if (Convert.ToInt32(attendanceCount) > 0)
                {
                    return false;
                }

                // Проверяем, есть ли достижения
                object achievementCount = ExecuteScalar(
                    "SELECT COUNT(*) FROM Achievements WHERE StudentCardNumber = @StudentCardNumber",
                    new[] { new SqlParameter("@StudentCardNumber", studentCardNumber) });

                if (Convert.ToInt32(achievementCount) > 0)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool DeleteStudent(string studentCardNumber)
        {
            try
            {
                int result = ExecuteCommand(
                    "DELETE FROM Students WHERE StudentCardNumber = @StudentCardNumber",
                    new[] { new SqlParameter("@StudentCardNumber", studentCardNumber) });

                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка удаления студента: " + ex.Message);
            }
        }

        public bool DeleteTrainer(int trainerID)
        {
            try
            {
                int result = ExecuteCommand(
                    "DELETE FROM Trainers WHERE TrainerID = @TrainerID",
                    new[] { new SqlParameter("@TrainerID", trainerID) });

                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка удаления тренера: " + ex.Message);
            }
        }

        public bool DeleteSection(int sectionID)
        {
            try
            {
                int result = ExecuteCommand(
                    "DELETE FROM Sections WHERE SectionID = @SectionID",
                    new[] { new SqlParameter("@SectionID", sectionID) });

                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка удаления секции: " + ex.Message);
            }
        }

        //  МЕТОДЫ ДЛЯ РАБОТЫ С ACCOUNTS

        public Account Authenticate(string login, string password)
        {
            try
            {
                DataTable dt = ExecuteQuery(@"
            SELECT * FROM Accounts 
            WHERE Login = @Login AND PasswordHash = @Password AND IsActive = 1",
                    new[] {
                new SqlParameter("@Login", login),
                new SqlParameter("@Password", password)
                    });

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return new Account
                    {
                        AccountID = Convert.ToInt32(row["AccountID"]),
                        Login = row["Login"].ToString(),
                        Role = (AccountRole)Enum.Parse(typeof(AccountRole), row["Role"].ToString()),
                        StudentCardNumber = row["StudentCardNumber"] != DBNull.Value ? row["StudentCardNumber"].ToString() : null,
                        TrainerID = row["TrainerID"] != DBNull.Value ? Convert.ToInt32(row["TrainerID"]) : null,
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : null
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка аутентификации: " + ex.Message);
            }
        }

        public List<Account> GetAccounts()
        {
            List<Account> accounts = new List<Account>();
            DataTable dt = ExecuteQuery(@"
        SELECT 
            a.*,
            s.LastName + ' ' + s.FirstName AS StudentName,
            t.LastName + ' ' + t.FirstName AS TrainerName
        FROM Accounts a
        LEFT JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
        LEFT JOIN Trainers t ON a.TrainerID = t.TrainerID
        ORDER BY a.Login");

            foreach (DataRow row in dt.Rows)
            {
                accounts.Add(new Account
                {
                    AccountID = Convert.ToInt32(row["AccountID"]),
                    Login = row["Login"].ToString(),
                    Role = (AccountRole)Enum.Parse(typeof(AccountRole), row["Role"].ToString()),
                    StudentCardNumber = row["StudentCardNumber"] != DBNull.Value ? row["StudentCardNumber"].ToString() : null,
                    TrainerID = row["TrainerID"] != DBNull.Value ? Convert.ToInt32(row["TrainerID"]) : null,
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : null
                });
            }
            return accounts;
        }

        public bool CreateAccount(Account account)
        {
            try
            {
                ExecuteCommand(@"
            INSERT INTO Accounts (Login, PasswordHash, Role, StudentCardNumber, TrainerID, IsActive)
            VALUES (@Login, @PasswordHash, @Role, @StudentCardNumber, @TrainerID, @IsActive)",
                    new[] {
                new SqlParameter("@Login", account.Login),
                new SqlParameter("@PasswordHash", account.PasswordHash),
                new SqlParameter("@Role", account.Role.ToString()),
                new SqlParameter("@StudentCardNumber", (object)account.StudentCardNumber ?? DBNull.Value),
                new SqlParameter("@TrainerID", (object)account.TrainerID ?? DBNull.Value),
                new SqlParameter("@IsActive", account.IsActive)
                    });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка создания аккаунта: " + ex.Message);
            }
        }

        public bool UpdateAccount(Account account)
        {
            try
            {
                ExecuteCommand(@"
            UPDATE Accounts SET 
                Login = @Login,
                Role = @Role,
                IsActive = @IsActive
            WHERE AccountID = @AccountID",
                    new[] {
                new SqlParameter("@Login", account.Login),
                new SqlParameter("@Role", account.Role.ToString()),
                new SqlParameter("@IsActive", account.IsActive),
                new SqlParameter("@AccountID", account.AccountID)
                    });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка обновления аккаунта: " + ex.Message);
            }
        }

        public bool DeleteAccount(int accountID)
        {
            try
            {
                ExecuteCommand("DELETE FROM Accounts WHERE AccountID = @AccountID",
                    new[] { new SqlParameter("@AccountID", accountID) });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка удаления аккаунта: " + ex.Message);
            }
        }

        public bool UpdateLastLogin(int accountID)
        {
            try
            {
                ExecuteCommand("UPDATE Accounts SET LastLogin = GETDATE() WHERE AccountID = @AccountID",
                    new[] { new SqlParameter("@AccountID", accountID) });
                return true;
            }
            catch { return false; }
        }

        // Проверка существования логина
        public bool IsLoginExists(string login, int? excludeAccountID = null)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Accounts WHERE Login = @Login";
                if (excludeAccountID.HasValue)
                {
                    query += " AND AccountID != @AccountID";
                }

                object result = ExecuteScalar(query,
                    excludeAccountID.HasValue
                        ? new[] {
                    new SqlParameter("@Login", login),
                    new SqlParameter("@AccountID", excludeAccountID.Value)
                          }
                        : new[] { new SqlParameter("@Login", login) });

                return Convert.ToInt32(result) > 0;
            }
            catch { return false; }
        }
    }
}