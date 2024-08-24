using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class Nurse2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DischargedPatients",
                columns: table => new
                {
                    DischargedPatients = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: true),
                    DischargeTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DischargeDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DischargedPatients", x => x.DischargedPatients);
                    table.ForeignKey(
                        name: "FK_DischargedPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber");
                });

            migrationBuilder.CreateTable(
                name: "PatientAllergies",
                columns: table => new
                {
                    AllergyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: false),
                    Allergy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAllergies", x => x.AllergyId);
                    table.ForeignKey(
                        name: "FK_PatientAllergies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientCurrentMedication",
                columns: table => new
                {
                    MedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: false),
                    CurrentMedication = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientCurrentMedication", x => x.MedicationId);
                    table.ForeignKey(
                        name: "FK_PatientCurrentMedication_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedicalCondition",
                columns: table => new
                {
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: false),
                    MedicalCondition = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicalCondition", x => x.ConditionId);
                    table.ForeignKey(
                        name: "FK_PatientMedicalCondition_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientsAdministration",
                columns: table => new
                {
                    PatientsAdministration1 = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: false),
                    PatientWard = table.Column<int>(type: "int", nullable: false),
                    PatientBed = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientsAdministration", x => x.PatientsAdministration1);
                    table.ForeignKey(
                        name: "FK_PatientsAdministration_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientVital",
                columns: table => new
                {
                    PatientVitalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Tempreture = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BloodPressure = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Pulse = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Respiratory = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BloodOxygen = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BloodGlucoseLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VitalTime = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientVital", x => x.PatientVitalId);
                    table.ForeignKey(
                        name: "FK_PatientVital_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentCode",
                columns: table => new
                {
                    TreatmentCodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icd10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentCodeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentCode", x => x.TreatmentCodeId);
                });

            migrationBuilder.CreateTable(
                name: "VitalRange",
                columns: table => new
                {
                    VitalRangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VitalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumNormal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumNormal = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VitalRange", x => x.VitalRangeId);
                });

            migrationBuilder.CreateTable(
                name: "Ward",
                columns: table => new
                {
                    WardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WardName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfBeds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ward", x => x.WardId);
                });

            migrationBuilder.CreateTable(
                name: "Bed",
                columns: table => new
                {
                    BedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WardId = table.Column<int>(type: "int", nullable: false),
                    BedNumber = table.Column<int>(type: "int", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bed", x => x.BedId);
                    table.ForeignKey(
                        name: "FK_Bed_Ward_WardId",
                        column: x => x.WardId,
                        principalTable: "Ward",
                        principalColumn: "WardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientWardAssignment",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(13)", nullable: false),
                    BedId = table.Column<int>(type: "int", nullable: false),
                    DateAssigned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDischarged = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientWardAssignment", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_PatientWardAssignment_Bed_BedId",
                        column: x => x.BedId,
                        principalTable: "Bed",
                        principalColumn: "BedId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientWardAssignment_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientIDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bed_WardId",
                table: "Bed",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_DischargedPatients_PatientId",
                table: "DischargedPatients",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_PatientId",
                table: "PatientAllergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCurrentMedication_PatientId",
                table: "PatientCurrentMedication",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicalCondition_PatientId",
                table: "PatientMedicalCondition",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientsAdministration_PatientId",
                table: "PatientsAdministration",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVital_PatientId",
                table: "PatientVital",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientWardAssignment_BedId",
                table: "PatientWardAssignment",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientWardAssignment_PatientId",
                table: "PatientWardAssignment",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DischargedPatients");

            migrationBuilder.DropTable(
                name: "PatientAllergies");

            migrationBuilder.DropTable(
                name: "PatientCurrentMedication");

            migrationBuilder.DropTable(
                name: "PatientMedicalCondition");

            migrationBuilder.DropTable(
                name: "PatientsAdministration");

            migrationBuilder.DropTable(
                name: "PatientVital");

            migrationBuilder.DropTable(
                name: "PatientWardAssignment");

            migrationBuilder.DropTable(
                name: "TreatmentCode");

            migrationBuilder.DropTable(
                name: "VitalRange");

            migrationBuilder.DropTable(
                name: "Bed");

            migrationBuilder.DropTable(
                name: "Ward");
        }
    }
}
