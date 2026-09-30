using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class OrionNebula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EDSystemName",
                table: "EDSystemName");

            migrationBuilder.DropColumn(
                name: "Id64",
                table: "EDSystemName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EDSystemName",
                table: "EDSystemName",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EDSystemName",
                table: "EDSystemName");

            migrationBuilder.AddColumn<long>(
                name: "Id64",
                table: "EDSystemName",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EDSystemName",
                table: "EDSystemName",
                column: "Id64");
        }
    }
}
