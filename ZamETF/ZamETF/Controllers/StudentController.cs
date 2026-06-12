using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;
using Microsoft.AspNetCore.Authorization;


namespace ZamETF.Controllers
{
    [Authorize(Roles = "Student")]

    public class StudentController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _env;

        public StudentController(UserManager<Korisnik> userManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> SlanjeZadaca()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var predmeti = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .Where(u => u.StudentId == korisnik.Id)
                .Select(u => u.Predmet)
                .ToListAsync();

            var predmetIds = predmeti.Select(p => p.Id).ToList();

            var zadace = await _context.Zadace
                .Include(z => z.Predmet)
                .Where(z => predmetIds.Contains(z.PredmetID))
                .OrderBy(z => z.Rok)
                .ToListAsync();

            var mojePredaje = await _context.PredajeZadace
                .Where(p => p.StudentID == korisnik.Id)
                .ToListAsync();

            var model = new ZamETF.ViewModels.StudentZadaceVM
            {
                Stavke = zadace.Select(z => new ZamETF.ViewModels.StudentZadacaItemVM
                {
                    Zadaca = z,
                    MojaPredaja = mojePredaje.FirstOrDefault(p => p.ZadacaId == z.Id)
                }).ToList()
            };

            ViewBag.Predmeti = predmeti;   // za sidebar u _LayoutStudent
            return View(model);
        }

        // GET: Student/DetaljiZadace/5  — detalji jedne zadaće + forma za predaju
        public async Task<IActionResult> DetaljiZadace(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var zadaca = await _context.Zadace
                .Include(z => z.Predmet)
                .FirstOrDefaultAsync(z => z.Id == id);
            if (zadaca == null) return NotFound();

            var predaja = await _context.PredajeZadace
                .FirstOrDefaultAsync(p => p.ZadacaId == id && p.StudentID == korisnik.Id);

            var predmeti = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .Where(u => u.StudentId == korisnik.Id)
                .Select(u => u.Predmet)
                .ToListAsync();
            ViewBag.Predmeti = predmeti;

            return View(new ZamETF.ViewModels.DetaljiZadaceVM
            {
                Zadaca = zadaca,
                MojaPredaja = predaja
            });
        }

        // POST: Student/PredajZadacu  — upload PDF-a
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PredajZadacu(int zadacaId, IFormFile fajl, string komentar)
        {
            komentar = komentar ?? "";
            var korisnik = await _userManager.GetUserAsync(User);
            var student = await _context.Studenti.FindAsync(korisnik.Id);
            if (student == null) return NotFound();

            var zadaca = await _context.Zadace.FirstOrDefaultAsync(z => z.Id == zadacaId);
            if (zadaca == null) return NotFound();

            if (!zadaca.ProvjeriRok())
            {
                TempData["Greska"] = "Rok za predaju je istekao.";
                return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
            }

            // --- validacija fajla ---
            if (fajl == null || fajl.Length == 0)
            {
                TempData["Greska"] = "Niste odabrali fajl.";
                return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
            }

            var ext = Path.GetExtension(fajl.FileName).ToLowerInvariant();
            if (ext != ".pdf")
            {
                TempData["Greska"] = "Dozvoljeni su samo PDF fajlovi.";
                return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
            }

            if (fajl.Length > 10 * 1024 * 1024)   // 10 MB limit
            {
                TempData["Greska"] = "Fajl je prevelik (maksimalno 10 MB).";
                return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
            }

            // --- snimi fajl u wwwroot/uploads/zadace ---
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "zadace");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"z{zadacaId}_s{student.Id}_{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await fajl.CopyToAsync(stream);
            }
            var relPath = $"/uploads/zadace/{fileName}";

            // --- postojeća predaja? (ažuriraj umjesto duplikata) ---
            var predaja = await _context.PredajeZadace
                .FirstOrDefaultAsync(p => p.ZadacaId == zadacaId && p.StudentID == student.Id);

            if (predaja != null)
            {
                // obriši stari fajl
                if (!string.IsNullOrEmpty(predaja.Fajl))
                {
                    var stari = Path.Combine(_env.WebRootPath,
                        predaja.Fajl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(stari)) System.IO.File.Delete(stari);
                }

                predaja.Fajl = relPath;
                predaja.Komentar = komentar;
                predaja.DatumPredaje = DateTime.Now;
                predaja.Status = StatusZadace.Predana;
                predaja.Bodovi = null;   // vraća se na ocjenjivanje
            }
            else
            {
                _context.PredajeZadace.Add(new PredajaZadace
                {
                    ZadacaId = zadacaId,
                    StudentID = student.Id,
                    Fajl = relPath,
                    Komentar = komentar,
                    DatumPredaje = DateTime.Now,
                    Status = StatusZadace.Predana
                });
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Zadaća je uspješno predana.";
            return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
        }
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            // Predmeti studenta — samo upisani
            var predmeti = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .Where(u => u.StudentId == korisnik.Id)
                .Select(u => u.Predmet)
                .ToListAsync();

            var predmetIds = predmeti.Select(p => p.Id).ToList();

            // Obavijesti za studenta
            var obavijesti = await _context.Obavijesti
                .Where(o => o.PrimalacId == korisnik.Id)
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            // Aktuelno — samo za predmete na koje je student upisan
            var aktuelnoIspiti = await _context.Ispiti
                .Include(i => i.Predmet)
                .Where(i => i.RokZaPrijavu >= DateTime.Now && predmetIds.Contains(i.PredmetId))
                .ToListAsync();

            var aktuelnoZadace = await _context.Zadace
                .Include(z => z.Predmet)
                .Where(z => z.Rok >= DateTime.Now && predmetIds.Contains(z.PredmetID))
                .ToListAsync();

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

        // GET: Student/DetaljiPredmeta/5
        public async Task<IActionResult> DetaljiPredmeta(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            // student mora biti upisan na predmet
            var upis = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .FirstOrDefaultAsync(u => u.StudentId == korisnik.Id && u.Predmet.Id == id);
            if (upis == null) return NotFound();

            var predmet = upis.Predmet;

            var bodovanje = await _context.Bodovanja
                .FirstOrDefaultAsync(b => b.PredmetId == id && b.StudentId == korisnik.Id);

            var zadace = await _context.Zadace
                .Include(z => z.Predaje)
                .Where(z => z.PredmetID == id)
                .OrderBy(z => z.Rok)
                .ToListAsync();

            // za sidebar
            var predmeti = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .Where(u => u.StudentId == korisnik.Id)
                .Select(u => u.Predmet)
                .ToListAsync();
            ViewBag.Predmeti = predmeti;

            return View(new ZamETF.ViewModels.StudentPredmetVM
            {
                Predmet = predmet,
                Bodovi = bodovanje?.Bodovi,
                Zadace = zadace,
                MojeId = korisnik.Id
            });
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