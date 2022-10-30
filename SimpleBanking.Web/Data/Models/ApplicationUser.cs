using Microsoft.AspNetCore.Identity;

namespace SimpleBanking.Web.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.TransfersIncoming = new HashSet<Transaction>();
            this.TransfersOutgoing = new HashSet<Transaction>();
        }

        public decimal Balance { get; set; } = 0.00M;

        public virtual ICollection<Transaction> TransfersIncoming { get; set; }
        public virtual ICollection<Transaction> TransfersOutgoing { get; set; }
    }
}
