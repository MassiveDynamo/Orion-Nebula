using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    [Index(nameof(SystemAddress))]
    [Index(nameof(SystemName))]
    public class FSSAllBodiesFound
    {
        [Key]
        public long Id { get; set; }

        // Common fields
        public DateTime Timestamp { get; set; }

        [MaxLength(200)]
        public string Event { get; set; } = string.Empty;

        [MaxLength(200)]
        public string SystemName { get; set; } = string.Empty;

        public long? SystemAddress { get; set; }

        // Number of bodies found in the scan (nullable if not provided)
        public int? BodyCount { get; set; }

        // Some journal entries provide a list of discovered bodies; store as JSON for fidelity
        public string? Bodies { get; set; }

        // Optionally include scan duration or scan type if present
        public double? ScanTime { get; set; }

        public FSSAllBodiesFound() { }
    }
}
