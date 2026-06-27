using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nak_kahwin_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionToSavingEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "SavingEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "SavingEntries");
        }
    }
}
