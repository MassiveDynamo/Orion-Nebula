using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class NoRaw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawJson",
                table: "FSSAllBodiesFound");

            migrationBuilder.DropColumn(
                name: "RawJson",
                table: "FSDJump");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawJson",
                table: "FSSAllBodiesFound",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawJson",
                table: "FSDJump",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
