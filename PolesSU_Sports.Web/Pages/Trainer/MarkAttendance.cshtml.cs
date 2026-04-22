using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Trainer
{
    public class MarkAttendanceModel : PageModel
    {
        public int ScheduleId { get; set; }
        public string SectionName { get; set; }
        public string VisitDate { get; set; }
        public DataTable Students { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IActionResult OnGet(int scheduleId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            ScheduleId = scheduleId;
            VisitDate = DateTime.Now.ToString("dd.MM.yyyy");

            try
            {
                // Получаем информацию о занятии
                var schedule = DBConnection.Instance.ExecuteQuery(@"
                    SELECT sec.SectionName, sp.SportName, sc.Room
                    FROM Schedule sc
                    JOIN Sections sec ON sc.SectionID = sec.SectionID
                    JOIN Sports sp ON sec.SportID = sp.SportID
                    WHERE sc.ScheduleID = @ScheduleID",
                    new[] { new SqlParameter("@ScheduleID", scheduleId) });

                if (schedule.Rows.Count > 0)
                {
                    SectionName = $"{schedule.Rows[0]["SectionName"]} ({schedule.Rows[0]["SportName"]})";
                }
                else
                {
                    ErrorMessage = "Занятие не найдено";
                    return RedirectToPage("/Trainer/Attendance");
                }

                // Получаем всех студентов в секции + их посещаемость на сегодня
                Students = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        s.StudentCardNumber,
                        s.LastName,
                        s.FirstName,
                        s.MiddleName,
                        s.GroupName,
                        CASE 
                            WHEN a.Status IS NULL THEN 1
                            WHEN a.Status = 1 THEN 1
                            ELSE 0
                        END AS IsPresent,
                        ISNULL(a.Notes, '') AS Notes
                    FROM StudentSections ss
                    JOIN Students s ON ss.StudentCardNumber = s.StudentCardNumber
                    LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber 
                        AND a.ScheduleID = @ScheduleID
                        AND a.VisitDate = CAST(GETDATE() AS DATE)
                    WHERE ss.SectionID = (SELECT SectionID FROM Schedule WHERE ScheduleID = @ScheduleID)
                    AND ss.IsActive = 1
                    ORDER BY s.LastName, s.FirstName",
                    new[] { new SqlParameter("@ScheduleID", scheduleId) });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки данных: " + ex.Message;
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            try
            {
                var scheduleId = int.Parse(Request.Form["ScheduleId"]);
                var studentCardNumbers = Request.Form["StudentCardNumber"];
                var absentStudents = Request.Form["IsAbsent"];
                var notesValues = Request.Form["Notes"];

                for (int i = 0; i < studentCardNumbers.Count; i++)
                {
                    var cardNumber = studentCardNumbers[i];
                    var isAbsent = absentStudents.Contains(cardNumber);
                    var status = isAbsent ? 0 : 1;
                    var note = i < notesValues.Count ? notesValues[i] : "";

                    var exists = DBConnection.Instance.ExecuteScalar(@"
                        SELECT COUNT(*) FROM Attendance 
                        WHERE StudentCardNumber = @CardNumber 
                        AND ScheduleID = @ScheduleID 
                        AND VisitDate = CAST(GETDATE() AS DATE)",
                        new[] {
                            new SqlParameter("@CardNumber", cardNumber),
                            new SqlParameter("@ScheduleID", scheduleId)
                        });

                    if (Convert.ToInt32(exists) > 0)
                    {
                        DBConnection.Instance.ExecuteCommand(@"
                            UPDATE Attendance 
                            SET Status = @Status, Notes = @Notes
                            WHERE StudentCardNumber = @CardNumber 
                            AND ScheduleID = @ScheduleID 
                            AND VisitDate = CAST(GETDATE() AS DATE)",
                            new[] {
                                new SqlParameter("@Status", status),
                                new SqlParameter("@Notes", string.IsNullOrEmpty(note) ? (object)DBNull.Value : note),
                                new SqlParameter("@CardNumber", cardNumber),
                                new SqlParameter("@ScheduleID", scheduleId)
                            });
                    }
                    else
                    {
                        DBConnection.Instance.ExecuteCommand(@"
                            INSERT INTO Attendance (StudentCardNumber, ScheduleID, VisitDate, Status, Notes)
                            VALUES (@CardNumber, @ScheduleID, CAST(GETDATE() AS DATE), @Status, @Notes)",
                            new[] {
                                new SqlParameter("@CardNumber", cardNumber),
                                new SqlParameter("@ScheduleID", scheduleId),
                                new SqlParameter("@Status", status),
                                new SqlParameter("@Notes", string.IsNullOrEmpty(note) ? (object)DBNull.Value : note)
                            });
                    }
                }

                // ✅ Уведомление + редирект на главную
                TempData["SuccessMessage"] = "Посещаемость успешно отмечена!";
                return RedirectToPage("/Trainer/Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                return RedirectToPage();
            }
        }
    }
}