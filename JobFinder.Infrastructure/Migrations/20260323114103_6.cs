using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BenefitsJson",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalCode",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoundedYear",
                table: "EmployerProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mission",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecruitmentProcessJson",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Values",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vision",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkEnvironment",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenefitsJson",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "FiscalCode",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "FoundedYear",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "Mission",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "RecruitmentProcessJson",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "Values",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "Vision",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "WorkEnvironment",
                table: "EmployerProfiles");
        }
    }
}
