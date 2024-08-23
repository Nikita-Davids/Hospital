using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;

namespace Hospital.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ActiveIngredients> ActiveIngredients { get; set; }
        public DbSet<ActiveIngredients> AdminLogin { get; set; }
        public DbSet<Medications> Medications  { get; set; }
    }
}
