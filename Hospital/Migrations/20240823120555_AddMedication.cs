using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class AddMedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            

            migrationBuilder.CreateTable(
                name: "Medication",
                columns: table => new
                {
                    MedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosageForm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReOrderLevel = table.Column<int>(type: "int", nullable: false),
                    MedicationActiveIngredients = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medication", x => x.MedicationId);
                });

            migrationBuilder.CreateTable(
                name: "Nurse",
                columns: table => new
                {
                    NurseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HealthCouncilRegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nurse", x => x.NurseID);
                });

            migrationBuilder.CreateTable(
                name: "Pharmacist",
                columns: table => new
                {
                    PharmacistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HealthCouncilRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharmacist", x => x.PharmacistId);
                });

            migrationBuilder.CreateTable(
                name: "Surgeon",
                columns: table => new
                {
                    SurgeonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HealthCouncilRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surgeon", x => x.SurgeonId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medication");

            migrationBuilder.DropTable(
                name: "Nurse");

            migrationBuilder.DropTable(
                name: "Pharmacist");

            migrationBuilder.DropTable(
                name: "Surgeon");

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    MedicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DosageForm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReOrderLevel = table.Column<int>(type: "int", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.MedicationID);
                });
        }
    }
}
