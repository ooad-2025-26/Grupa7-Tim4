using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zamger.Models;

namespace Zamger.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Student { get; set; }
        public DbSet<Predmet> Predmet { get; set; }
        public DbSet<UpisNaPredmet> UpisNaPredmet { get; set; }
        public DbSet<Profesor> Profesor { get; set; }
        public DbSet<Administrator> Administrator { get; set; }
        public DbSet<Korisnik> Korisnik { get; set; }
        public DbSet<Ispit> Ispit { get; set; }
        public DbSet<PrijavaIspit> PrijavaIspit { get; set; }
        public DbSet<PredajaZadace> PredajaZadace { get; set; }
        public DbSet<ZahtjevZaDokument> ZahtjevZaDokument { get; set; }
        public DbSet<StudentService> StudentService { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<UpisNaPredmet>().ToTable("UpisNaPredmet");
            modelBuilder.Entity<Profesor>().ToTable("Profesor");
            modelBuilder.Entity<Administrator>().ToTable("Administrator");
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<Ispit>().ToTable("Ispit");
            modelBuilder.Entity<PrijavaIspit>().ToTable("PrijavaIspit");
            modelBuilder.Entity<PredajaZadace>().ToTable("PredajaZadace");
            modelBuilder.Entity<ZahtjevZaDokument>().ToTable("ZahtjevZaDokument");
            modelBuilder.Entity<StudentService>().ToTable("StudentService");

            base.OnModelCreating(modelBuilder);
        }
    }
}
