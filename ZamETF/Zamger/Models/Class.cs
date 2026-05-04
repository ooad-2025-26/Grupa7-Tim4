using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zamger.Models
{
    public enum VrstaStudenta
    {
        Redovni,
        RedovniSF,
        Vanredni
    }

    public class Student
    {
        [Key]
        public int BrojIndeksa { get; set; }
        public VrstaStudenta Vrsta { get; set; }
        public int GodinaStudija { get; set; }

        public ICollection<UpisNaPredmet> UpisaniPredmeti { get; set; }

        public Student() { }
    }

    public class Predmet
    {
        [Key]
        public int ID { get; set; }
        public string Naziv { get; set; }
        public double ECTS { get; set; }

        public ICollection<UpisNaPredmet> UpisaniStudenti { get; set; }

        public Predmet() { }

        public int GenerisiID()
        {
            int id = 0;
            Random generator = new Random();
            for (int i = 0; i < 10; i++)
                id += (int)Math.Pow(10, i) * generator.Next(0, 9);
            return id;
        }
    }

    public class UpisNaPredmet
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Predmet")]
        public int PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public UpisNaPredmet() { }
    }

    public class Profesor
    {
        [Key]
        public int ProfesorId { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }

        public ICollection<Predmet> Predmeti { get; set; }

        public Profesor() { }
    }

    public class Administrator
    {
        [Key]
        public int AdministratorId { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }

        public Administrator() { }
    }

    public class Korisnik
    {
        [Key]
        public int KorisnikId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Korisnik() { }
    }

    public class Ispit
    {
        [Key]
        public int IspitId { get; set; }
        public string Naziv { get; set; }
        public DateTime Datum { get; set; }

        public ICollection<PrijavaIspit> Prijave { get; set; }

        public Ispit() { }
    }

    public class PrijavaIspit
    {
        [Key]
        public int PrijavaId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Ispit")]
        public int IspitId { get; set; }
        public Ispit Ispit { get; set; }

        public PrijavaIspit() { }
    }

    public class PredajaZadace
    {
        [Key]
        public int ZadacaId { get; set; }
        public string Naziv { get; set; }
        public DateTime DatumPredaje { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public PredajaZadace() { }
    }

    public class ZahtjevZaDokument
    {
        [Key]
        public int ZahtjevId { get; set; }
        public string TipDokumenta { get; set; }
        public DateTime DatumZahtjeva { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public ZahtjevZaDokument() { }
    }

    public class StudentService
    {
        [Key]
        public int ServiceId { get; set; }
        public string Naziv { get; set; }

        public StudentService() { }
    }
}
