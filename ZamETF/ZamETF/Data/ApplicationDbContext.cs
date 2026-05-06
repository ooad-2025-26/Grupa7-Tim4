using Microsoft.EntityFrameworkCore;
using ZamETF.Models;

namespace ZamETF.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=sql6033.site4now.net;Database=db_ac8f1e_zametfdb;User Id=db_ac8f1e_zametfdb_admin;Password=Munchmallow3!;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Korisnik>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Student>("Student")
                .HasValue<Profesor>("Profesor")
                .HasValue<Administrator>("Administrator")
                .HasValue<StudentskaSluzba>("StudentskaSluzba");

            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Profesor)
                .WithMany(pr => pr.Predmeti)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrijavaIspit>()
                .HasOne(pi => pi.Student)
                .WithMany(s => s.PrijaveIspita)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrijavaIspit>()
                .HasOne(pi => pi.Ispit)
                .WithMany(i => i.Prijave)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Zadaca>()
                .HasOne(z => z.Predmet)
                .WithMany(p => p.Zadace)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PredajaZadace>()
                .HasOne(pz => pz.Student)
                .WithMany(s => s.PredajeZadace)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PredajaZadace>()
                .HasOne(pz => pz.Zadaca)
                .WithMany(z => z.Predaje)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ZahtjevZaDokument>()
                .HasOne(z => z.Student)
                .WithMany(s => s.Zahtjevi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminZahtjev>()
                .HasOne(az => az.Administrator)
                .WithMany(a => a.Zahtjevi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminZahtjev>()
                .HasOne(az => az.Zahtjev)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Predmet)
                .WithMany(p => p.Ocjene)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bodovanje>()
                .HasOne(b => b.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bodovanje>()
                .HasOne(b => b.Predmet)
                .WithMany(p => p.Bodovanja)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prisustvo>()
                .HasOne(pr => pr.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prisustvo>()
                .HasOne(pr => pr.Predmet)
                .WithMany(p => p.Prisustva)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UpisNaPredmet>()
                .HasOne(u => u.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UpisNaPredmet>()
                .HasOne(u => u.Predmet)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Student> Studenti { get; set; }
        public DbSet<Profesor> Profesori { get; set; }
        public DbSet<Administrator> Administratori { get; set; }
        public DbSet<StudentskaSluzba> StudentskeSluzbe { get; set; }
        public DbSet<Predmet> Predmeti { get; set; }
        public DbSet<Ispit> Ispiti { get; set; }
        public DbSet<PrijavaIspit> PrijaveIspita { get; set; }
        public DbSet<Zadaca> Zadace { get; set; }
        public DbSet<PredajaZadace> PredajeZadace { get; set; }
        public DbSet<ZahtjevZaDokument> ZahtjeviDokumenata { get; set; }
        public DbSet<AdminZahtjev> AdminZahtjevi { get; set; }
        public DbSet<Ocjena> Ocjene { get; set; }
        public DbSet<Bodovanje> Bodovanja { get; set; }
        public DbSet<Prisustvo> Prisustva { get; set; }
        public DbSet<UpisNaPredmet> UpisaNaPredmet { get; set; }
    }
}