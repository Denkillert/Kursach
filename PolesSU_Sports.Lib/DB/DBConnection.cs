using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace PolesSU_Sports.Lib.DB
{
    public class DBConnection
    {
        private static DBConnection _instance;
        private readonly string _connectionString;
        private readonly IDbConnectionFactory _connectionFactory;
        private SqlConnection _connection;

        // ✅ Конструктор для DI (веб)
        public DBConnection(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connectionString = _connectionFactory.GetConnectionString();

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new Exception("❌ Строка подключения не найдена!");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
            builder.Encrypt = false;
            builder.TrustServerCertificate = true;
            builder.IntegratedSecurity = false;

            _connectionString = builder.ConnectionString;
        }

        // ✅ Конструктор для десктопа (через app.config)
        private DBConnection()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["PolesSU_DB"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connStr))
                throw new Exception("❌ Строка подключения не найдена в app.config!");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);
            builder.Encrypt = false;
            builder.TrustServerCertificate = true;
            builder.IntegratedSecurity = false;

            _connectionString = builder.ConnectionString;
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
            SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Не удалось подключиться к базе данных:\n{ex.Message}");
                }
            }
            return connection;
        }

        public void CloseConnection(SqlConnection connection)
        {
            if (connection?.State != ConnectionState.Closed)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        // ✅ ОСНОВНОЙ МЕТОД (без транзакции)
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

        // ✅ ПЕРЕГРУЗКА С ТРАНЗАКЦИЕЙ
        public DataTable ExecuteQuery(string query, SqlParameter[] parameters, SqlTransaction transaction)
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, transaction.Connection, transaction))
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

        // ✅ ОСНОВНОЙ МЕТОД (без транзакции)
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

        // ✅ ПЕРЕГРУЗКА С ТРАНЗАКЦИЕЙ
        public int ExecuteCommand(string query, SqlParameter[] parameters, SqlTransaction transaction)
        {
            int result = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, transaction.Connection, transaction))
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
        // ✅ ПЕРЕГРУЗКА С ТРАНЗАКЦИЕЙ (ДОБАВИТЬ ЭТО!)
        public object ExecuteScalar(string query, SqlParameter[] parameters, SqlTransaction transaction)
        {
            object result = null;
            try
            {
                // ✅ Сначала убеждаемся, что соединение открыто
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(query, _connection, transaction))
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

        // ==================== МЕТОДЫ ДЛЯ РАБОТЫ С МОДЕЛЯМИ ====================

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
                JOIN Trainers t ON sec.TrainerID = t.DocumentNumber
                ORDER BY sec.SectionName");

            foreach (DataRow row in dt.Rows)
            {
                sections.Add(new Section
                {
                    SectionID = Convert.ToInt32(row["SectionID"]),
                    SectionName = row["SectionName"].ToString(),
                    SportID = Convert.ToInt32(row["SportID"]),
                    SportName = row["SportName"].ToString(),
                    TrainerID = row["TrainerID"].ToString(),
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

        // ==================== МЕТОДЫ ДЛЯ УДАЛЕНИЯ ====================

        public bool CanDeleteStudent(string studentCardNumber)
        {
            try
            {
                object attendanceCount = ExecuteScalar(
                    "SELECT COUNT(*) FROM Attendance WHERE StudentCardNumber = @StudentCardNumber",
                    new[] { new SqlParameter("@StudentCardNumber", studentCardNumber) });

                if (Convert.ToInt32(attendanceCount) > 0) return false;

                object achievementCount = ExecuteScalar(
                    "SELECT COUNT(*) FROM Achievements WHERE StudentCardNumber = @StudentCardNumber",
                    new[] { new SqlParameter("@StudentCardNumber", studentCardNumber) });

                if (Convert.ToInt32(achievementCount) > 0) return false;

                return true;
            }
            catch { return false; }
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

        public bool DeleteTrainer(string DocumentNumber)
        {
            try
            {
                int result = ExecuteCommand(
                    "DELETE FROM Trainers WHERE DocumentNumber = @DocumentNumber",
                    new[] { new SqlParameter("@DocumentNumber", DocumentNumber) });
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

        // ==================== МЕТОДЫ ДЛЯ РАБОТЫ С ACCOUNTS ====================

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
                        TrainerID = row["TrainerID"] != DBNull.Value ? row["TrainerID"].ToString() : null,
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
                LEFT JOIN Trainers t ON a.TrainerID = t.DocumentNumber  
                ORDER BY a.Login");

            foreach (DataRow row in dt.Rows)
            {
                accounts.Add(new Account
                {
                    AccountID = Convert.ToInt32(row["AccountID"]),
                    Login = row["Login"].ToString(),
                    Role = (AccountRole)Enum.Parse(typeof(AccountRole), row["Role"].ToString()),
                    StudentCardNumber = row["StudentCardNumber"] != DBNull.Value ? row["StudentCardNumber"].ToString() : null,
                    TrainerID = row["TrainerID"] != DBNull.Value ? row["TrainerID"].ToString() : null,
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : null
                });
            }
            return accounts;
        }

        // ✅ ОСНОВНОЙ МЕТОД (без транзакции)
        public bool CreateAccount(Account account)
        {
            try
            {
                string query = @"
                    INSERT INTO Accounts (Login, PasswordHash, Role, StudentCardNumber, TrainerID, IsActive, CreatedDate)
                    VALUES (@Login, @PasswordHash, @Role, @StudentCardNumber, @TrainerID, @IsActive, GETDATE())";

                var parameters = new[] {
                    new SqlParameter("@Login", account.Login),
                    new SqlParameter("@PasswordHash", account.PasswordHash),
                    new SqlParameter("@Role", account.Role.ToString()),
                    new SqlParameter("@StudentCardNumber", (object)account.StudentCardNumber ?? DBNull.Value),
                    new SqlParameter("@TrainerID", (object)account.TrainerID ?? DBNull.Value),
                    new SqlParameter("@IsActive", account.IsActive ? 1 : 0)
                };

                ExecuteCommand(query, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка создания аккаунта: " + ex.Message);
            }
        }

        // ✅ ПЕРЕГРУЗКА С ТРАНЗАКЦИЕЙ
        public bool CreateAccount(Account account, SqlTransaction transaction)
        {
            try
            {
                string query = @"
                    INSERT INTO Accounts (Login, PasswordHash, Role, StudentCardNumber, TrainerID, IsActive, CreatedDate)
                    VALUES (@Login, @PasswordHash, @Role, @StudentCardNumber, @TrainerID, @IsActive, GETDATE())";

                var parameters = new[] {
                    new SqlParameter("@Login", account.Login),
                    new SqlParameter("@PasswordHash", account.PasswordHash),
                    new SqlParameter("@Role", account.Role.ToString()),
                    new SqlParameter("@StudentCardNumber", (object)account.StudentCardNumber ?? DBNull.Value),
                    new SqlParameter("@TrainerID", (object)account.TrainerID ?? DBNull.Value),
                    new SqlParameter("@IsActive", account.IsActive ? 1 : 0)
                };

                ExecuteCommand(query, parameters, transaction);
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
                string query = @"
                    UPDATE Accounts SET 
                        Login = @Login,
                        Role = @Role,
                        IsActive = @IsActive
                    WHERE AccountID = @AccountID";

                var parameters = new[] {
                    new SqlParameter("@Login", account.Login),
                    new SqlParameter("@Role", account.Role.ToString()),
                    new SqlParameter("@IsActive", account.IsActive),
                    new SqlParameter("@AccountID", account.AccountID)
                };

                ExecuteCommand(query, parameters);
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
        // ==================== ЗАЯВКИ СТУДЕНТОВ ====================

        public void CreateSectionRequest(string studentCard, int sectionID, string requestType)
        {
            try
            {
                ExecuteCommand(@"
            INSERT INTO StudentSectionRequests (StudentCardNumber, SectionID, RequestType, Status)
            VALUES (@StudentCard, @SectionID, @RequestType, 'Pending')",
                    new[] {
                new SqlParameter("@StudentCard", studentCard),
                new SqlParameter("@SectionID", sectionID),
                new SqlParameter("@RequestType", requestType)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка создания заявки: " + ex.Message);
            }
        }

        public DataTable GetStudentRequests(string studentCard)
        {
            try
            {
                return ExecuteQuery(@"
            SELECT 
                r.RequestID,
                r.RequestType,
                r.Status,
                r.RequestDate,
                r.ResponseDate,
                r.RejectionReason,
                sec.SectionName,
                sp.SportName
            FROM StudentSectionRequests r
            JOIN Sections sec ON r.SectionID = sec.SectionID
            JOIN Sports sp ON sec.SportID = sp.SportID
            WHERE r.StudentCardNumber = @StudentCard
            ORDER BY r.RequestDate DESC",
                    new[] { new SqlParameter("@StudentCard", studentCard) });
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения заявок: " + ex.Message);
            }
        }

        public DataTable GetAllPendingRequests()
        {
            try
            {
                return ExecuteQuery(@"
            SELECT 
                r.RequestID,
                r.StudentCardNumber,
                s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS StudentName,
                s.GroupName,
                r.SectionID,
                sec.SectionName,
                sp.SportName,
                r.RequestType,
                r.RequestDate
            FROM StudentSectionRequests r
            JOIN Students s ON r.StudentCardNumber = s.StudentCardNumber
            JOIN Sections sec ON r.SectionID = sec.SectionID
            JOIN Sports sp ON sec.SportID = sp.SportID
            WHERE r.Status = 'Pending'
            ORDER BY r.RequestDate DESC",
                    null);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения заявок: " + ex.Message);
            }
        }

        public void ProcessRequest(int requestID, bool approved, int managerID, string reason = null)
        {
            var connection = GetConnection();

            var transaction = connection.BeginTransaction();

            try
            {
                // Получаем данные заявки
                var request = ExecuteQuery(@"
            SELECT StudentCardNumber, SectionID, RequestType 
            FROM StudentSectionRequests 
            WHERE RequestID = @RequestID",
                    new[] { new SqlParameter("@RequestID", requestID) }, transaction);

                if (request.Rows.Count == 0)
                    throw new Exception("Заявка не найдена");

                var studentCard = request.Rows[0]["StudentCardNumber"].ToString();
                var sectionID = Convert.ToInt32(request.Rows[0]["SectionID"]);
                var requestType = request.Rows[0]["RequestType"].ToString();

                // Обновляем статус заявки
                ExecuteCommand(@"
            UPDATE StudentSectionRequests 
            SET Status = @Status, 
                ResponseDate = GETDATE(),
                ManagerID = @ManagerID,
                RejectionReason = @Reason
            WHERE RequestID = @RequestID",
                    new[] {
                new SqlParameter("@Status", approved ? "Approved" : "Rejected"),
                new SqlParameter("@ManagerID", managerID),
                new SqlParameter("@Reason", (object)reason ?? DBNull.Value),
                new SqlParameter("@RequestID", requestID)
                    }, transaction);

                // Если одобрено — выполняем действие
                if (approved)
                {
                    if (requestType == "Join")
                    {
                        // ✅ ПРОВЕРЯЕМ: есть ли запись с IsActive = 0
                        var checkResult = ExecuteQuery(@"
                    SELECT StudentSectionID FROM StudentSections 
                    WHERE StudentCardNumber = @StudentCard AND SectionID = @SectionID",
                            new[] {
                        new SqlParameter("@StudentCard", studentCard),
                        new SqlParameter("@SectionID", sectionID)
                            }, transaction);

                        if (checkResult.Rows.Count > 0)
                        {
                            // ✅ ЗАПИСЬ ЕСТЬ — просто активируем
                            ExecuteCommand(@"
                        UPDATE StudentSections 
                        SET IsActive = 1 
                        WHERE StudentCardNumber = @StudentCard AND SectionID = @SectionID",
                                new[] {
                            new SqlParameter("@StudentCard", studentCard),
                            new SqlParameter("@SectionID", sectionID)
                                }, transaction);
                        }
                        else
                        {
                            // ✅ ЗАПИСИ НЕТ — вставляем новую
                            ExecuteCommand(@"
                        INSERT INTO StudentSections (StudentCardNumber, SectionID, EnrollmentDate, IsActive)
                        VALUES (@StudentCard, @SectionID, GETDATE(), 1)",
                                new[] {
                            new SqlParameter("@StudentCard", studentCard),
                            new SqlParameter("@SectionID", sectionID)
                                }, transaction);
                        }
                    }
                    else if (requestType == "Leave")
                    {
                        ExecuteCommand(@"
                    UPDATE StudentSections 
                    SET IsActive = 0
                    WHERE StudentCardNumber = @StudentCard AND SectionID = @SectionID",
                            new[] {
                        new SqlParameter("@StudentCard", studentCard),
                        new SqlParameter("@SectionID", sectionID)
                            }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw new Exception("Ошибка обработки заявки: " + ex.Message);
            }
            
                transaction?.Dispose();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                connection.Dispose();
            
        }

        public bool HasPendingRequest(string studentCard, int sectionID, string requestType)
        {
            try
            {
                var result = ExecuteScalar(@"
            SELECT COUNT(*) FROM StudentSectionRequests 
            WHERE StudentCardNumber = @StudentCard 
            AND SectionID = @SectionID 
            AND RequestType = @RequestType 
            AND Status = 'Pending'",
                    new[] {
                new SqlParameter("@StudentCard", studentCard),
                new SqlParameter("@SectionID", sectionID),
                new SqlParameter("@RequestType", requestType)
                    });
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool IsEnrolledInSection(string studentCard, int sectionID)
        {
            try
            {
                var result = ExecuteScalar(@"
            SELECT COUNT(*) FROM StudentSections 
            WHERE StudentCardNumber = @StudentCard 
            AND SectionID = @SectionID 
            AND IsActive = 1",
                    new[] {
                new SqlParameter("@StudentCard", studentCard),
                new SqlParameter("@SectionID", sectionID)
                    });
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Удаление студента из секции (менеджером)
        public void RemoveStudentFromSection(string studentCard, int sectionID)
        {
            try
            {
                ExecuteCommand(@"
            UPDATE StudentSections 
            SET IsActive = 0, ExitDate = GETDATE()
            WHERE StudentCardNumber = @StudentCard AND SectionID = @SectionID",
                    new[] {
                new SqlParameter("@StudentCard", studentCard),
                new SqlParameter("@SectionID", sectionID)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка удаления: " + ex.Message);
            }
        }

        public DataTable GetSectionStudents(int sectionID)
        {
            try
            {
                return ExecuteQuery(@"
            SELECT 
                s.StudentCardNumber,
                s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS FullName,
                s.GroupName,
                f.FacultyName,
                ss.EnrollmentDate
            FROM StudentSections ss
            JOIN Students s ON ss.StudentCardNumber = s.StudentCardNumber
            JOIN Faculties f ON s.FacultyID = f.FacultyID
            WHERE ss.SectionID = @SectionID AND ss.IsActive = 1
            ORDER BY s.LastName",
                    new[] { new SqlParameter("@SectionID", sectionID) });
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения студентов: " + ex.Message);
            }
        }
    }
}