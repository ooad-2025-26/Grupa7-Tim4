namespace ZamETF.Models
{
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
}