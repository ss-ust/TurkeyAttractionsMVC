using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TurkeyAttractionsMVC.Models
{
    public class City
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public double Lat { get; set; }
        public double Lng { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public virtual ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
    }
}
