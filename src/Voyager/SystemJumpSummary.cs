namespace Voyager
{
    internal class SystemJumpSummary
    {
        public string StarSystem { get; set; }
        public int JumpCount { get; set; }
        public DateTime LastJump { get; set; }

        public SystemJumpSummary(string starSystem, int jumpCount = 0, DateTime? lastJump = null)
        {
            StarSystem = starSystem;
            JumpCount = jumpCount;
            LastJump = lastJump ?? DateTime.MinValue;
        }
    }
}