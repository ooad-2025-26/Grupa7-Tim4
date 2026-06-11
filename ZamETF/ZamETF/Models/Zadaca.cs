using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class Zadaca
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NazivID { get; set; }

        public Predmet Predmet { get; set; }
        public int PredmetID { get; set; }

        [StringLength(2000)]
        public string Opis { get; set; }

        [Required]
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