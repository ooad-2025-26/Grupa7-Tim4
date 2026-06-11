using System.Collections.Generic;
using ZamETF.Models;

namespace ZamETF.ViewModels
{
    // --- Profesor: unos bodova ---
    public class StudentBodVM
    {
        public int StudentId { get; set; }
        public string ImePrezime { get; set; }
        public string Indeks { get; set; }
        public int? Bodovi { get; set; }
    }

    public class UnosOcjenaVM
    {
        public Predmet Predmet { get; set; }
        public List<StudentBodVM> Studenti { get; set; } = new List<StudentBodVM>();
    }

    // --- Student: pregled predmeta ---
    public class StudentPredmetVM
    {
        public Predmet Predmet { get; set; }
        public int? Bodovi { get; set; }            // finalno bodovanje predmeta (ili null)
        public List<Zadaca> Zadace { get; set; } = new List<Zadaca>();
        public int MojeId { get; set; }             // id prijavljenog studenta
    }
}
