using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class ProspectedAsteroid
    {
        [Key]
        public long Id { get; set; }

        // Common fields
        public DateTime Timestamp { get; set; }

        [MaxLength(50)]
        public string Event { get; set; } = string.Empty;

        // Asteroid material content level
        [MaxLength(100)]
        public string? Content { get; set; }

        [MaxLength(100)]
        public string? Content_Localised { get; set; }

        // Materials found in this asteroid (stored as JSON array)
        public string? Materials { get; set; }

        // Special material if this is a motherlode
        [MaxLength(100)]
        public string? MotherlodeMaterial { get; set; }

        // Remaining resources in the asteroid as percentage
        public double? Remaining { get; set; }

        public ProspectedAsteroid() { }
    }
}
