using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;

namespace PolesSU_Sports.Web.Pages.Student
{
    public class ScheduleModel : PageModel
    {
        public DataTable Schedule { get; set; }
        public string StudentName { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCardNumber = HttpContext.Session.GetString("UserLogin");

            var student = DBConnection.Instance.ExecuteQuery(
                "SELECT LastName, FirstName FROM Students WHERE StudentCardNumber = @CardNumber",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) });

            if (student.Rows.Count > 0)
            {
                StudentName = $"{student.Rows[0]["LastName"]} {student.Rows[0]["FirstName"]}";
            }

            Schedule = DBConnection.Instance.ExecuteQuery(@"
            SELECT 
                CASE sc.DayOfWeek
                    WHEN 1 THEN 'Понедельник'
                    WHEN 2 THEN 'Вторник'
                    WHEN 3 THEN 'Среда'
                    WHEN 4 THEN 'Четверг'
                    WHEN 5 THEN 'Пятница'
                    WHEN 6 THEN 'Суббота'
                    WHEN 7 THEN 'Воскресенье'
                    ELSE 'Неизвестно'
                END AS [День недели],
                CONVERT(VARCHAR(5), sc.StartTime, 108) AS [Начало],
                CONVERT(VARCHAR(5), sc.EndTime, 108) AS [Окончание],
                sec.SectionName AS [Секция],
                sp.SportName AS [Вид спорта],
                t.LastName + ' ' + t.FirstName AS [Тренер],
                ISNULL(NULLIF(LTRIM(RTRIM(sc.Room)), ''), 'Не указано') AS [Кабинет]
            FROM Schedule sc
            JOIN Sections sec ON sc.SectionID = sec.SectionID
            JOIN Sports sp ON sec.SportID = sp.SportID
            JOIN Trainers t ON sec.TrainerID = t.TrainerID
            JOIN StudentSections ss ON sec.SectionID = ss.SectionID 
                AND ss.StudentCardNumber = @CardNumber 
                AND ss.IsActive = 1
            ORDER BY sc.DayOfWeek, sc.StartTime",
            new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) });

            return Page();
        }
    }
}