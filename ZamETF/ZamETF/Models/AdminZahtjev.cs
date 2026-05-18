using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class AdminZahtjev
    {
        public int Id { get; set; }
        [Required]
        public Administrator Administrator { get; set; }

        [Required]
        public ZahtjevZaDokument Zahtjev { get; set; }

        public TipAdminZahtjeva VrstaZahtjeva { get; set; }

        [StringLength(2000)]
        public string Komentar { get; set; }

        public bool Obradjen { get; set; } = false;

        public TipAdminZahtjeva GetVrstaZahtjeva() => VrstaZahtjeva;
        public bool GetObradjen() => Obradjen;
        public void SetObradjen(bool obradjen) => Obradjen = obradjen;
    }
}