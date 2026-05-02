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
        public List<int> PendingRequestSectionIds { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCard = HttpContext.Session.GetString("UserLogin");

            AllSections = DBConnection.Instance.GetSections();

            // Получаем секции, где студент уже записан
            var enrolled = DBConnection.Instance.ExecuteQuery(@"
                SELECT SectionID FROM StudentSections 
                WHERE StudentCardNumber = @StudentCard AND IsActive = 1",
                new[] { new SqlParameter("@StudentCard", studentCard) });

            EnrolledSectionIds = new List<int>();
            foreach (System.Data.DataRow row in enrolled.Rows)
            {
                EnrolledSectionIds.Add(Convert.ToInt32(row["SectionID"]));
            }

            // Получаем секции, куда есть pending заявки
            var pending = DBConnection.Instance.ExecuteQuery(@"
                SELECT SectionID FROM StudentSectionRequests 
                WHERE StudentCardNumber = @StudentCard AND Status = 'Pending' AND RequestType = 'Join'",
                new[] { new SqlParameter("@StudentCard", studentCard) });

            PendingRequestSectionIds = new List<int>();
            foreach (System.Data.DataRow row in pending.Rows)
            {
                PendingRequestSectionIds.Add(Convert.ToInt32(row["SectionID"]));
            }

            return Page();
        }

        public IActionResult OnPostRequestJoin(int sectionId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCard = HttpContext.Session.GetString("UserLogin");

            try
            {
                // Проверяем, не записан ли уже
                if (DBConnection.Instance.IsEnrolledInSection(studentCard, sectionId))
                {
                    ErrorMessage = "Вы уже записаны в эту секцию";
                    return RedirectToPage();
                }

                // Проверяем, нет ли уже pending заявки
                if (DBConnection.Instance.HasPendingRequest(studentCard, sectionId, "Join"))
                {
                    ErrorMessage = "У вас уже есть заявка в эту секцию";
                    return RedirectToPage();
                }

                // Создаём заявку
                DBConnection.Instance.CreateSectionRequest(studentCard, sectionId, "Join");
                SuccessMessage = "✅ Заявка отправлена! Менеджер рассмотрит её в ближайшее время.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Ошибка: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}