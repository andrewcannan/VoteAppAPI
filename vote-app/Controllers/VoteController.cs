using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using vote_app.Data;
using vote_app.Models;

namespace vote_app.Controllers
{
    [ApiController]
    [Route("api/vote")]
    public class VoteController : Controller
    {
        private readonly VoteAppDbContext dbContext;

        public VoteController(VoteAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddVote([FromForm] VoteFormData formData)
        {
            // Get the userId claim from the current user's ClaimsPrincipal
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");

            // Check if the userId claim is present
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Unable to determine user identity" });
            }

            // Create a new Vote object from the form data
            var newVote = new Vote
            {
                User = userId,
                YesVote = formData.YesVote,
            };

            // Add the vote to the database
            dbContext.Votes.Add(newVote);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Vote added successfully", redirect = true });
        }
    }
}

// Class to represent the form data submitted by the user
public class VoteFormData
{
    public bool YesVote { get; set; }
}
