namespace ZamETF.Models
{
    public class Zadaca
    {
        public int Id { get; set; }
        public string NazivID { get; set; }
        public Predmet Predmet { get; set; }
        public string Opis { get; set; }
        public DateTime Rok { get; set; }

        public string GetNazivID() => NazivID;
        public void SetNazivID(string naziv) => NazivID = naziv;
        public DateTime GetRok() => Rok;
        public void SetRok(DateTime rok) => Rok = rok;

        public ICollection<PredajaZadace> Predaje { get; set; } = new List<PredajaZadace>();

        public void DodajPredaju(PredajaZadace predaja) => Predaje.Add(predaja);
        public bool ProvjeriRok() => DateTime.Now <= Rok;
    }
}