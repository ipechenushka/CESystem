using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using CESystem.ClientPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CESystem.Controllers
{
    [Route("")] 
    public class UserController : Controller
    {
        private readonly LocalDbContext _db;
        private readonly ILogger _log;
        private readonly IUserService _userService;
        
        public UserController(LocalDbContext dbContext, ILogger<Startup> logger, IUserService userService)
        {
            _db = dbContext;
            _log = logger;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Home()
        {
            return Content("Welcome to CESystem!");
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login(string name, string password)
        {
            if (name == null || password == null) 
                return BadRequest("Incorrect requests parameters");
            
            var loginUser = await _userService.FindUserByNameAsync(name);
            
            if (loginUser == null)
                return NotFound("User not Found");

            var accessStatus = Crypto.VerifyHashedPassword(loginUser.PasswordHash, password + loginUser.PasswordSalt);

            if (!accessStatus)
                return StatusCode(403, "Access denied");
            
            await Authenticate(loginUser);

            _log.LogInformation($"{loginUser.Name} has entered in server");
            
            return loginUser.Role.Equals("admin")
                ? Redirect("/admin/home")
                : Redirect($"/account/{loginUser.CurrentAccount}");
        }

        [HttpGet, Route("login")]
        public IActionResult Login()
        {
            return Content("Login Page");
        }

        [HttpPost, Route("registration")]
        public async Task<IActionResult> Registration(string name, string password)
        {
            if (name == null || password == null) 
                return BadRequest("Incorrect requests parameters");

            var newUser = await _userService.FindUserByNameAsync(name);
            
            if (newUser == null)
            {
                var passwordSalt = Crypto.GenerateSalt();
                var passwordHash = Crypto.HashPassword(password + passwordSalt);
                
                newUser = new UserRecord
                {
                    Name = name, 
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    CreatedDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")
                };

                await _db.UserRecords.AddAsync(newUser);
                await _db.SaveChangesAsync();

                var newAccount = new AccountRecord { UserId = newUser.Id };
                
                await _db.AccountRecords.AddAsync(newAccount);
                await _db.SaveChangesAsync();

                newUser.CurrentAccount = newAccount.Id;
                
                await Authenticate(newUser);
                await _db.SaveChangesAsync();
                
                _log.LogInformation($"{newUser.Name} has registered in server");
                
                return Redirect($"/account/{newAccount.Id}");
            }
            
            return BadRequest("User with that name is already exist");
        }


        [HttpGet, Route("registration")]
        public IActionResult Registration()
        {
            return Content("Registration Page");
        }

        private async Task Authenticate(UserRecord user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role),
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", 
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext
                .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}
