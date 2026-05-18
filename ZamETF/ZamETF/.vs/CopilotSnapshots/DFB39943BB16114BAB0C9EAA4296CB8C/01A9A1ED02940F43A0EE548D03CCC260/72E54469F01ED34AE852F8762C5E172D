namespace ZamETF.Models
{
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
}