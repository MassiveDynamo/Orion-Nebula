using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class SystemName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StarSystem",
                table: "FSSAllBodiesFound",
                newName: "SystemName");

            migrationBuilder.RenameIndex(
                name: "IX_FSSAllBodiesFound_StarSystem",
                table: "FSSAllBodiesFound",
                newName: "IX_FSSAllBodiesFound_SystemName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SystemName",
                table: "FSSAllBodiesFound",
                newName: "StarSystem");

            migrationBuilder.RenameIndex(
                name: "IX_FSSAllBodiesFound_SystemName",
                table: "FSSAllBodiesFound",
                newName: "IX_FSSAllBodiesFound_StarSystem");
        }
    }
}
