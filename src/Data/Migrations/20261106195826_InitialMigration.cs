using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EDSystemName",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDSystemName", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "FSDJump",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StarSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemAddress = table.Column<long>(type: "bigint", nullable: true),
                    StarPosX = table.Column<double>(type: "float", nullable: true),
                    StarPosY = table.Column<double>(type: "float", nullable: true),
                    StarPosZ = table.Column<double>(type: "float", nullable: true),
                    SystemAllegiance = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SystemEconomy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SystemGovernment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SystemSecurity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Population = table.Column<long>(type: "bigint", nullable: true),
                    Faction = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FactionState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Conflicts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Powers = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PowerplayState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReserveLevel = table.Column<int>(type: "int", nullable: true),
                    NeedsPermit = table.Column<bool>(type: "bit", nullable: true),
                    JumpDist = table.Column<double>(type: "float", nullable: true),
                    FuelUsed = table.Column<double>(type: "float", nullable: true),
                    FuelLevel = table.Column<double>(type: "float", nullable: true),
                    StarClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FSDJump", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FSSAllBodiesFound",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemAddress = table.Column<long>(type: "bigint", nullable: true),
                    BodyCount = table.Column<int>(type: "int", nullable: true),
                    Bodies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScanTime = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FSSAllBodiesFound", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiningRefined",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type_Localised = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiningRefined", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProspectedAsteroid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content_Localised = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherlodeMaterial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remaining = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProspectedAsteroid", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StarSystem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false),
                    Z = table.Column<double>(type: "float", nullable: false),
                    Population = table.Column<long>(type: "bigint", nullable: true),
                    BodyCount = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarSystem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupercruiseExit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StarSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemAddress = table.Column<long>(type: "bigint", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BodyID = table.Column<int>(type: "int", nullable: true),
                    BodyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Taxi = table.Column<bool>(type: "bit", nullable: true),
                    Multicrew = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupercruiseExit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FSDJump_StarSystem",
                table: "FSDJump",
                column: "StarSystem");

            migrationBuilder.CreateIndex(
                name: "IX_FSDJump_SystemAddress",
                table: "FSDJump",
                column: "SystemAddress");

            migrationBuilder.CreateIndex(
                name: "IX_FSSAllBodiesFound_SystemAddress",
                table: "FSSAllBodiesFound",
                column: "SystemAddress");

            migrationBuilder.CreateIndex(
                name: "IX_FSSAllBodiesFound_SystemName",
                table: "FSSAllBodiesFound",
                column: "SystemName");

            migrationBuilder.CreateIndex(
                name: "IX_StarSystem_Name",
                table: "StarSystem",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SupercruiseExit_StarSystem",
                table: "SupercruiseExit",
                column: "StarSystem");

            migrationBuilder.CreateIndex(
                name: "IX_SupercruiseExit_SystemAddress",
                table: "SupercruiseExit",
                column: "SystemAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EDSystemName");

            migrationBuilder.DropTable(
                name: "FSDJump");

            migrationBuilder.DropTable(
                name: "FSSAllBodiesFound");

            migrationBuilder.DropTable(
                name: "MiningRefined");

            migrationBuilder.DropTable(
                name: "ProspectedAsteroid");

            migrationBuilder.DropTable(
                name: "StarSystem");

            migrationBuilder.DropTable(
                name: "SupercruiseExit");
        }
    }
}
