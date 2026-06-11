using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class Ocjena
    {
        public int Id { get; set; }
        [Required]
        public Student Student { get; set; }

        [Required]
        public Predmet Predmet { get; set; }

        [Range(5, 10)]
        public int Vrijednost { get; set; }
    }
}