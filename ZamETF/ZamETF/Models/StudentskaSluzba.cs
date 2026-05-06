namespace ZamETF.Models
{
    public class StudentskaSluzba : Korisnik
    {
        public ICollection<ZahtjevZaDokument> Zahtjevi { get; set; } = new List<ZahtjevZaDokument>();

        public void DodajZahtjev(ZahtjevZaDokument zahtjev) => Zahtjevi.Add(zahtjev);
        public List<ZahtjevZaDokument> PregledajZahtjeve() => Zahtjevi.ToList();

        public void ObradiZahtjev(int idZahtjeva)
        {
            var zahtjev = Zahtjevi.FirstOrDefault(z => z.Id == idZahtjeva);
            if (zahtjev != null) zahtjev.ObradiZahtjev();
        }
    }
}