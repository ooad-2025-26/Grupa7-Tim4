using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ZamETF.Models
{
    // ==================== ENUMI ====================

    public enum Uloga
    {
        Student,
        Profesor,
        Administrator,
        StudentskaSluzba
    }

    public enum TipDokumenta
    {
        PrepisOcjena,
        OcjenePoSemestrima,
        Potvrda
    }

    public enum StatusZadace
    {
        Predana,
        Prepisana,
        Pregledana
    }

    public enum StatusPrijaveIspit
    {
        PrijavljenIspit,
        PopunjenTermin,
        Odjavljen,
        IstekaoRok
    }

    public enum TipAdminZahtjeva
    {
        StatistikaProfesor,
        StatistikaStudent,
        StatistikaStudentskaSluzba,
        KreiranjeKorisnika,
        BrisanjeKorisnika,
        AzuriranjeKorisnika
    }


    // ==================== KORISNIK ====================

    public class Korisnik
    {
        public int Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Lozinka { get; set; }
        public Uloga Uloga { get; set; }

        // getImeIPrezime / setImeIPrezime
        public string GetImeIPrezime() => $"{Ime} {Prezime}";
        public void SetImeIPrezime(string ime, string prezime)
        {
            Ime = ime;
            Prezime = prezime;
        }

        public string GetEmail() => Email;
        public void SetEmail(string email) => Email = email;

        public string GetUsername() => Username;
        public void SetUsername(string username) => Username = username;

        public Uloga GetUloga() => Uloga;
        public void SetUloga(Uloga uloga) => Uloga = uloga;

        public bool ProvjeriLozinku(string lozinka) => Lozinka == lozinka;
    }


    // ==================== STUDENT ====================

    public class Student : Korisnik
    {
        public Student() { }

        public Student(int indeks)
        {
         
        }

        public string Indeks { get; set; }
        public int GodinaStudija { get; set; }

        public string GetIndeks() => Indeks;
        public void SetIndeks(int indeks) => Indeks = indeks.ToString();

        public ICollection<PrijavaIspit> PrijaveIspita { get; set; } = new List<PrijavaIspit>();
        public ICollection<PredajaZadace> PredajeZadace { get; set; } = new List<PredajaZadace>();
        public ICollection<ZahtjevZaDokument> Zahtjevi { get; set; } = new List<ZahtjevZaDokument>();

        // FZ 02 – prijava ispita
        public void DodajPrijavuIspita(PrijavaIspit prijava) => PrijaveIspita.Add(prijava);

        // FZ 02 – predaja zadace
        public void DodajPredajuZadace(PredajaZadace predaja) => PredajeZadace.Add(predaja);

        // Pregled prijava ispita
        public List<PrijavaIspit> PregledajPrijaveIspita() => PrijaveIspita.ToList();

        // Pregled predanih zadaca
        public List<PredajaZadace> PregledajPredaneZadace() => PredajeZadace.ToList();

        // FZ 07 – zahtjev prema studentskoj sluzbi
        public void PosaljiZahtjevZaDokument(string tipDokumenta)
        {
            Zahtjevi.Add(new ZahtjevZaDokument
            {
                Student = this,
                TipDokumenta = tipDokumenta,
                Status = false,
                Datum = DateTime.Now
            });
        }

        // FZ 04 – pregled ocjena sa ispita
        public List<int> PregledOcjene(Predmet predmet) =>
            predmet.Ocjene
                   .Where(o => o.Student.Id == Id)
                   .Select(o => o.Vrijednost)
                   .ToList();

        // FZ 04 – pregled bodova sa zadaca
        public List<int> PregledBodova(Predmet predmet) =>
            predmet.Bodovanja
                   .Where(b => b.Student.Id == Id)
                   .Select(b => b.Bodovi)
                   .ToList();

        // FZ 04 – pregled prisustva na predmetu
        public List<bool> PregledPrisustva(Predmet predmet) =>
            predmet.Prisustva
                   .Where(p => p.Student.Id == Id)
                   .Select(p => p.Prisutan)
                   .ToList();
    }


    // ==================== PROFESOR ====================

    public class Profesor : Korisnik
    {
        public string Titula { get; set; }
        public ICollection<Predmet> Predmeti { get; set; } = new List<Predmet>();

        // Dodavanje predmeta
        public void DodajPredmet(Predmet predmet) => Predmeti.Add(predmet);

        // Pregled predmeta
        public List<Predmet> PregledajPredmete() => Predmeti.ToList();

        // Pregled svih zadaca kroz sve predmete profesora
        public List<Zadaca> PregledajZadace() =>
            Predmeti.SelectMany(p => p.Zadace).ToList();

        // Dodavanje zadace na predmet
        public void DodajZadacu(Predmet predmet, Zadaca zadaca) => predmet.Zadace.Add(zadaca);

        // FZ 03 / FZ 04 – unos bodova za predaju zadace
        public void UnesiBodovanje(PredajaZadace predaja, int bodovi)
        {
            predaja.Bodovi = bodovi;
            predaja.Status = StatusZadace.Pregledana;

            // Spremi bodovanje i u kolekciju predmeta radi statistike
            var predmet = predaja.Zadaca?.Predmet;
            if (predmet != null)
            {
                predmet.Bodovanja.Add(new Bodovanje
                {
                    Student = predaja.Student,
                    Predmet = predmet,
                    Bodovi = bodovi
                });
            }
        }

        // FZ 04 – unos ocjene za ispit
        public void UnesiOcjenu(Student student, Ispit ispit, int ocjena)
        {
            var predmet = ispit.Predmet;
            predmet?.Ocjene.Add(new Ocjena
            {
                Student = student,
                Predmet = predmet,
                Vrijednost = ocjena
            });
        }

        // FZ 04 / FZ 08 – evidentiranje prisustva
        public void UnesiPrisustvo(Student student, Predmet predmet, bool prisutan)
        {
            predmet.Prisustva.Add(new Prisustvo
            {
                Student = student,
                Predmet = predmet,
                Prisutan = prisutan
            });
        }
    }


    // ==================== ADMINISTRATOR ====================

    public class Administrator : Korisnik
    {
        public ICollection<AdminZahtjev> Zahtjevi { get; set; } = new List<AdminZahtjev>();

        public void DodajZahtjev(AdminZahtjev zahtjev) => Zahtjevi.Add(zahtjev);

        public List<AdminZahtjev> PregledajZahtjeve() => Zahtjevi.ToList();

        public void ObradiZahtjev(int idZahtjeva)
        {
            var zahtjev = Zahtjevi.FirstOrDefault(z => z.Id == idZahtjeva);
            if (zahtjev != null)
                zahtjev.Obradjen = true;
        }
    }


    // ==================== STUDENTSKA SLUZBA ====================

    public class StudentskaSluzba : Korisnik
    {
        public ICollection<ZahtjevZaDokument> Zahtjevi { get; set; } = new List<ZahtjevZaDokument>();

        public void DodajZahtjevZaDokument(Student student, string dokument)
        {
            Zahtjevi.Add(new ZahtjevZaDokument
            {
                Student = student,
                TipDokumenta = dokument,
                Datum = DateTime.Now,
                Status = false
            });
        }

        public void DodajZahtjev(ZahtjevZaDokument zahtjev) => Zahtjevi.Add(zahtjev);

        public List<ZahtjevZaDokument> PregledajZahtjeve() => Zahtjevi.ToList();

        public void ObradiZahtjev(int idZahtjeva)
        {
            var zahtjev = Zahtjevi.FirstOrDefault(z => z.Id == idZahtjeva);
            if (zahtjev != null)
                zahtjev.ObradiZahtjev();
        }
    }


    // ==================== PREDMET ====================

    public class Predmet
    {
        public int Id { get; set; }
        public string Naziv { get; set; }
        public string SifraPredmeta { get; set; }

        public string GetSifraPredmeta() => SifraPredmeta;

        public Profesor Profesor { get; set; }

        public ICollection<Student> Studenti { get; set; } = new List<Student>();
        public ICollection<Zadaca> Zadace { get; set; } = new List<Zadaca>();
        public ICollection<Ocjena> Ocjene { get; set; } = new List<Ocjena>();
        public ICollection<Bodovanje> Bodovanja { get; set; } = new List<Bodovanje>();
        public ICollection<Prisustvo> Prisustva { get; set; } = new List<Prisustvo>();

        public void DodajStudenta(Student student) => Studenti.Add(student);
        public List<Student> PregledajStudente() => Studenti.ToList();

        public void DodajZadacu(Zadaca zadaca) => Zadace.Add(zadaca);
        public List<Zadaca> PregledajZadace() => Zadace.ToList();
    }


    // ==================== ISPIT ====================

    public class Ispit
    {
        public int Id { get; set; }
        public Predmet Predmet { get; set; }
        public DateTime Datum { get; set; }
        public DateTime RokZaPrijavu { get; set; }

        public DateTime GetDatum() => Datum;
        public void SetDatum(DateTime datum) => Datum = datum;

        public DateTime GetRokZaPrijavu() => RokZaPrijavu;
        public void SetRokZaPrijavu(DateTime rok) => RokZaPrijavu = rok;

        public ICollection<PrijavaIspit> Prijave { get; set; } = new List<PrijavaIspit>();

        public void DodajPrijavu(PrijavaIspit prijava) => Prijave.Add(prijava);

        public bool ProvjeriRokZaPrijavu() => DateTime.Now <= RokZaPrijavu;
    }


    // ==================== PRIJAVA ISPIT ====================

    public class PrijavaIspit
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Ispit Ispit { get; set; }
        public DateTime DatumPrijave { get; set; } = DateTime.Now;
        public StatusPrijaveIspit Status { get; set; } = StatusPrijaveIspit.PrijavljenIspit;

        public StatusPrijaveIspit GetStatus() => Status;
        public void SetStatus(StatusPrijaveIspit status) => Status = status;

        public void PotvrdiPrijavu() => Status = StatusPrijaveIspit.PrijavljenIspit;
        public void OtkaziPrijavu() => Status = StatusPrijaveIspit.Odjavljen;
    }


    // ==================== ZADACA ====================

    public class Zadaca
    {
        public int Id { get; set; }
        public string NazivID { get; set; }
        public Predmet Predmet { get; set; }
        public string Opis { get; set; }
        public DateTime Rok { get; set; }

        public string GetNazivID() => NazivID;
        public void SetNazivID(string naziv) => NazivID = naziv;

        public DateTime GetRok() => Rok;
        public void SetRok(DateTime rok) => Rok = rok;

        public ICollection<PredajaZadace> Predaje { get; set; } = new List<PredajaZadace>();

        public void DodajPredaju(PredajaZadace predaja) => Predaje.Add(predaja);

        public bool ProvjeriRok() => DateTime.Now <= Rok;
    }


    // ==================== PREDAJA ZADACE ====================

    public class PredajaZadace
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Zadaca Zadaca { get; set; }
        public DateTime DatumPredaje { get; set; } = DateTime.Now;
        public string Fajl { get; set; }
        public string Komentar { get; set; }
        public int Bodovi { get; set; }
        public StatusZadace Status { get; set; } = StatusZadace.Predana;

        public string GetFajl() => Fajl;
        public void SetFajl(string fajl) => Fajl = fajl;

        public int GetBodovi() => Bodovi;
        public void SetBodovi(int bodovi) => Bodovi = bodovi;

        public StatusZadace GetStatus() => Status;
        public void SetStatus(StatusZadace status) => Status = status;

        public void DodajKomentar(string komentar) => Komentar = komentar;
    }


    // ==================== ZAHTJEV ZA DOKUMENT ====================

    public class ZahtjevZaDokument
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public string TipDokumenta { get; set; }
        public DateTime Datum { get; set; } = DateTime.Now;
        public bool Status { get; set; } = false;

        public string GetTipDokumenta() => TipDokumenta;

        public bool GetStatus() => Status;
        public void SetStatus(bool status) => Status = status;

        public void ObradiZahtjev() => Status = true;
    }


    // ==================== ADMIN ZAHTJEV ====================

    public class AdminZahtjev
    {
        public int Id { get; set; }
        public Administrator Administrator { get; set; }
        public ZahtjevZaDokument Zahtjev { get; set; }
        public TipAdminZahtjeva VrstaZahtjeva { get; set; }
        public string Komentar { get; set; }
        public bool Obradjen { get; set; } = false;

        public TipAdminZahtjeva GetVrstaZahtjeva() => VrstaZahtjeva;

        public bool GetObradjen() => Obradjen;
        public void SetObradjen(bool obradjen) => Obradjen = obradjen;
    }


    // ==================== DODATNE TABELE ====================
    // ==================== UPIS NA PREDMET ====================

    public class UpisNaPredmet
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public int StudentId { get; set; }
        public Predmet Predmet { get; set; }
        public int PredmetId { get; set; }
        public DateTime DatumUpisa { get; set; } = DateTime.Now;
        public int GodinaStudija { get; set; }
    }

    public class Ocjena
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Predmet Predmet { get; set; }
        public int Vrijednost { get; set; }
    }

    public class Bodovanje
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Predmet Predmet { get; set; }
        public int Bodovi { get; set; }
    }

    public class Prisustvo
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Predmet Predmet { get; set; }
        public bool Prisutan { get; set; }
    }

    // Veza profesor-predmet (many-to-many pomocna tabela ako zatreba)
    public class ProfesorPredmet
    {
        public int Id { get; set; }
        public Profesor Profesor { get; set; }
        public Predmet Predmet { get; set; }
    }


    // ==================== DB CONTEXT ====================

    public class UniversityContext : DbContext
    {
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
        public DbSet<ProfesorPredmet> ProfesorPredmeti { get; set; }

        public UniversityContext(DbContextOptions<UniversityContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TPH nasljeđivanje – diskriminator po Uloga
            modelBuilder.Entity<Korisnik>()
                .HasDiscriminator<Uloga>("Uloga")
                .HasValue<Student>(Uloga.Student)
                .HasValue<Profesor>(Uloga.Profesor)
                .HasValue<Administrator>(Uloga.Administrator)
                .HasValue<StudentskaSluzba>(Uloga.StudentskaSluzba);

            // Predmet → Profesor (many-to-one)
            modelBuilder.Entity<Predmet>()
                .HasOne(p => p.Profesor)
                .WithMany(pr => pr.Predmeti)
                .OnDelete(DeleteBehavior.SetNull);

            // Predmet → Studenti (many-to-many kroz navigaciju)
            modelBuilder.Entity<Predmet>()
                .HasMany(p => p.Studenti)
                .WithMany();

            // Ispit → Predmet
            modelBuilder.Entity<Ispit>()
                .HasOne(i => i.Predmet)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // PrijavaIspit → Student / Ispit
            modelBuilder.Entity<PrijavaIspit>()
                .HasOne(pi => pi.Student)
                .WithMany(s => s.PrijaveIspita)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrijavaIspit>()
                .HasOne(pi => pi.Ispit)
                .WithMany(i => i.Prijave)
                .OnDelete(DeleteBehavior.Cascade);

            // Zadaca → Predmet
            modelBuilder.Entity<Zadaca>()
                .HasOne(z => z.Predmet)
                .WithMany(p => p.Zadace)
                .OnDelete(DeleteBehavior.Cascade);

            // PredajaZadace → Student / Zadaca
            modelBuilder.Entity<PredajaZadace>()
                .HasOne(pz => pz.Student)
                .WithMany(s => s.PredajeZadace)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PredajaZadace>()
                .HasOne(pz => pz.Zadaca)
                .WithMany(z => z.Predaje)
                .OnDelete(DeleteBehavior.Cascade);

            // ZahtjevZaDokument → Student
            modelBuilder.Entity<ZahtjevZaDokument>()
                .HasOne(z => z.Student)
                .WithMany(s => s.Zahtjevi)
                .OnDelete(DeleteBehavior.Cascade);

            // AdminZahtjev → Administrator / ZahtjevZaDokument
            modelBuilder.Entity<AdminZahtjev>()
                .HasOne(az => az.Administrator)
                .WithMany(a => a.Zahtjevi)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AdminZahtjev>()
                .HasOne(az => az.Zahtjev)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            // Ocjena → Student / Predmet
            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ocjena>()
                .HasOne(o => o.Predmet)
                .WithMany(p => p.Ocjene)
                .OnDelete(DeleteBehavior.Cascade);

            // Bodovanje → Student / Predmet
            modelBuilder.Entity<Bodovanje>()
                .HasOne(b => b.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bodovanje>()
                .HasOne(b => b.Predmet)
                .WithMany(p => p.Bodovanja)
                .OnDelete(DeleteBehavior.Cascade);

            // Prisustvo → Student / Predmet
            modelBuilder.Entity<Prisustvo>()
                .HasOne(pr => pr.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Prisustvo>()
                .HasOne(pr => pr.Predmet)
                .WithMany(p => p.Prisustva)
                .OnDelete(DeleteBehavior.Cascade);

            // ProfesorPredmet
            modelBuilder.Entity<ProfesorPredmet>()
                .HasOne(pp => pp.Profesor)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProfesorPredmet>()
                .HasOne(pp => pp.Predmet)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}