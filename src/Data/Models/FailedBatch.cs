using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Data.Models
{
    [Index(nameof(Timestamp))]
    public class FailedBatch
    {
        [Key]
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        [MaxLength(260)]
        public string SourceFile { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ErrorType { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        // Raw payload (JSON lines concatenated) for replay
        public string RawPayload { get; set; } = string.Empty;

        // Replay status information
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public int ReplayAttempts { get; set; }

        public DateTime? ReplayedAt { get; set; }

        public FailedBatch() { }
    }
}
