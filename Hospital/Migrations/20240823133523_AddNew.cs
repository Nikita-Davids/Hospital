using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class AddNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Surgeon",
                table: "Surgeon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pharmacist",
                table: "Pharmacist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Nurse",
                table: "Nurse");

            migrationBuilder.RenameTable(
                name: "Surgeon",
                newName: "Surgeons");

            migrationBuilder.RenameTable(
                name: "Pharmacist",
                newName: "Pharmacists");

            migrationBuilder.RenameTable(
                name: "Nurse",
                newName: "Nurses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Surgeons",
                table: "Surgeons",
                column: "SurgeonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pharmacists",
                table: "Pharmacists",
                column: "PharmacistId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Nurses",
                table: "Nurses",
                column: "NurseID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Surgeons",
                table: "Surgeons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pharmacists",
                table: "Pharmacists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Nurses",
                table: "Nurses");

            migrationBuilder.RenameTable(
                name: "Surgeons",
                newName: "Surgeon");

            migrationBuilder.RenameTable(
                name: "Pharmacists",
                newName: "Pharmacist");

            migrationBuilder.RenameTable(
                name: "Nurses",
                newName: "Nurse");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Surgeon",
                table: "Surgeon",
                column: "SurgeonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pharmacist",
                table: "Pharmacist",
                column: "PharmacistId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Nurse",
                table: "Nurse",
                column: "NurseID");
        }
    }
}
