using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Student
{
    public class SectionsModel : PageModel
    {
        public List<Section> AllSections { get; set; }
        public List<int> EnrolledSectionIds { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCardNumber = HttpContext.Session.GetString("UserLogin");

            AllSections = DBConnection.Instance.GetSections();

            // Получаем ID секций, куда уже записан студент
            EnrolledSectionIds = new List<int>();
            var enrolled = DBConnection.Instance.ExecuteQuery(
                "SELECT SectionID FROM StudentSections WHERE StudentCardNumber = @CardNumber AND IsActive = 1",
                new[] { new SqlParameter("@CardNumber", studentCardNumber) });

            foreach (System.Data.DataRow row in enrolled.Rows)
            {
                EnrolledSectionIds.Add(Convert.ToInt32(row["SectionID"]));
            }

            return Page();
        }

        public IActionResult OnPostEnroll(int sectionId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCardNumber = HttpContext.Session.GetString("UserLogin");

            try
            {
                // Проверяем, не записан ли уже
                var exists = DBConnection.Instance.ExecuteScalar(
                    "SELECT COUNT(*) FROM StudentSections WHERE StudentCardNumber = @CardNumber AND SectionID = @SectionID AND IsActive = 1",
                    new[] {
                        new SqlParameter("@CardNumber", studentCardNumber),
                        new SqlParameter("@SectionID", sectionId)
                    });

                if (Convert.ToInt32(exists) > 0)
                {
                    ErrorMessage = "❌ Вы уже записаны в эту секцию";
                    return RedirectToPage();
                }

                // Создаём запись
                DBConnection.Instance.ExecuteCommand(@"
                    INSERT INTO StudentSections (StudentCardNumber, SectionID, EnrollmentDate, IsActive)
                    VALUES (@CardNumber, @SectionID, GETDATE(), 1)",
                    new[] {
                        new SqlParameter("@CardNumber", studentCardNumber),
                        new SqlParameter("@SectionID", sectionId)
                    });

                SuccessMessage = "✅ Заявка на запись отправлена!";
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Ошибка: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}