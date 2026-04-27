using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;

namespace PolesSU_Sports.Web.Pages.Student
{
    public class IndexModel : PageModel
    {
        private readonly DBConnection _dbConnection;

        public IndexModel(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public string StudentName { get; set; }
        public string GroupName { get; set; }
        public int EnrolledSectionsCount { get; set; }
        public int TotalAttendance { get; set; }
        public List<Section> AvailableSections { get; set; }
        public List<Section> EnrolledSections { get; set; }

        public IActionResult OnGet()
        {
            // ✅ Проверка авторизации
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCardNumber = HttpContext.Session.GetString("UserLogin");

            // Получаем данные студента
            var student = _dbConnection.ExecuteQuery(
                "SELECT LastName, FirstName, MiddleName, GroupName FROM Students WHERE StudentCardNumber = @CardNumber",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) });

            if (student.Rows.Count > 0)
            {
                StudentName = $"{student.Rows[0]["LastName"]} {student.Rows[0]["FirstName"]}";
                GroupName = student.Rows[0]["GroupName"].ToString();
            }

            // Количество секций, где записан студент
            EnrolledSectionsCount = Convert.ToInt32(_dbConnection.ExecuteScalar(
                "SELECT COUNT(*) FROM StudentSections WHERE StudentCardNumber = @CardNumber AND IsActive = 1",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) }));

            // Общая посещаемость
            TotalAttendance = Convert.ToInt32(_dbConnection.ExecuteScalar(
                "SELECT COUNT(*) FROM Attendance WHERE StudentCardNumber = @CardNumber AND Status = 1",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) }));

            // Доступные секции (куда ещё не записан)
            AvailableSections = _dbConnection.GetSections();

            // Секции, куда записан студент
            EnrolledSections = new List<Section>();
            var enrolled = _dbConnection.ExecuteQuery(@"
                SELECT sec.* FROM Sections sec
                JOIN StudentSections ss ON sec.SectionID = ss.SectionID
                WHERE ss.StudentCardNumber = @CardNumber AND ss.IsActive = 1",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) });

            foreach (System.Data.DataRow row in enrolled.Rows)
            {
                EnrolledSections.Add(new Section
                {
                    SectionID = Convert.ToInt32(row["SectionID"]),
                    SectionName = row["SectionName"].ToString(),
                    SportID = Convert.ToInt32(row["SportID"]),
                    TrainerID = row["TrainerID"].ToString(),
                    MaxStudents = row["MaxStudents"] != System.DBNull.Value ? Convert.ToInt32(row["MaxStudents"]) : null,
                    PricePerMonth = Convert.ToDecimal(row["PricePerMonth"]),
                    Description = row["Description"].ToString()
                });
            }

            return Page();
        }
    }
}