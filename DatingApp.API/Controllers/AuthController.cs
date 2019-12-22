using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repo;
        private readonly IConfiguration config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            this.config = config;
            this.repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // using this validation we must to delete [ApiController] and add [FromBody]UserForRegisterDto annotations in this method
            //if(!ModelState.IsValid)
            //return BadRequest(ModelState);

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await repo.UserExist(userForRegisterDto.Username))
                return BadRequest("Username already exist!");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await repo.Register(userToCreate, userForRegisterDto.Password);

            // TODO : create CreatedAtRoute navigate to specific url after user created
            return StatusCode(201);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await repo.Login(userForLoginDto.Username.ToLower(),userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized(); // לא מורשה להיכנס

            var claims = new[] // להגדיר פרמטרים הכרה של המשתמש
            {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()), // מס' מזהה
                new Claim(ClaimTypes.Name, userFromRepo.Username) // שם משתמש
            };

            // from appsettings.json minimum 12 characters
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("AppSettings:Token").Value)); // הגדרת מפתח בקובץ json AppSettings.json

            //signing credentials
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature); // אישורי כניסה באמצעות המפתח שהוגדר לעיל והאלגוריתם

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // מס' ייחודי ושם משתמש
                Expires = DateTime.Now.AddDays(1), // תוקף האבטחה ליום אחד
                SigningCredentials = creds // הגדרת האישור סוג
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok( new { // ה טוקן חוזר ללקוח כשההתחברות הצליחה
                token = tokenHandler.WriteToken(token)
            });

        }
    }
}