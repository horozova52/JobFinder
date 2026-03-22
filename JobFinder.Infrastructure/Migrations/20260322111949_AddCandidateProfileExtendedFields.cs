using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateProfileExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                table: "Experiences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: true);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Experiences");

         
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CandidateProfiles");


                  }
    }
}
