using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class MiningRefined
    {
        [Key]
        public long Id { get; set; }

        // Common fields
        public DateTime Timestamp { get; set; }

        [MaxLength(50)]
        public string Event { get; set; } = string.Empty;

        // Material type being refined (internal name)
        [MaxLength(100)]
        public string? Type { get; set; }

        // Localized material name for display
        [MaxLength(100)]
        public string? Type_Localised { get; set; }

        public MiningRefined() { }
    }
}
