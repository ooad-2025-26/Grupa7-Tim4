using System.Collections.Generic;
using ZamETF.Models;
namespace ZamETF.ViewModels
{
    public class StudentBodVM
    {
        public int StudentId { get; set; }
        public string ImePrezime { get; set; }
        public string Indeks { get; set; }
        public int? Bodovi { get; set; } // bodovi iz zadaća (Bodovanje tabela)

        // Bodovi iz ispita
        public int? BodoviParcijalni1 { get; set; }
        public int? BodoviParcijalni2 { get; set; }
        public int? BodoviZavrsni { get; set; }
        public int? BodoviIntegralni { get; set; }
        public int? BodoviTeorija { get; set; }

        // Izračunato
        public int UkupnoBodovaIspit =>
            (BodoviParcijalni1 ?? 0) + (BodoviParcijalni2 ?? 0) +
            (BodoviZavrsni ?? 0) + (BodoviIntegralni ?? 0) +
            (BodoviTeorija ?? 0);

        public int UkupnoBodova => (Bodovi ?? 0) + UkupnoBodovaIspit;

        public int? FinalnaOcjena { get; set; } // iz Ocjena tabele
    }

    public class UnosOcjenaVM
    {
        public Predmet Predmet { get; set; }
        public List<StudentBodVM> Studenti { get; set; } = new List<StudentBodVM>();
    }

    public class StudentPredmetVM
    {
        public Predmet Predmet { get; set; }
        public int? Bodovi { get; set; }
        public List<Zadaca> Zadace { get; set; } = new List<Zadaca>();
        public int MojeId { get; set; }

        // Bodovi iz ispita
        public int? BodoviParcijalni1 { get; set; }
        public int? BodoviParcijalni2 { get; set; }
        public int? BodoviZavrsni { get; set; }
        public int? BodoviIntegralni { get; set; }
        public int? BodoviTeorija { get; set; }

        public int UkupnoBodovaIspit =>
            (BodoviParcijalni1 ?? 0) + (BodoviParcijalni2 ?? 0) +
            (BodoviZavrsni ?? 0) + (BodoviIntegralni ?? 0) +
            (BodoviTeorija ?? 0);

        public int UkupnoBodova => (Bodovi ?? 0) + UkupnoBodovaIspit;

        public int? FinalnaOcjena { get; set; }
    }
}