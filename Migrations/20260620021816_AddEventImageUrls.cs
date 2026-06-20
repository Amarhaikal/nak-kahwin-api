using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nak_kahwin_api.Migrations
{
    /// <inheritdoc />
    public partial class AddEventImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EngagementImageUrl",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarriageImageUrl",
                table: "Events",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngagementImageUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MarriageImageUrl",
                table: "Events");
        }
    }
}
