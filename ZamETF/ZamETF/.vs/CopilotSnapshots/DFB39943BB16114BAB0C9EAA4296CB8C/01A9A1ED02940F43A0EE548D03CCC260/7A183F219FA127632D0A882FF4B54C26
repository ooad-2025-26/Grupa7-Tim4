using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZamETF.Models
{
    public class Korisnik : IdentityUser<int>
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Uloga Uloga { get; set; }

        [NotMapped]
        public string Username
        {
            get => UserName;
            set => UserName = value;
        }

        [NotMapped]
        public string Lozinka { get; set; }

        public string GetImeIPrezime() => $"{Ime} {Prezime}";
        public void SetImeIPrezime(string ime, string prezime)
        {
            Ime = ime;
            Prezime = prezime;
        }
        public string GetEmail() => Email;
        public void SetEmail(string email) => Email = email;
        public string GetUsername() => UserName;
        public void SetUsername(string username) => UserName = username;
        public Uloga GetUloga() => Uloga;
        public void SetUloga(Uloga uloga) => Uloga = uloga;
        public bool ProvjeriLozinku(string lozinka) => PasswordHash == lozinka;
    }
}