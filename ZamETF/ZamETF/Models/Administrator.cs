namespace ZamETF.Models
{
    public class Administrator : Korisnik
    {
        public ICollection<AdminZahtjev> Zahtjevi { get; set; } = new List<AdminZahtjev>();

        public void DodajZahtjev(AdminZahtjev zahtjev) => Zahtjevi.Add(zahtjev);
        public List<AdminZahtjev> PregledajZahtjeve() => Zahtjevi.ToList();

        public void ObradiZahtjev(int idZahtjeva)
        {
            var zahtjev = Zahtjevi.FirstOrDefault(z => z.Id == idZahtjeva);
            if (zahtjev != null) zahtjev.Obradjen = true;
        }
    }
}