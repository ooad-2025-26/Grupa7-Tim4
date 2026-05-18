using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class ZahtjevZaDokument
    {
        public int Id { get; set; }
        [Required]
        public Student Student { get; set; }

        [Required]
        [StringLength(200)]
        public string TipDokumenta { get; set; }

        public DateTime Datum { get; set; } = DateTime.Now;

        public bool Status { get; set; } = false;

        public string GetTipDokumenta() => TipDokumenta;
        public bool GetStatus() => Status;
        public void SetStatus(bool status) => Status = status;
        public void ObradiZahtjev() => Status = true;
    }
}