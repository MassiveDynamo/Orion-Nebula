using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Data.Models
{
    [Index(nameof(SystemAddress))]
    [Index(nameof(StarSystem))]
    public class SupercruiseExit
    {
        [Key]
        public long Id { get; set; }

        // Common fields
        public DateTime Timestamp { get; set; }

        [MaxLength(200)]
        public string Event { get; set; } = string.Empty;

        [MaxLength(200)]
        public string StarSystem { get; set; } = string.Empty;

        // SystemAddress identifies the system
        public long? SystemAddress { get; set; }

        // Body being exited to
        [MaxLength(200)]
        public string Body { get; set; } = string.Empty;

        // Body identifier
        public int? BodyID { get; set; }

        [MaxLength(50)]
        public string? BodyType { get; set; }

        // Taxi and Multicrew flags
        public bool? Taxi { get; set; }

        public bool? Multicrew { get; set; }

        public SupercruiseExit() { }
    }
}
