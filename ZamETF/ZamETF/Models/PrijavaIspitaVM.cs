using System.Collections.Generic;

namespace ZamETF.Models
{
    public class PrijavaIspitaVM
    {
        public List<Ispit> Dostupni { get; set; } = new List<Ispit>();
        public List<PrijavaIspit> MojePrijave { get; set; } = new List<PrijavaIspit>();
    }
}
