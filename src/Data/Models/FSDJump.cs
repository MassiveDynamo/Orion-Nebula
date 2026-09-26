using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Data.Models
{
    // Index StarSystem and SystemAddress as requested
    [Index(nameof(SystemAddress))]
    [Index(nameof(StarSystem))]
    public class FSDJump
    {
        [Key]
        public long Id { get; set; }

        // Common fields
        public DateTime Timestamp { get; set; }

        [MaxLength(200)]
        public string Event { get; set; } = string.Empty;

        [MaxLength(200)]
        public string StarSystem { get; set; } = string.Empty;

        // SystemAddress can be null for unknown systems
        public long? SystemAddress { get; set; }

        // Star position (nullable when not provided)
        public double? StarPosX { get; set; }
        public double? StarPosY { get; set; }
        public double? StarPosZ { get; set; }

        // System metadata (from journal when available)
        [MaxLength(100)]
        public string? SystemAllegiance { get; set; }

        [MaxLength(100)]
        public string? SystemEconomy { get; set; }

        [MaxLength(100)]
        public string? SystemGovernment { get; set; }

        [MaxLength(100)]
        public string? SystemSecurity { get; set; }

        // Population may be absent; use long?
        public long? Population { get; set; }

        // Faction and its state when present
        [MaxLength(200)]
        public string? Faction { get; set; }

        [MaxLength(100)]
        public string? FactionState { get; set; }

        // Conflicts is a complex array in the journal; store raw JSON fragment
        public string? Conflicts { get; set; }

        // Powerplay related
        [MaxLength(100)]
        public string? Powers { get; set; }

        [MaxLength(100)]
        public string? PowerplayState { get; set; }

        public int? ReserveLevel { get; set; }

        public bool? NeedsPermit { get; set; }

        // Jump details
        public double? JumpDist { get; set; }
        public double? FuelUsed { get; set; }
        public double? FuelLevel { get; set; }

        [MaxLength(50)]
        public string? StarClass { get; set; }

        // Store raw JSON payload for fidelity
        public string RawJson { get; set; } = string.Empty;

        // Parameterless ctor
        public FSDJump() { }
    }
}
