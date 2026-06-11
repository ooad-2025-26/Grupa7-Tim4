using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZamETF.Models;

namespace ZamETF.Data
{
    public class ApplicationDbContext : IdentityDbContext<Korisnik, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=SQL6033.site4now.net;Initial Catalog=db_ac9277_zametf;User Id=db_ac9277_zametf_admin;Password=Munchmallow3!;Encrypt=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            modelBuilder.Entity<Obavijest>()
    .HasOne(o => o.Posiljалac)
    .WithMany()
    .HasForeignKey(o => o.PošiljalacId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Obavijest>()
                .HasOne(o => o.Primalac)
                .WithMany()
                .HasForeignKey(o => o.PrimalacId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Obavijest>()
                .HasOne(o => o.Zahtjev)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Predmet>()
    .HasOne(p => p.Profesor).WithMany(pr => pr.Predmeti)
    .HasForeignKey(p => p.ProfesorId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrijavaIspit>()
                .HasOne(pi => pi.Student).WithMany(s => s.PrijaveIspita)
                .HasForeignKey(pi => pi.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrijavaIspit>()
                .HasOne(pi => pi.Ispit).WithMany(i => i.Prijave)
                .HasForeignKey(pi => pi.IspitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.Predmet).WithMany()
                .HasForeignKey(i => i.PredmetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Zadaca>()
            .HasOne(z => z.Predmet).WithMany(p => p.Zadace)
            .HasForeignKey(z => z.PredmetID)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PredajaZadace>()
                .HasOne(pz => pz.Student).WithMany(s => s.PredajeZadace)
                .HasForeignKey(pz => pz.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PredajaZadace>()
                .HasOne(pz => pz.Zadaca).WithMany(z => z.Predaje)
                .HasForeignKey(pz => pz.ZadacaId)
                .OnDelete(DeleteBehavior.Restrict);
        }

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
        public DbSet<Obavijest> Obavijesti { get; set; }
    }
}