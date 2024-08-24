using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;

namespace Hospital.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ActiveIngredient> ActiveIngredient { get; set; }
        public DbSet<AdminLogin> AdminLogin { get; set; }
        public DbSet<Medication> Medication { get; set; }
        public DbSet<Nurse> Nurses { get; set; }

        public DbSet<Pharmacist> Pharmacists { get; set; }

        public DbSet<Surgeon> Surgeons { get; set; }
        public DbSet<DosageForm> DosageForm { get; set; }
        public DbSet<Patients> Patients { get; set; }
        public DbSet<DayHospital> DayHospital { get; set; }
        public DbSet<ChronicCondition> ChronicCondition { get; set; }

        public DbSet<OperatingTheatre> OperatingTheatre { get; set; }

        public DbSet<Province> Province { get; set; }

        public DbSet<Suburb> Suburb { get; set; }
        public DbSet<Town> Town { get; set; }
        public DbSet<Bed> Bed { get; set; }
        public DbSet<DischargedPatient> DischargedPatients { get; set; }
        public DbSet<PatientAllergy> PatientAllergies { get; set; }
        public DbSet<PatientCurrentMedication> PatientCurrentMedication { get; set; }
        public DbSet<PatientMedicalCondition> PatientMedicalCondition { get; set; }
        public DbSet<PatientsAdministration> PatientsAdministration { get; set; }
        public DbSet<PatientVital> PatientVital { get; set; }

        public DbSet<PatientWardAssignment> PatientWardAssignment { get; set; }

        public DbSet<TreatmentCode> TreatmentCode { get; set; }

        public DbSet<VitalRange> VitalRange { get; set; }
        public DbSet<Ward> Ward { get; set; }









    }
}
