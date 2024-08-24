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





    }
}
