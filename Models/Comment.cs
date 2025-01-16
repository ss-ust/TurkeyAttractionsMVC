using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TurkeyAttractionsMVC.Models
{
    public class Comment
    {
        public int ID { get; set; }

        [ForeignKey("Attraction")]
        public int AttractionID { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Text { get; set; } = string.Empty;

        public virtual Attraction Attraction { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
