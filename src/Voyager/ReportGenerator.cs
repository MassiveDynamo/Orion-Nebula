using Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;

namespace Voyager
{
    internal class ReportGenerator
    {
        /// <summary>
        /// Creates a report based on the EDSystemName (Orion Nebula systems) and the FSDJump data in the OrionDbContext. The report is saved to the specified data folder.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dataFolder"></param>
        internal static void GenerateReport(ILogger logger, OrionDbContext dbContext, string dataFolder)
        {
            var orionSystemCount = dbContext.EDSystemName.Count();
            logger.Information("Found {Count} Orion Nebula systems in the database.", orionSystemCount);

            // Select the FSDJump records that match the Orion Nebula systems using a join
            var query = dbContext.FSDJump
                .Where(jump => dbContext.EDSystemName.Any(system => system.Name == jump.StarSystem))
                .GroupBy(jump => jump.StarSystem)
                .Select(group => new
                {
                    StarSystem = group.Key,
                    JumpCount = group.Count(),
                    LatestDate = group.Max(jump => jump.Timestamp),
                    EarliestDate = group.Min(jump => jump.Timestamp)
                })
                .ToList();


            logger.Information("Found {Count} FSD jumps for Orion Nebula systems.", query.Sum(x => x.JumpCount));
            var results = query.ToList();

            // compute overall stats
            var totalSystems = dbContext.EDSystemName.Count();
            var visitedSystems = results.Count;
            var totalJumps = results.Sum(r => r.JumpCount);
            var earliestJump = results.Count > 0 ? results.Min(r => r.EarliestDate) : (DateTime?)null;
            var latestJump = results.Count > 0 ? results.Max(r => r.LatestDate) : (DateTime?)null;
            var percentComplete = totalSystems > 0 ? (double)visitedSystems / totalSystems * 100.0 : 0.0;
            var oldestJump = results.OrderBy(r => r.EarliestDate).Take(10).Select(r => $"<tr><td>{r.StarSystem}</td><td>{r.JumpCount}</td><td>{r.EarliestDate:yyyy-MM-dd HH:mm:ss}</td><td>{r.LatestDate:yyyy-MM-dd HH:mm:ss}</td></tr>");

            // Load the template and replace the placeholders with the actual values from the query results.
            var sb = new StringBuilder();
            sb.Append(File.ReadAllText("Template.html"));
            sb.Replace("{{TOTAL_SYSTEMS}}", totalSystems.ToString());
            sb.Replace("{{VISITED_SYSTEMS}}", visitedSystems.ToString());
            sb.Replace("{{TOTAL_JUMPS}}", totalJumps.ToString());
            sb.Replace("{{EARLIEST_JUMP}}", earliestJump?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
            sb.Replace("{{LATEST_JUMP}}", latestJump?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
            sb.Replace("{{PERCENT_COMPLETE}}", percentComplete.ToString("F2") + "%");
            sb.Replace("{{PERCENT_VALUE}}", percentComplete.ToString("F2"));
            sb.Replace("{{SYSTEM_ROWS}}", string.Join("", oldestJump));
            sb.Replace("{{GENERATED_AT}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            // write file
            var outPath = Path.Combine(dataFolder, "OrionReport.html");
            File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
            logger.Information("Wrote report to {ReportPath}", outPath);
        }
    }
}
