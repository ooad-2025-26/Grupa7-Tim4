using System;
using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class Prisustvo
    {
        public int Id { get; set; }

        [Required]
        public Student Student { get; set; }

        [Required]
        public Predmet Predmet { get; set; }

        public bool Prisutan { get; set; }

        // Dodan Datum property
        public DateTime Datum { get; set; } = DateTime.Now;
    }
}
