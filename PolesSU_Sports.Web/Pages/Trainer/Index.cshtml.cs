using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Trainer
{
    public class IndexModel : PageModel
    {
        public string TrainerName { get; set; }
        public int TotalStudents { get; set; }
        public int TotalSections { get; set; }
        public int TodayAttendance { get; set; }
        public List<Section> MySections { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            var userLogin = HttpContext.Session.GetString("UserLogin");
            var accountId = HttpContext.Session.GetInt32("UserID");

            // ✅ Получаем TrainerID из аккаунта
            var accountData = DBConnection.Instance.ExecuteQuery(
                "SELECT TrainerID FROM Accounts WHERE AccountID = @AccountID",
                new[] { new SqlParameter("@AccountID", accountId) });

            if (accountData.Rows.Count == 0 || accountData.Rows[0]["TrainerID"] == DBNull.Value)
            {
                return RedirectToPage("/Index");
            }

            string trainerId = accountData.Rows[0]["TrainerID"].ToString();

            // ✅ Получаем данные тренера
            var trainer = DBConnection.Instance.ExecuteQuery(
            "SELECT LastName, FirstName, MiddleName FROM Trainers WHERE DocumentNumber = @DocumentNumber",  
            new[] { new SqlParameter("@DocumentNumber", trainerId) });

            if (trainer.Rows.Count > 0)
            {
                var lastName = trainer.Rows[0]["LastName"].ToString();
                var firstName = trainer.Rows[0]["FirstName"].ToString();
                var middleName = trainer.Rows[0]["MiddleName"] != DBNull.Value
                    ? trainer.Rows[0]["MiddleName"].ToString() : "";
                TrainerName = $"{lastName} {firstName} {middleName}".Trim();
            }

            // Количество секций
            TotalSections = Convert.ToInt32(DBConnection.Instance.ExecuteScalar(
                "SELECT COUNT(*) FROM Sections WHERE TrainerID = @TrainerID",
                new[] { new SqlParameter("@TrainerID", trainerId) }));

            // Количество студентов во всех секциях
            TotalStudents = Convert.ToInt32(DBConnection.Instance.ExecuteScalar(@"
                SELECT COUNT(DISTINCT ss.StudentCardNumber) 
                FROM StudentSections ss
                JOIN Sections sec ON ss.SectionID = sec.SectionID
                WHERE sec.TrainerID = @TrainerID AND ss.IsActive = 1",
                new[] { new SqlParameter("@TrainerID", trainerId) }));

            // Посещаемость сегодня
            TodayAttendance = Convert.ToInt32(DBConnection.Instance.ExecuteScalar(@"
                SELECT COUNT(*) FROM Attendance a
                JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                JOIN Sections sec ON sc.SectionID = sec.SectionID
                WHERE sec.TrainerID = @TrainerID 
                AND a.VisitDate = CAST(GETDATE() AS DATE)
                AND a.Status = 1",
                new[] { new SqlParameter("@TrainerID", trainerId) }));

            // Мои секции
            MySections = DBConnection.Instance.GetSections()
                .Where(s => s.TrainerID == trainerId).ToList();

            return Page();
        }
    }
}