using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class StarSystem
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public long? Population { get; set; }

        public int BodyCount { get; set; }

        // Common fields
        public DateTime Date { get; set; }
    }
}
