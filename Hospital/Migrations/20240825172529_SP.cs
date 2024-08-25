using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Town_Province_ProvinceId",
                table: "Town");

            migrationBuilder.AlterColumn<string>(
                name: "VitalName",
                table: "VitalRange",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumNormal",
                table: "VitalRange",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumNormal",
                table: "VitalRange",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "TreatmentCodeDescription",
                table: "TreatmentCode",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icd10Code",
                table: "TreatmentCode",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TownName",
                table: "Town",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProvinceId",
                table: "Town",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProvinceName",
                table: "Province",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icd10Code",
                table: "ChronicCondition",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentCodeId",
                table: "ChronicCondition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsOccupied",
                table: "Bed",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SurgeonPrescription",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientIdnumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientSurname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrescriptionDosageForm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurgeonId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Urgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dispense = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrescriptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispenseDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PharmacistName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PharmacistSurname = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurgeonPrescription", x => x.PrescriptionId);
                    table.ForeignKey(
                        name: "FK_SurgeonPrescription_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "MedicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurgeonPrescription_Surgeons_SurgeonId",
                        column: x => x.SurgeonId,
                        principalTable: "Surgeons",
                        principalColumn: "SurgeonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurgeonPrescription_MedicationId",
                table: "SurgeonPrescription",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_SurgeonPrescription_SurgeonId",
                table: "SurgeonPrescription",
                column: "SurgeonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Town_Province_ProvinceId",
                table: "Town",
                column: "ProvinceId",
                principalTable: "Province",
                principalColumn: "ProvinceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Town_Province_ProvinceId",
                table: "Town");

            migrationBuilder.DropTable(
                name: "SurgeonPrescription");

            migrationBuilder.DropColumn(
                name: "TreatmentCodeId",
                table: "ChronicCondition");

            migrationBuilder.AlterColumn<string>(
                name: "VitalName",
                table: "VitalRange",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumNormal",
                table: "VitalRange",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumNormal",
                table: "VitalRange",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TreatmentCodeDescription",
                table: "TreatmentCode",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Icd10Code",
                table: "TreatmentCode",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TownName",
                table: "Town",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ProvinceId",
                table: "Town",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ProvinceName",
                table: "Province",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Icd10Code",
                table: "ChronicCondition",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsOccupied",
                table: "Bed",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddForeignKey(
                name: "FK_Town_Province_ProvinceId",
                table: "Town",
                column: "ProvinceId",
                principalTable: "Province",
                principalColumn: "ProvinceId");
        }
    }
}
