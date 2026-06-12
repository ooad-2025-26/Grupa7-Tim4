using System.ComponentModel.DataAnnotations;
namespace ZamETF.Models
{
    public class Ocjena
    {
        public int Id { get; set; }
        [Required]
        public Student Student { get; set; }
        public int StudentId { get; set; }
        [Required]
        public Predmet Predmet { get; set; }
        public int PredmetId { get; set; }
        [Range(5, 10)]
        public int Vrijednost { get; set; }
        public bool JeFinalna { get; set; } = false;
        public DateTime DatumUnosa { get; set; } = DateTime.Now;
    }
}