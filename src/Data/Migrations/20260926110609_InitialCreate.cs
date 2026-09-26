using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EDSystemName",
                columns: table => new
                {
                    Id64 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDSystemName", x => x.Id64);
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
                    StarClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    StarSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemAddress = table.Column<long>(type: "bigint", nullable: true),
                    BodyCount = table.Column<int>(type: "int", nullable: true),
                    Bodies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScanTime = table.Column<double>(type: "float", nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FSSAllBodiesFound", x => x.Id);
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
                name: "IX_FSSAllBodiesFound_StarSystem",
                table: "FSSAllBodiesFound",
                column: "StarSystem");

            migrationBuilder.CreateIndex(
                name: "IX_FSSAllBodiesFound_SystemAddress",
                table: "FSSAllBodiesFound",
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
        }
    }
}
