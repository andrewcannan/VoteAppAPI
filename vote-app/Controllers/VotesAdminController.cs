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
    public class VotesAdminController : ControllerBase
    {
        private readonly VoteAppDbContext _context;

        public VotesAdminController(VoteAppDbContext context)
        {
            _context = context;
        }

        // GET: api/VotesAdmin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vote>>> GetVotes()
        {
            return await _context.Votes.ToListAsync();
        }

        // GET: api/VotesAdmin/5
        [HttpGet("{voteId}")]
        public async Task<ActionResult<Vote>> GetVote(int voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);

            if (vote == null)
            {
                return NotFound();
            }

            return vote;
        }

        // PUT: api/VotesAdmin/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{voteId}")]
        public async Task<IActionResult> PutVote(int voteId, [FromForm] UpdateVoteFormData formData)
        {
            var vote = await _context.Votes.FindAsync(voteId);

            if (vote == null)
            {
                return NotFound();
            }

            // Update only the yesVote property with form data
            vote.YesVote = formData.YesVote;

            _context.Entry(vote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoteExists(voteId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new {message = "Vote updated successfully"});
        }

        // DELETE: api/VotesAdmin/5
        [HttpDelete("{voteId}")]
        public async Task<IActionResult> DeleteVote(int voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote == null)
            {
                return NotFound();
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoteExists(int voteId)
        {
            return _context.Votes.Any(e => e.VoteId == voteId);
        }
    }
}

// Class to represent formdata sent from the client
public class UpdateVoteFormData
{
    public bool YesVote { get; set; }
}
