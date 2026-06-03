using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    public class StudentController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var student = await _context.Studenti
                .Include(s => s.PrijaveIspita)
                .Include(s => s.PredajeZadace)
                .FirstOrDefaultAsync(s => s.Id == korisnik.Id);

            // Predmeti studenta
            var predmeti = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .Where(u => u.StudentId == korisnik.Id)
                .Select(u => u.Predmet)
                .ToListAsync();

            // Obavijesti za studenta
            var obavijesti = await _context.Obavijesti
                .Where(o => o.PrimalacId == korisnik.Id)
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            // Aktuelno – ispiti dostupni za prijavu i zadace otvorene
            var aktuelnoIspiti = await _context.Ispiti
                .Include(i => i.Predmet)
                .Where(i => i.RokZaPrijavu >= DateTime.Now)
                .ToListAsync();

            var aktuelnoZadace = await _context.Zadace
                .Include(z => z.Predmet)
                .Where(z => z.Rok >= DateTime.Now)
                .ToListAsync();

            // Pretvori u listu obavijesti za prikaz
            var aktuelno = new List<Obavijest>();

            foreach (var ispit in aktuelnoIspiti)
            {
                aktuelno.Add(new Obavijest
                {
                    Naslov = $"Dostupne prijave za ispit – {ispit.Predmet?.Naziv}",
                    Poruka = $"Rok za prijavu: {ispit.RokZaPrijavu:dd.MM.yyyy}"
                });
            }

            foreach (var zadaca in aktuelnoZadace)
            {
                aktuelno.Add(new Obavijest
                {
                    Naslov = $"{zadaca.Predmet?.Naziv} – Otvoren rok za zadaću",
                    Poruka = $"Rok: {zadaca.Rok:dd.MM.yyyy}"
                });
            }

            ViewBag.Predmeti = predmeti;
            ViewBag.Obavijesti = obavijesti;
            ViewBag.Aktuelno = aktuelno;

            return View();
        }
        // GET: Student/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Indeks,GodinaStudija,Ime,Prezime,UserName,Email,Uloga")] Student student, string Lozinka)
        {
            if (ModelState.IsValid)
            {
                student.Uloga = Uloga.Student;
                var result = await _userManager.CreateAsync(student, Lozinka);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(student);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Studenti.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Indeks,GodinaStudija,Ime,Prezime,UserName,Email,Uloga")] Student student)
        {
            if (id != student.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var postojeci = await _context.Studenti.FindAsync(id);
                if (postojeci == null) return NotFound();
                postojeci.Ime = student.Ime;
                postojeci.Prezime = student.Prezime;
                postojeci.UserName = student.UserName;
                postojeci.Email = student.Email;
                postojeci.Indeks = student.Indeks;
                postojeci.GodinaStudija = student.GodinaStudija;
                await _userManager.UpdateAsync(postojeci);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
                await _userManager.DeleteAsync(student);
            return RedirectToAction(nameof(Index));
        }
        // GET: Student/ZahtjevZaDokument
        public async Task<IActionResult> ZahtjevZaDokument()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var zahtjevi = await _context.ZahtjeviDokumenata
                .Where(z => z.Student.Id == korisnik.Id)
                .OrderByDescending(z => z.Datum)
                .ToListAsync();
            ViewBag.Zahtjevi = zahtjevi;
            return View();
        }

        // POST: Student/PošaljiZahtjev
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PošaljiZahtjev(string tipDokumenta, string napomena)
        {
            // Pronađi trenutno ulogovanog studenta
            var korisnik = await _userManager.GetUserAsync(User);
            var student = await _context.Studenti.FindAsync(korisnik.Id);
            if (student == null) return NotFound();

            // Pronađi studentsku službu
            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska služba nije pronađena.";
                return RedirectToAction(nameof(ZahtjevZaDokument));
            }

            // Kreiraj zahtjev
            var zahtjev = new ZahtjevZaDokument
            {
                Student = student,
                TipDokumenta = tipDokumenta,
                Datum = DateTime.Now,
                Status = false
            };
            _context.ZahtjeviDokumenata.Add(zahtjev);
            await _context.SaveChangesAsync();

            // Pošalji obavijest studentskoj službi
            var obavijest = new Obavijest
            {
                Naslov = $"Zahtjev za dokument: {tipDokumenta}",
                Poruka = $"Student {student.Ime} {student.Prezime} je poslao zahtjev za {tipDokumenta}. {napomena}",
                PošiljalacId = student.Id,
                PrimalacId = studentskaSluzba.Id,
                ZahtjevId = zahtjev.Id,
                DatumSlanja = DateTime.Now
            };
            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev je uspješno poslan studentskoj službi!";
            return RedirectToAction(nameof(ZahtjevZaDokument));
        }
        // GET: Student/DetaljiObavijesti/5
        public async Task<IActionResult> DetaljiObavijesti(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                .FirstOrDefaultAsync(o => o.Id == id && o.PrimalacId == korisnik.Id);

            if (obavijest == null) return NotFound();

            // Oznaci kao procitanu
            if (!obavijest.Procitana)
            {
                obavijest.Procitana = true;
                await _context.SaveChangesAsync();
            }

            // Pronadi ime posiljаoca
            var posiljалac = await _context.Users.FindAsync(obavijest.PošiljalacId);
            string posiljалacIme = "Sistem";
            if (posiljалac != null)
            {
                var korisnikPosiljалac = posiljалac as Korisnik;
                if (korisnikPosiljалac != null)
                    posiljалacIme = korisnikPosiljалac.Ime + " " + korisnikPosiljалac.Prezime;
            }

            // Provjeri da li je masovno poslano – ako isti naslov ima vise primatelja
            var naslov = obavijest.Naslov ?? "";
            int brojSaIstimNaslovom = await _context.Obavijesti
                .CountAsync(o => o.Naslov == naslov && o.DatumSlanja == obavijest.DatumSlanja);
            bool masovnoPoslano = brojSaIstimNaslovom > 1;

            ViewBag.Obavijest = obavijest;
            ViewBag.Posiljалac = posiljалacIme;
            ViewBag.MasovnoPoslano = masovnoPoslano;

            return View();
        }
        // POST: Student/PosaljiZahtjev (bez sumera)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiZahtjev(string tipDokumenta, string jezik, string svrha, string napomena)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var student = await _context.Studenti.FindAsync(korisnik.Id);
            if (student == null) return NotFound();

            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska služba nije pronađena.";
                return RedirectToAction(nameof(ZahtjevZaDokument));
            }

            var tipPun = tipDokumenta;
            if (!string.IsNullOrEmpty(svrha))
                tipPun += " – " + svrha;

            var zahtjev = new ZahtjevZaDokument
            {
                Student = student,
                TipDokumenta = tipPun,
                Datum = DateTime.Now,
                Status = false
            };
          
            var obavijest = new Obavijest
            {
                Naslov = "Zahtjev za dokument: " + tipPun,
                Poruka = "Student " + student.Ime + " " + student.Prezime +
                         " je poslao zahtjev za " + tipPun +
                         " (Jezik: " + jezik + "). " + napomena,
                PošiljalacId = student.Id,
                PrimalacId = studentskaSluzba.Id,
                Zahtjev = zahtjev, // <-- assign the object reference, not the int id
                DatumSlanja = DateTime.Now
            };

            // Add both entities so EF can manage the FK relationship and generate the Id
            _context.ZahtjeviDokumenata.Add(zahtjev);
            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev je uspješno poslan studentskoj službi!";
            return RedirectToAction(nameof(ZahtjevZaDokument));
        }
    }
}