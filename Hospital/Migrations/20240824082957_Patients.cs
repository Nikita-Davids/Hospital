using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class Patients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientIDNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatientContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientEmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatientDateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientGender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientIDNumber);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
