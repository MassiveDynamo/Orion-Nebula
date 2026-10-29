using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEvents1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MiningRefined",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    Event = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                name: "MiningRefined");

            migrationBuilder.DropTable(
                name: "ProspectedAsteroid");

            migrationBuilder.DropTable(
                name: "SupercruiseExit");
        }
    }
}
