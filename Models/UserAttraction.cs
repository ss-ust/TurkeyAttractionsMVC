using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TurkeyAttractionsMVC.Models
{
    public class UserAttraction
    {
        public int ID { get; set; }

        [ForeignKey("Attraction")]
        public int AttractionId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public bool Visited { get; set; }

        public virtual Attraction Attraction { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
