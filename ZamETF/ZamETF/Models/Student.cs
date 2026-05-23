using System.ComponentModel.DataAnnotations;
namespace ZamETF.Models
{
    public class Student : Korisnik
    {
        public Student() { }
        public Student(int indeks) { Indeks = indeks.ToString(); }

        [StringLength(20)]
        public string Indeks { get; set; }

        [Range(1, 5)]
        public int GodinaStudija { get; set; }

        [StringLength(13)]
        public string JMBG { get; set; }

        public DateTime? DatumRodjenja { get; set; }

        [StringLength(100)]
        public string ImeOca { get; set; }

        [StringLength(100)]
        public string ImeMajke { get; set; }

        [StringLength(100)]
        public string MjestoPrebivalisca { get; set; }

        [StringLength(100)]
        public string Odsjek { get; set; }

        [StringLength(50)]
        public string Ciklus { get; set; }

        [StringLength(50)]
        public string TipStudija { get; set; }

        [StringLength(50)]
        public string StatusStudenta { get; set; }

        public int Semestar { get; set; }

        // Postojece kolekcije
        public ICollection<PrijavaIspit> PrijaveIspita { get; set; } = new List<PrijavaIspit>();
        public ICollection<PredajaZadace> PredajeZadace { get; set; } = new List<PredajaZadace>();
        public ICollection<ZahtjevZaDokument> Zahtjevi { get; set; } = new List<ZahtjevZaDokument>();

        public void DodajPrijavuIspita(PrijavaIspit prijava) => PrijaveIspita.Add(prijava);
        public void DodajPredajuZadace(PredajaZadace predaja) => PredajeZadace.Add(predaja);
        public List<PrijavaIspit> PregledajPrijaveIspita() => PrijaveIspita.ToList();
        public List<PredajaZadace> PregledajPredaneZadace() => PredajeZadace.ToList();

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

        public List<int> PregledOcjene(Predmet predmet) =>
            predmet.Ocjene.Where(o => o.Student.Id == Id).Select(o => o.Vrijednost).ToList();
        public List<int> PregledBodova(Predmet predmet) =>
            predmet.Bodovanja.Where(b => b.Student.Id == Id).Select(b => b.Bodovi).ToList();
        public List<bool> PregledPrisustva(Predmet predmet) =>
            predmet.Prisustva.Where(p => p.Student.Id == Id).Select(p => p.Prisutan).ToList();

        public string GetIndeks() => Indeks;
        public void SetIndeks(int indeks) => Indeks = indeks.ToString();
    }
}