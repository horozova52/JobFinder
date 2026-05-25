using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployerProfileId",
                table: "Experiences",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_EmployerProfileId",
                table: "Experiences",
                column: "EmployerProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiences_EmployerProfiles_EmployerProfileId",
                table: "Experiences",
                column: "EmployerProfileId",
                principalTable: "EmployerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Experiences_EmployerProfiles_EmployerProfileId",
                table: "Experiences");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_EmployerProfileId",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "EmployerProfileId",
                table: "Experiences");
        }
    }
}
