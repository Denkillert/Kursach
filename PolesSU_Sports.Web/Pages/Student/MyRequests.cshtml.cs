using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PolesSU_Sports.Lib.DB;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Web.Pages.Student
{
    public class MyRequestsModel : PageModel
    {
        public DataTable Requests { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student")
                return RedirectToPage("/Index");

            var studentCard = HttpContext.Session.GetString("UserLogin");
            Requests = DBConnection.Instance.GetStudentRequests(studentCard);

            return Page();
        }
    }
}