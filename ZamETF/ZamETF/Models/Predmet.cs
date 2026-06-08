using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class Predmet
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Naziv { get; set; }

        [Required]
        [StringLength(50)]
        public string SifraPredmeta { get; set; }

        public int Semestar { get; set; }

        public Profesor Profesor { get; set; }

        public string GetSifraPredmeta() => SifraPredmeta;

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
}