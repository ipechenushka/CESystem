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
        private readonly ILogger _log;
        private readonly IUserService _userService;
        private readonly LocalDbContext _db;
        public UserController(ILogger<Startup> logger, IUserService userService, LocalDbContext localDbContext)
        {
            _db = localDbContext;
            _log = logger;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult HomePage() => Content("Welcome to CESystem!");
     
        [HttpGet, Route("registration")]
        public IActionResult RegistrationPage() => Content("Registration Page");
        
        [HttpGet, Route("login")]
        public IActionResult LoginPage() => Content("Login Page");

        
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
                return Forbid();
            
            await Authenticate(loginUser);

            _log.LogInformation($"{loginUser.Name} has entered in server");
            
            return loginUser.Role.Equals("admin")
                ? Redirect("/admin/home")
                : Redirect($"/account/{loginUser.CurrentAccount}");
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

                await _userService.AddUserAsync(newUser);
                await _db.SaveChangesAsync();
                
                var newAccount = new AccountRecord { UserId = newUser.Id };

                await _userService.AddAccountAsync(newAccount);
                await _db.SaveChangesAsync();

                newUser.CurrentAccount = newAccount.Id;
                
                await Authenticate(newUser);
                await _db.SaveChangesAsync();
                
                _log.LogInformation($"{newUser.Name} has registered in server");
                
                return Redirect($"/account/{newAccount.Id}");
            }
            
            return BadRequest("User with that name is already exist");
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
