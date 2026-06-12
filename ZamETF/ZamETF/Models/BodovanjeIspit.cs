using System.ComponentModel.DataAnnotations;
namespace ZamETF.Models
{
    public enum TipIspita
    {
        Parcijalni1,
        Parcijalni2,
        Zavrsni,
        Integralni,
        Teorija
    }

    public class BodovanjeIspit
    {
        public int Id { get; set; }
        [Required]
        public Student Student { get; set; }
        public int StudentId { get; set; }
        [Required]
        public Predmet Predmet { get; set; }
        public int PredmetId { get; set; }
        public TipIspita Tip { get; set; }
        [Range(0, 100)]
        public int Bodovi { get; set; }
        public DateTime DatumUnosa { get; set; } = DateTime.Now;
    }
}