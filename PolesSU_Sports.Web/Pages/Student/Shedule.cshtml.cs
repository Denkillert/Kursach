using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;

namespace PolesSU_Sports.Web.Pages.Student
{
    public class ScheduleModel : PageModel
    {
        public DataTable Schedule { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCardNumber = HttpContext.Session.GetString("UserLogin");

            Schedule = DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    sc.Date AS [Дата],
                    sc.TimeSlot AS [Время],
                    sec.SectionName AS [Секция],
                    sp.SportName AS [Вид спорта],
                    t.LastName + ' ' + t.FirstName AS [Тренер],
                    CASE WHEN a.AttendanceID IS NOT NULL AND a.Status = 1 THEN '✅' ELSE '❌' END AS [Посещение]
                FROM Schedule sc
                JOIN Sections sec ON sc.SectionID = sec.SectionID
                JOIN Sports sp ON sec.SportID = sp.SportID
                JOIN Trainers t ON sec.TrainerID = t.TrainerID
                JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.StudentCardNumber = @CardNumber AND ss.IsActive = 1
                LEFT JOIN Attendance a ON sc.ScheduleID = a.ScheduleID AND a.StudentCardNumber = @CardNumber
                WHERE sc.Date >= GETDATE()
                ORDER BY sc.Date, sc.TimeSlot",
                new[] { new Microsoft.Data.SqlClient.SqlParameter("@CardNumber", studentCardNumber) });

            return Page();
        }
    }
}