using Voyager;
using Xunit;

namespace Voyager.Tests;

public class ReportGeneratorTests
{
    [Fact]
    public void CountBeltClusters_CountsBeltClusterBodies()
    {
        var json = """
        [
          { "bodyType": "Star" },
          { "BodyType": "BeltCluster" },
          { "type": "BeltCluster" },
          { "Type": "beltcluster" },
          { "BodyType": "Planet" }
        ]
        """;

        Assert.Equal(3, ReportGenerator.CountBeltClusters(json));
    }
}
