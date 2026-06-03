using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZamETF.Models
{
    public class Korisnik : IdentityUser<int>
    {
        [Required]
        [StringLength(20)]
        public string Ime { get; set; }

        [Required]
        [StringLength(30)]
        public string Prezime { get; set; }

        [Required]
        public Uloga Uloga { get; set; }

        [NotMapped]
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username
        {
            get => UserName;
            set => UserName = value;
        }

        [NotMapped]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Duzina lozinke mora biti najmanje 6 a najvise 50 karaktera!")]
        [RegularExpression("^(?=.*[A-Z])(?=.*\\d).+$", ErrorMessage = "Lozinka mora sadržavati barem jedno veliko slovo i jednu brojku.")]
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
        public bool ProvjeriLozinku(string lozinka, IPasswordHasher<Korisnik> hasher)
        {
            var result = hasher.VerifyHashedPassword(this, PasswordHash, lozinka);
            return result == PasswordVerificationResult.Success;
        }
    }
}