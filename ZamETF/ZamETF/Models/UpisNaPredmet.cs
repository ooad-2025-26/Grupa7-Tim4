using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class UpisNaPredmet
    {
        public int Id { get; set; }
        [Required]
        public Student Student { get; set; }

        public int StudentId { get; set; }

        [Required]
        public Predmet Predmet { get; set; }

        public int PredmetId { get; set; }

        public DateTime DatumUpisa { get; set; } = DateTime.Now;

        [Range(1, 5)]
        public int GodinaStudija { get; set; }
    }
}