using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Trainer
{
    public class ScheduleModel : PageModel
    {
        public DataTable WeeklySchedule { get; set; }
        public string TrainerName { get; set; }
        public string CurrentWeek { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            var accountId = HttpContext.Session.GetInt32("UserID");

            try
            {
                // Получаем TrainerID
                var accountData = DBConnection.Instance.ExecuteQuery(
                    "SELECT TrainerID FROM Accounts WHERE AccountID = @AccountID",
                    new[] { new SqlParameter("@AccountID", accountId) });

                if (accountData.Rows.Count == 0 || accountData.Rows[0]["TrainerID"] == DBNull.Value)
                    return RedirectToPage("/Index");

                int trainerId = Convert.ToInt32(accountData.Rows[0]["TrainerID"]);

                // Получаем имя тренера
                var trainer = DBConnection.Instance.ExecuteQuery(
                    "SELECT LastName, FirstName FROM Trainers WHERE TrainerID = @TrainerID",
                    new[] { new SqlParameter("@TrainerID", trainerId) });

                if (trainer.Rows.Count > 0)
                {
                    TrainerName = $"{trainer.Rows[0]["LastName"]} {trainer.Rows[0]["FirstName"]}";
                }

                CurrentWeek = $"Неделя: {DateTime.Now:dd.MM.yyyy}";

                // Получаем расписание на всю неделю (начиная с понедельника)
                var monday = GetMondayOfWeek();

                WeeklySchedule = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        CASE sc.DayOfWeek
                            WHEN 1 THEN 'Понедельник'
                            WHEN 2 THEN 'Вторник'
                            WHEN 3 THEN 'Среда'
                            WHEN 4 THEN 'Четверг'
                            WHEN 5 THEN 'Пятница'
                            WHEN 6 THEN 'Суббота'
                            WHEN 7 THEN 'Воскресенье'
                        END AS [День недели],
                        CONVERT(VARCHAR(5), sc.StartTime, 108) AS [Начало],
                        CONVERT(VARCHAR(5), sc.EndTime, 108) AS [Окончание],
                        sec.SectionName AS [Секция],
                        sp.SportName AS [Вид спорта],
                        ISNULL(NULLIF(LTRIM(RTRIM(sc.Room)), ''), 'Не указано') AS [Кабинет],
                        (SELECT COUNT(*) FROM StudentSections ss 
                         WHERE ss.SectionID = sec.SectionID AND ss.IsActive = 1) AS [Студентов]
                    FROM Schedule sc
                    JOIN Sections sec ON sc.SectionID = sec.SectionID
                    JOIN Sports sp ON sec.SportID = sp.SportID
                    WHERE sec.TrainerID = @TrainerID
                    ORDER BY sc.DayOfWeek, sc.StartTime",
                    new[] { new SqlParameter("@TrainerID", trainerId) });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
            }

            return Page();
        }

        private DateTime GetMondayOfWeek()
        {
            var today = DateTime.Now;
            int daysToMonday = ((int)today.DayOfWeek + 6) % 7;
            return today.AddDays(-daysToMonday).Date;
        }
    }
}