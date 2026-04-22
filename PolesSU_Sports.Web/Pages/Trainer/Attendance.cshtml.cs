using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Trainer
{
    public class AttendanceModel : PageModel
    {
        public DataTable TodaySchedule { get; set; }
        public string TrainerName { get; set; }
        public string CurrentDate { get; set; }
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            var accountId = HttpContext.Session.GetInt32("UserID");

            try
            {
                // Получаем TrainerID из аккаунта
                var accountData = DBConnection.Instance.ExecuteQuery(
                    "SELECT TrainerID FROM Accounts WHERE AccountID = @AccountID",
                    new[] { new SqlParameter("@AccountID", accountId) });

                if (accountData.Rows.Count == 0 || accountData.Rows[0]["TrainerID"] == DBNull.Value)
                {
                    ErrorMessage = "Тренер не найден";
                    return Page();
                }

                int trainerId = Convert.ToInt32(accountData.Rows[0]["TrainerID"]);

                // Получаем имя тренера
                var trainer = DBConnection.Instance.ExecuteQuery(
                    "SELECT LastName, FirstName FROM Trainers WHERE TrainerID = @TrainerID",
                    new[] { new SqlParameter("@TrainerID", trainerId) });

                if (trainer.Rows.Count > 0)
                {
                    TrainerName = $"{trainer.Rows[0]["LastName"]} {trainer.Rows[0]["FirstName"]}";
                }

                CurrentDate = DateTime.Now.ToString("dd MMMM yyyy, dddd");

                // ✅ Получаем расписание на сегодня (TrainerID берём из Sections, не из Schedule!)
                int dayOfWeek = (int)DateTime.Now.DayOfWeek;

                TodaySchedule = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        sc.ScheduleID,
                        sec.SectionName AS [Секция],
                        sp.SportName AS [Вид спорта],
                        CONVERT(VARCHAR(5), sc.StartTime, 108) AS [Начало],
                        CONVERT(VARCHAR(5), sc.EndTime, 108) AS [Окончание],
                        ISNULL(NULLIF(LTRIM(RTRIM(sc.Room)), ''), 'Не указано') AS [Кабинет],
                        (SELECT COUNT(*) FROM StudentSections ss 
                         WHERE ss.SectionID = sec.SectionID AND ss.IsActive = 1) AS [Студентов],
                        (SELECT COUNT(*) FROM Attendance a 
                         WHERE a.ScheduleID = sc.ScheduleID 
                         AND a.VisitDate = CAST(GETDATE() AS DATE)
                         AND a.Status = 1) AS [Отмечено]
                    FROM Schedule sc
                    JOIN Sections sec ON sc.SectionID = sec.SectionID
                    JOIN Sports sp ON sec.SportID = sp.SportID
                    WHERE sec.TrainerID = @TrainerID  -- ✅ Здесь берём из Sections
                    AND sc.DayOfWeek = @DayOfWeek
                    ORDER BY sc.StartTime",
                    new[] {
                        new SqlParameter("@TrainerID", trainerId),
                        new SqlParameter("@DayOfWeek", dayOfWeek)
                    });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки данных: " + ex.Message;
            }

            return Page();
        }
    }
}