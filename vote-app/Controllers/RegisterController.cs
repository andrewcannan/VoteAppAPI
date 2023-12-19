using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vote_app.Data;
using vote_app.Models;

namespace vote_app.Controllers
{
    [ApiController]
    [Route("api/register")]
    public class RegisterController : Controller
    {
        private readonly VoteAppDbContext dbContext;
        private readonly IHttpClientFactory httpClientFactory;

        // Constructor for dependency injection
        public RegisterController(VoteAppDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            this.dbContext = dbContext;
            this.httpClientFactory = httpClientFactory;
        }


        [HttpPost]
        public async Task<IActionResult> AddUserAsync([FromForm] RegisterFormData formData)
        {
            // Check if a user with the same email already exists in the database
            var existingUser = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == formData.Email);

            if (existingUser != null)
            {
                // User with the same email already exists, handle accordingly
                return BadRequest(new { message = "User with the provided email already exists" });
            }

            // Check password complexity rules
            if (!IsPasswordValid(formData.Password))
            {
                return BadRequest(new { message = "Password does not meet complexity requirements" });
            }

            else
            {
                // Create a new User object from the form data
                var newUser = new User
                {
                    Email = formData.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(formData.Password),
                    FirstName = formData.FirstName,
                    Surname = formData.Surname,
                    DateOfBirth = formData.DateOfBirth,
                };

                // Make an API request to Postcodes.io to get longitude and latitude
                var postcodeApiUrl = $"https://api.postcodes.io/postcodes/{formData.Postcode}";
                var httpClient = httpClientFactory.CreateClient();

                try
                {
                    var response = await httpClient.GetAsync(postcodeApiUrl);
                    response.EnsureSuccessStatusCode();

                    var apiResult = await response.Content.ReadFromJsonAsync<PostcodeApiResponse>();
                    // Set the longitude and latitude properties of the new user object
                    newUser.Longitude = apiResult.result.Longitude;
                    newUser.Latitude = apiResult.result.Latitude;
                }
                catch (HttpRequestException)
                {
                    return BadRequest(new { message = "Error retrieving longitude and latitude from Postcodes.io" });

                }

                // Add the user to the database
                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "User added successfully", redirect = true });
            }
        }

        // Password complexity rules, require a minimum length of 8, at least one uppercase letter, one lowercase letter, and one digit
        private static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            {
                return false;
            }
            return true;
        }
    }

    // Internal class to represent the structure of the API response from Postcodes.io
    internal class PostcodeApiResponse
    {
        public Result result { get; set; }

        public class Result
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }
    }

    // Class to represent the form data submitted by the user
    public class RegisterFormData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Postcode { get; set; }
    }
}