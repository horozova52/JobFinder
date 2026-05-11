using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RejectionFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "RejectionFeedback",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionFeedback",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
