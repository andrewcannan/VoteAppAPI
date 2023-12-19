using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vote_app.Data;
using vote_app.Models;

namespace vote_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class UsersAdminController : ControllerBase
    {
        private readonly VoteAppDbContext _context;

        public UsersAdminController(VoteAppDbContext context)
        {
            _context = context;
        }

        // GET: api/UsersAdmin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserData>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userDataObjects = users.Select(user => new UserData(user));
            return Ok(userDataObjects);
        }

        // GET: api/UsersAdmin/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserData>> GetUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var userDataObject = new UserData(user);
            return Ok(userDataObject);
        }

        // PUT: api/UsersAdmin/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{userId}")]
        public async Task<IActionResult> PutUser(int userId, [FromForm] UpdateUserFormData formData)
        {
            if (userId != formData.UserId)
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Update user properties with form data
            user.Email = formData.Email;
            user.FirstName = formData.FirstName;
            user.Surname = formData.Surname;
            user.DateOfBirth = formData.DateOfBirth;
            user.Longitude = formData.Longitude;
            user.Latitude = formData.Latitude;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(userId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new {message="User updated successfully"});
        }


        // DELETE: api/UsersAdmin/5
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int userId)
        {
            return _context.Users.Any(e => e.UserId == userId);
        }
    }
}

// Class to represent the UserData to be sent in Get method
public class UserData
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    // Constructor to map from User entity to UserData
    public UserData(User user)
    {
        UserId = user.UserId;
        Email = user.Email;
        FirstName = user.FirstName;
        Surname = user.Surname;
        DateOfBirth = user.DateOfBirth;
        Longitude = user.Longitude;
        Latitude = user.Latitude;
    }
}

// Class to represent formdata sent from client
public class UpdateUserFormData
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}