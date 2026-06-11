using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class Bodovanje
    {
        public int Id { get; set; }
        [Required]
        public Student Student { get; set; }
        public int StudentId { get; set; }

        [Required]
        public Predmet Predmet { get; set; }
        public int PredmetId { get; set; }

        [Range(0, 100)]
        public int Bodovi { get; set; }
    }
}