using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Trainer
{
    public class NotesModel : PageModel
    {
        public DataTable Notes { get; set; }
        public string TrainerName { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Content { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            LoadData();
            return Page();
        }

        public IActionResult OnPost()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Trainer")
                return RedirectToPage("/Index");

            var accountId = HttpContext.Session.GetInt32("UserID");
            var accountData = DBConnection.Instance.ExecuteQuery(
                "SELECT TrainerID FROM Accounts WHERE AccountID = @AccountID",
                new[] { new SqlParameter("@AccountID", accountId) });

            string trainerId = accountData.Rows[0]["TrainerID"].ToString();

            try
            {
                DBConnection.Instance.ExecuteCommand(@"
                    INSERT INTO TrainerNotes (TrainerID, Title, Content, NoteDate)
                    VALUES (@TrainerID, @Title, @Content, GETDATE())",
                    new[] {
                        new SqlParameter("@TrainerID", trainerId),
                        new SqlParameter("@Title", Title),
                        new SqlParameter("@Content", Content)
                    });

                TempData["SuccessMessage"] = "Заметка сохранена!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)  
        {
            try
            {
                DBConnection.Instance.ExecuteCommand(
                    "DELETE FROM TrainerNotes WHERE NoteID = @NoteID",
                    new[] { new SqlParameter("@NoteID", id) });

                TempData["SuccessMessage"] = "Заметка удалена";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
            }

            return RedirectToPage();
        }

        private void LoadData()
        {
            var accountId = HttpContext.Session.GetInt32("UserID");
            var accountData = DBConnection.Instance.ExecuteQuery(
                "SELECT TrainerID FROM Accounts WHERE AccountID = @AccountID",
                new[] { new SqlParameter("@AccountID", accountId) });

            string trainerId = accountData.Rows[0]["TrainerID"].ToString();

            var trainer = DBConnection.Instance.ExecuteQuery(
                "SELECT LastName, FirstName FROM Trainers WHERE DocumentNumber = @TrainerID",
                new[] { new SqlParameter("@TrainerID", trainerId) });

            if (trainer.Rows.Count > 0)
            {
                TrainerName = $"{trainer.Rows[0]["LastName"]} {trainer.Rows[0]["FirstName"]}";
            }

            Notes = DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    NoteID,
                    Title,
                    Content,
                    NoteDate,
                    CreatedDate
                FROM TrainerNotes
                WHERE TrainerID = @TrainerID
                ORDER BY CreatedDate DESC",
                new[] { new SqlParameter("@TrainerID", trainerId) });
        }
    }
}