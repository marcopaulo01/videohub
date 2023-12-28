using _301236921_Garcia_Lab3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _301236921_Garcia_Lab3.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserDbContext _userDbContext;

        public LoginController (UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            //match the username & password to RDS
            var users = _userDbContext.Users.Where(u => u.Username == username && u.Password == password).ToList();

            if(users.Any())
            {
                var user = users.First();
                Guid userId = user.Id;

                return RedirectToAction("Index", "Video", new { userId = userId });
            }
           
            else
            {
                ViewData["ErrorMessage"] = "Invalid username or password";
                return View();
            }
        }
    }
}
