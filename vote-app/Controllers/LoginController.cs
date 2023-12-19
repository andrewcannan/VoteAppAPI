using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vote_app.Data;

namespace vote_app.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : Controller
    {
        private readonly VoteAppDbContext dbContext;
        private readonly IConfiguration config;

        // Constructor for dependency injection
        public LoginController(VoteAppDbContext dbContext, IConfiguration config)
        {
            this.dbContext = dbContext;
            this.config = config;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginFormData loginFormData)
        {
            // Validate the incoming login request
            if (string.IsNullOrEmpty(loginFormData.Email) || string.IsNullOrEmpty(loginFormData.Password))
            {
                return BadRequest(new { message = "Invalid login request" });
            }

            // Retrieve the user from the database based on the provided email
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == loginFormData.Email);

            // Check if the user exists and the password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginFormData.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // If username and password are correct then proceed to generate token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // Set isAdmin to false as standard
            var isAdmin = false;

            if (user.Email == "admin@admin.com")
            {
                isAdmin = true;
            }

            // add user id and isAdmin val to token
            var claims = new List<Claim>
            {
                 new Claim("userId", user.UserId.ToString()),
                 new Claim("admin", isAdmin.ToString())
            };

            var SecurityToken = new JwtSecurityToken(
                 config["Jwt:Issuer"],
                 config["Jwt:Issuer"],
                 claims,
                 expires: DateTime.Now.AddMinutes(60),
                 signingCredentials: credentials
             );


            var token = new JwtSecurityTokenHandler().WriteToken(SecurityToken);

            return Ok(new { message = "Login Successful", redirect = true, token });
        }
    }
}

// Class to represent the form data submitted by the user
public class LoginFormData
{
    public string Email { get; set; }
    public string Password { get; set; }
}
