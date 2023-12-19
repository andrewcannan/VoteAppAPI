using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vote_app.Models
{
    public class Vote
    {
        [Key]
        public int VoteId { get; set; }

        [ForeignKey("UserId")]
        public int User { get; set; }

        [Required]
        public bool YesVote { get; set; }
    }
}
