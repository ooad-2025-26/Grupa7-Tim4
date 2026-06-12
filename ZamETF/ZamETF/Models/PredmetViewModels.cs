using System.Collections.Generic;
using ZamETF.Models;
namespace ZamETF.ViewModels
{
    public class StudentBodVM
    {
        public int StudentId { get; set; }
        public string ImePrezime { get; set; }
        public string Indeks { get; set; }

        // Bodovi iz zadaća — suma predanih bodova
        public int BodoviZadace { get; set; }

        // Bodovi iz ispita
        public int? BodoviParcijalni { get; set; }
        public int? BodoviZavrsni { get; set; }
        public int? BodoviIntegralni { get; set; }
        public int? BodoviTeorija { get; set; }

        public int UkupnoBodovaIspit =>
            (BodoviParcijalni ?? 0) + (BodoviZavrsni ?? 0) +
            (BodoviIntegralni ?? 0) + (BodoviTeorija ?? 0);

        public int UkupnoBodova => Math.Min(BodoviZadace + UkupnoBodovaIspit, 100);

        public int? FinalnaOcjena { get; set; }
    }

    public class UnosOcjenaVM
    {
        public Predmet Predmet { get; set; }
        public List<StudentBodVM> Studenti { get; set; } = new List<StudentBodVM>();
    }

    public class StudentPredmetVM
    {
        public Predmet Predmet { get; set; }
        public int BodoviZadace { get; set; }
        public List<Zadaca> Zadace { get; set; } = new List<Zadaca>();
        public int MojeId { get; set; }

        public int? BodoviParcijalni { get; set; }
        public int? BodoviZavrsni { get; set; }
        public int? BodoviIntegralni { get; set; }
        public int? BodoviTeorija { get; set; }

        public int UkupnoBodovaIspit =>
            (BodoviParcijalni ?? 0) + (BodoviZavrsni ?? 0) +
            (BodoviIntegralni ?? 0) + (BodoviTeorija ?? 0);

        public int UkupnoBodova => Math.Min(BodoviZadace + UkupnoBodovaIspit, 100);

        public int? FinalnaOcjena { get; set; }
    }
}