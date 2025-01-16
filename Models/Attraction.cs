using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurkeyAttractionsMVC.Models
{
    public class Attraction
    {
        public int ID { get; set; }

        [ForeignKey("City")]
        public int CityID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public double Lat { get; set; }
        public double Lng { get; set; }

        public string Description { get; set; } = string.Empty;

        public virtual City City { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<UserAttraction> UserAttractions { get; set; } = new List<UserAttraction>();
    }
}
