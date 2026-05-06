namespace ZamETF.Models
{
    public class Korisnik
    {
        public int Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Lozinka { get; set; }
        public Uloga Uloga { get; set; }

        public string GetImeIPrezime() => $"{Ime} {Prezime}";
        public void SetImeIPrezime(string ime, string prezime) { Ime = ime; Prezime = prezime; }
        public string GetEmail() => Email;
        public void SetEmail(string email) => Email = email;
        public string GetUsername() => Username;
        public void SetUsername(string username) => Username = username;
        public Uloga GetUloga() => Uloga;
        public void SetUloga(Uloga uloga) => Uloga = uloga;
        public bool ProvjeriLozinku(string lozinka) => Lozinka == lozinka;
    }
}