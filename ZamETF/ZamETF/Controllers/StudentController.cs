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

            ViewBag.Predmeti = predmeti;
            return View(model);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PredajZadacu(int zadacaId, IFormFile fajl, string komentar)
        {
            try
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

                var predaja = await _context.PredajeZadace
                    .FirstOrDefaultAsync(p => p.ZadacaId == zadacaId && p.StudentID == student.Id);

                // Ako nema novog fajla
                if (fajl == null || fajl.Length == 0)
                {
                    if (predaja != null)
                    {
                        predaja.Komentar = komentar;
                        predaja.DatumPredaje = DateTime.Now;
                        await _context.SaveChangesAsync();
                        TempData["Uspjeh"] = "Komentar je ažuriran.";
                        return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
                    }
                    else
                    {
                        TempData["Greska"] = "Niste odabrali fajl.";
                        return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
                    }
                }

                // Validacija
                var ext = Path.GetExtension(fajl.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    TempData["Greska"] = "Dozvoljeni su samo PDF fajlovi.";
                    return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
                }

                if (fajl.Length > 10 * 1024 * 1024)
                {
                    TempData["Greska"] = "Fajl je prevelik (maksimalno 10 MB).";
                    return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
                }

                // Čitaj fajl u byte[]
                byte[] fajlBytes;
                using (var ms = new MemoryStream())
                {
                    await fajl.CopyToAsync(ms);
                    fajlBytes = ms.ToArray();
                }

                if (predaja != null)
                {
                    predaja.FajlBytes = fajlBytes;
                    predaja.FajlIme = fajl.FileName;
                    predaja.Komentar = komentar;
                    predaja.DatumPredaje = DateTime.Now;
                    predaja.Status = StatusZadace.Predana;
                    predaja.Bodovi = null;
                }
                else
                {
                    _context.PredajeZadace.Add(new PredajaZadace
                    {
                        ZadacaId = zadacaId,
                        StudentID = student.Id,
                        FajlBytes = fajlBytes,
                        FajlIme = fajl.FileName,
                        Fajl = "",
                        Komentar = komentar,
                        DatumPredaje = DateTime.Now,
                        Status = StatusZadace.Predana
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Uspjeh"] = "Zadaća je uspješno predana.";
                return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
            }
            catch (Exception ex)
            {
                TempData["Greska"] = "Greška pri predaji: " + ex.Message;
                return RedirectToAction(nameof(DetaljiZadace), new { id = zadacaId });
            }
        }
        public async Task<IActionResult> PreuzmiPdf(int id)
        {
            var predaja = await _context.PredajeZadace.FindAsync(id);
            if (predaja == null || (predaja.FajlBytes == null && string.IsNullOrEmpty(predaja.Fajl)))
                return NotFound();

            if (predaja.FajlBytes != null)
                return File(predaja.FajlBytes, "application/pdf", predaja.FajlIme ?? "zadaca.pdf");

            // Fallback za stare zapise sa putanjom
            return NotFound();
        }
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var predmeti = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .Where(u => u.StudentId == korisnik.Id)
                .Select(u => u.Predmet)
                .ToListAsync();

            var predmetIds = predmeti.Select(p => p.Id).ToList();

            var obavijesti = await _context.Obavijesti
                .Where(o => o.PrimalacId == korisnik.Id)
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        public IActionResult Create() => View();

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Studenti.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
                await _userManager.DeleteAsync(student);
            return RedirectToAction(nameof(Index));
        }

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

        public async Task<IActionResult> DetaljiPredmeta(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var upis = await _context.UpisaNaPredmet
                .Include(u => u.Predmet)
                .FirstOrDefaultAsync(u => u.StudentId == korisnik.Id && u.Predmet.Id == id);
            if (upis == null) return NotFound();

            var predmet = upis.Predmet;

            var bodovanje = await _context.Bodovanja
                .FirstOrDefaultAsync(b => b.PredmetId == id && b.StudentId == korisnik.Id);

            var bodovanjaIspit = await _context.BodovanjaIspit
                .Where(b => b.PredmetId == id && b.StudentId == korisnik.Id)
                .ToListAsync();

            var ocjena = await _context.Ocjene
                .FirstOrDefaultAsync(o => o.PredmetId == id && o.StudentId == korisnik.Id);

            var zadace = await _context.Zadace
                .Include(z => z.Predaje)
                .Where(z => z.PredmetID == id)
                .OrderBy(z => z.Rok)
                .ToListAsync();

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
                BodoviParcijalni1 = bodovanjaIspit.FirstOrDefault(b => b.Tip == TipIspita.Parcijalni1)?.Bodovi,
                BodoviParcijalni2 = bodovanjaIspit.FirstOrDefault(b => b.Tip == TipIspita.Parcijalni2)?.Bodovi,
                BodoviZavrsni = bodovanjaIspit.FirstOrDefault(b => b.Tip == TipIspita.Zavrsni)?.Bodovi,
                BodoviIntegralni = bodovanjaIspit.FirstOrDefault(b => b.Tip == TipIspita.Integralni)?.Bodovi,
                BodoviTeorija = bodovanjaIspit.FirstOrDefault(b => b.Tip == TipIspita.Teorija)?.Bodovi,
                FinalnaOcjena = ocjena?.Vrijednost,
                Zadace = zadace,
                MojeId = korisnik.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PošaljiZahtjev(string tipDokumenta, string napomena)
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

            var zahtjev = new ZahtjevZaDokument
            {
                Student = student,
                TipDokumenta = tipDokumenta,
                Datum = DateTime.Now,
                Status = false
            };
            _context.ZahtjeviDokumenata.Add(zahtjev);
            await _context.SaveChangesAsync();

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

        public async Task<IActionResult> DetaljiObavijesti(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null) return RedirectToAction("Login", "Account");

            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                .FirstOrDefaultAsync(o => o.Id == id && o.PrimalacId == korisnik.Id);

            if (obavijest == null) return NotFound();

            if (!obavijest.Procitana)
            {
                obavijest.Procitana = true;
                await _context.SaveChangesAsync();
            }

            var posiljалac = await _context.Users.FindAsync(obavijest.PošiljalacId);
            string posiljалacIme = "Sistem";
            if (posiljалac is Korisnik k)
                posiljалacIme = k.Ime + " " + k.Prezime;

            var naslov = obavijest.Naslov ?? "";
            int brojSaIstimNaslovom = await _context.Obavijesti
                .CountAsync(o => o.Naslov == naslov && o.DatumSlanja == obavijest.DatumSlanja);
            bool masovnoPoslano = brojSaIstimNaslovom > 1;

            ViewBag.Obavijest = obavijest;
            ViewBag.Posiljалac = posiljалacIme;
            ViewBag.MasovnoPoslano = masovnoPoslano;

            return View();
        }

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
                Zahtjev = zahtjev,
                DatumSlanja = DateTime.Now
            };

            _context.ZahtjeviDokumenata.Add(zahtjev);
            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev je uspješno poslan studentskoj službi!";
            return RedirectToAction(nameof(ZahtjevZaDokument));
        }
    }
}