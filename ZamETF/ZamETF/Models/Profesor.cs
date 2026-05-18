using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class Profesor : Korisnik
    {
        [StringLength(50)]
        public string Titula { get; set; }

        public ICollection<Predmet> Predmeti { get; set; } = new List<Predmet>();

        public void DodajPredmet(Predmet predmet) => Predmeti.Add(predmet);
        public List<Predmet> PregledajPredmete() => Predmeti.ToList();
        public List<Zadaca> PregledajZadace() => Predmeti.SelectMany(p => p.Zadace).ToList();
        public void DodajZadacu(Predmet predmet, Zadaca zadaca) => predmet.Zadace.Add(zadaca);

        public void UnesiBodovanje(PredajaZadace predaja, int bodovi)
        {
            predaja.Bodovi = bodovi;
            predaja.Status = StatusZadace.Pregledana;
            var predmet = predaja.Zadaca?.Predmet;
            if (predmet != null)
                predmet.Bodovanja.Add(new Bodovanje { Student = predaja.Student, Predmet = predmet, Bodovi = bodovi });
        }

        public void UnesiOcjenu(Student student, Ispit ispit, int ocjena)
        {
            var predmet = ispit.Predmet;
            predmet?.Ocjene.Add(new Ocjena { Student = student, Predmet = predmet, Vrijednost = ocjena });
        }

        public void UnesiPrisustvo(Student student, Predmet predmet, bool prisutan)
        {
            predmet.Prisustva.Add(new Prisustvo { Student = student, Predmet = predmet, Prisutan = prisutan });
        }
    }
}