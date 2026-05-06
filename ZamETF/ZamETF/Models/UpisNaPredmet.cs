namespace ZamETF.Models
{
    public class UpisNaPredmet
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public int StudentId { get; set; }
        public Predmet Predmet { get; set; }
        public int PredmetId { get; set; }
        public DateTime DatumUpisa { get; set; } = DateTime.Now;
        public int GodinaStudija { get; set; }
    }
}