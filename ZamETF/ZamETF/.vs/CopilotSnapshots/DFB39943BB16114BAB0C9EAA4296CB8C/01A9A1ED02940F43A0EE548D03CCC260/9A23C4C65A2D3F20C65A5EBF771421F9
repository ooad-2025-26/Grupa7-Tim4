namespace ZamETF.Models
{
    public class ZahtjevZaDokument
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public string TipDokumenta { get; set; }
        public DateTime Datum { get; set; } = DateTime.Now;
        public bool Status { get; set; } = false;

        public string GetTipDokumenta() => TipDokumenta;
        public bool GetStatus() => Status;
        public void SetStatus(bool status) => Status = status;
        public void ObradiZahtjev() => Status = true;
    }
}