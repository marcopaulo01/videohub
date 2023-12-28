using _301236921_Garcia_Lab3.Data;
using _301236921_Garcia_Lab3.Models;
using Microsoft.AspNetCore.Mvc;

namespace _301236921_Garcia_Lab3.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserDbContext _userDbContext;

        public RegisterController(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(User UserRequest)
        {
            var user = new User
            {
                Username = UserRequest.Username,
                Password = UserRequest.Password,
            };

            _userDbContext.Users.Add(user);
            _userDbContext.SaveChanges();

            Guid userId = user.Id;
            return RedirectToAction("Index", "Video", new { userId = userId });

        }
    }
}
