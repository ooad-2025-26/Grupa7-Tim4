namespace ZamETF.Models
{
    public class PredajaZadace
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Zadaca Zadaca { get; set; }
        public DateTime DatumPredaje { get; set; } = DateTime.Now;
        public string Fajl { get; set; }
        public string Komentar { get; set; }
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