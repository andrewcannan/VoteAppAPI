using Microsoft.EntityFrameworkCore;
using vote_app.Models;

namespace vote_app.Data
{
    public class VoteAppDbContext : DbContext
    {
        public VoteAppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vote> Votes { get; set; }
    }
}