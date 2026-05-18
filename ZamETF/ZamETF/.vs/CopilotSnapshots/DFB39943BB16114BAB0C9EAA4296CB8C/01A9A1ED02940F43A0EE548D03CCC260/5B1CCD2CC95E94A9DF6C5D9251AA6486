namespace ZamETF.Models
{
    public class Student : Korisnik
    {
        public Student() { }
        public Student(int indeks) { Indeks = indeks.ToString(); }

        public string Indeks { get; set; }
        public int GodinaStudija { get; set; }

        public string GetIndeks() => Indeks;
        public void SetIndeks(int indeks) => Indeks = indeks.ToString();

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
    }
}