using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CESystem.AdminPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CESystem.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly LocalDbContext _db;
        private readonly ILogger _log;
        
        public AccountController(LocalDbContext dbContext, ILogger<Startup> logger)
        {
            _db = dbContext;
            _log = logger;
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            return Content("home page");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string name, string password)
        {
            if (name == null || password == null) return BadRequest("Incorrect requests parameters");

            var passwordHash = HashPassword(password);
            User loginUser = await _db.Users.FirstOrDefaultAsync(x => x.Name == name && x.Password == passwordHash);

            if (loginUser == null)
                return StatusCode(403, "User not Found");
            
            //_log.LogInformation($"{loginUser.Id}");
            await Authenticate(loginUser);
            
            return Redirect("/account/user/home");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Content("login page");
        }
        
        [HttpGet("need_login")]
        public IActionResult NeedLogin()
        {
            return Content("login please");
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Registration(string name, string password)
        {
            User user = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);
            
            if (user == null)
            {
                var passwordHash = HashPassword(password);
                user = new User {Name = name, Password = passwordHash, CreatedDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")};
                
                await _db.Users.AddAsync(user);
                await _db.Accounts.AddAsync(new Account {UserId = user.Id, User = user});
                await _db.SaveChangesAsync();
                
                await Authenticate(user); 
                
                return Redirect("/account/user/home");
            }
            
            return BadRequest("User with that username is already exist");
        }


        [HttpGet("registration")]
        public IActionResult Registration()
        {
            return Content("Registration Page");
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        private string HashPassword(string password)
        {
           return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
