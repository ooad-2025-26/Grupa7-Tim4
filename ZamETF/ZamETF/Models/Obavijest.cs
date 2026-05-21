namespace ZamETF.Models
{
    public class Obavijest
    {
        public int Id { get; set; }
        public string Naslov { get; set; }
        public string Poruka { get; set; }
        public DateTime DatumSlanja { get; set; } = DateTime.Now;
        public bool Procitana { get; set; } = false;

        // Ko je poslao
        public int PošiljalacId { get; set; }
        public Korisnik Posiljалac { get; set; }

        // Ko prima
        public int PrimalacId { get; set; }
        public Korisnik Primalac { get; set; }

        // Opcionalno – vezan zahtjev
        public int? ZahtjevId { get; set; }
        public ZahtjevZaDokument Zahtjev { get; set; }
    }
}