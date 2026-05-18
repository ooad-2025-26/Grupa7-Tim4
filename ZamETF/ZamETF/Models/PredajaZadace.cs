using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class PredajaZadace
    {
        public int Id { get; set; }

        [Required]
        public Student Student { get; set; }

        [Required]
        public Zadaca Zadaca { get; set; }

        public DateTime DatumPredaje { get; set; } = DateTime.Now;

        [Required]
        [StringLength(260)]
        public string Fajl { get; set; }

        [StringLength(2000)]
        public string Komentar { get; set; }

        [Range(0, 100)]
        public int Bodovi { get; set; }

        public StatusZadace Status { get; set; } = StatusZadace.Predana;

        public string GetFajl() => Fajl;
        public void SetFajl(string fajl) => Fajl = fajl;
        public int GetBodovi() => Bodovi;
        public void SetBodovi(int bodovi) => Bodovi = bodovi;
        public StatusZadace GetStatus() => Status;
        public void SetStatus(StatusZadace status) => Status = status;
        public void DodajKomentar(string komentar) => Komentar = komentar;
    }
}