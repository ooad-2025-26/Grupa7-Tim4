using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using ZamETF.Services;

namespace ZamETF.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AdministratorController(UserManager<Korisnik> userManager,
      ApplicationDbContext context, EmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }

        // GET: Administrator
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);

            var obavijesti = await _context.Obavijesti
                .Include(o => o.Posiljалac)
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .Where(o => o.PrimalacId == korisnik.Id)
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            ViewBag.Obavijesti = obavijesti;
            return View();
        }

        // GET: Administrator/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var administrator = await _context.Administratori.FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null) return NotFound();
            return View(administrator);
        }

        // GET: Administrator/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,Username,Email,Uloga")] Administrator administrator, string Lozinka)
        {
            if (ModelState.IsValid)
            {
                administrator.Uloga = Uloga.Administrator;
                var result = await _userManager.CreateAsync(administrator, Lozinka);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(administrator);
        }

        // GET: Administrator/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var administrator = await _context.Administratori.FindAsync(id);
            if (administrator == null) return NotFound();
            return View(administrator);
        }

        // POST: Administrator/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Prezime,UserName,Email,Uloga")] Administrator administrator)
        {
            if (id != administrator.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var postojeci = await _context.Administratori.FindAsync(id);
                if (postojeci == null) return NotFound();
                postojeci.Ime = administrator.Ime;
                postojeci.Prezime = administrator.Prezime;
                postojeci.UserName = administrator.UserName;
                postojeci.Email = administrator.Email;
                await _userManager.UpdateAsync(postojeci);
                return RedirectToAction(nameof(Index));
            }
            return View(administrator);
        }

        // GET: Administrator/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var administrator = await _context.Administratori.FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null) return NotFound();
            return View(administrator);
        }

        // POST: Administrator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var administrator = await _context.Administratori.FindAsync(id);
            if (administrator != null)
                await _userManager.DeleteAsync(administrator);
            return RedirectToAction(nameof(Index));
        }

        // GET: Administrator/UnosIzmjena
        public async Task<IActionResult> UnosIzmjena()
        {
            var studenti = await _context.Studenti.ToListAsync();
            ViewBag.Studenti = studenti;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzmijeniKorisnika(
     int id,
     string ime, string prezime,
     string username, string email,
     string indeks, string odsjek,
     string jmbg, DateTime? datumRodjenja,
     string imeOca, string imeMajke,
     string mjesto, string ciklus,
     string tipStudija, string status,
     int godinaStudija, int semestar)
        {
            ime = ime?.Trim();
            prezime = prezime?.Trim();

            bool ValidnoIme(string vrijednost) =>
                !string.IsNullOrWhiteSpace(vrijednost) &&
                vrijednost.Length >= 2 &&
                vrijednost.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'');

            if (!ValidnoIme(ime))
            {
                TempData["Greska"] = "Ime mora imati najmanje 2 slova i ne smije sadržavati brojeve.";
                return RedirectToAction(nameof(UnosIzmjena));
            }

            if (!ValidnoIme(prezime))
            {
                TempData["Greska"] = "Prezime mora imati najmanje 2 slova i ne smije sadržavati brojeve.";
                return RedirectToAction(nameof(UnosIzmjena));
            }

            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                TempData["Greska"] = "Student nije pronađen.";
                return RedirectToAction(nameof(UnosIzmjena));
            }

            var stariSemestar = student.Semestar;

            student.Ime = ime;
            student.Prezime = prezime;

            if (!string.IsNullOrWhiteSpace(username) && username != student.UserName)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(student, username);
                if (!setUsernameResult.Succeeded)
                {
                    TempData["Greska"] = "Greška pri promjeni korisničkog imena: " +
                        string.Join(", ", setUsernameResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(UnosIzmjena));
                }
            }

            if (!string.IsNullOrWhiteSpace(email) && email != student.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(student, email);
                if (!setEmailResult.Succeeded)
                {
                    TempData["Greska"] = "Greška pri promjeni emaila: " +
                        string.Join(", ", setEmailResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(UnosIzmjena));
                }
                student.EmailConfirmed = true;
            }

            student.Indeks = indeks;
            student.Odsjek = odsjek;
            student.JMBG = jmbg;
            student.ImeOca = imeOca;
            student.ImeMajke = imeMajke;
            student.MjestoPrebivalisca = mjesto;
            student.Ciklus = ciklus;
            student.TipStudija = tipStudija;
            student.StatusStudenta = status;
            student.GodinaStudija = godinaStudija;
            student.Semestar = semestar;

            if (datumRodjenja.HasValue)
                student.DatumRodjenja = datumRodjenja.Value;

            // Ažuriraj upise ako se semestar promijenio
            if (stariSemestar != semestar)
            {
                // Ukloni stare upise za predmete starog semestra
                var stariUpisi = await _context.UpisaNaPredmet
                    .Include(u => u.Predmet)
                    .Where(u => u.StudentId == id && u.Predmet.Semestar == stariSemestar)
                    .ToListAsync();
                _context.UpisaNaPredmet.RemoveRange(stariUpisi);

                // Dodaj upise za predmete novog semestra
                var noviPredmeti = await _context.Predmeti
                    .Where(p => p.Semestar == semestar)
                    .ToListAsync();

                var vecUpisani = await _context.UpisaNaPredmet
                    .Where(u => u.StudentId == id)
                    .Select(u => u.PredmetId)
                    .ToListAsync();

                foreach (var predmet in noviPredmeti)
                {
                    if (!vecUpisani.Contains(predmet.Id))
                    {
                        _context.UpisaNaPredmet.Add(new UpisNaPredmet
                        {
                            StudentId = id,
                            Student = student,
                            PredmetId = predmet.Id,
                            Predmet = predmet,
                            GodinaStudija = godinaStudija,
                            DatumUpisa = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            var result = await _userManager.UpdateAsync(student);

            if (result.Succeeded)
                TempData["Uspjeh"] = $"Podaci za {student.Ime} {student.Prezime} su uspješno izmijenjeni!" +
                    (stariSemestar != semestar ? $" Upisi ažurirani sa semestra {stariSemestar} na {semestar}." : "");
            else
                TempData["Greska"] = "Greška pri snimanju: " +
                    string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(UnosIzmjena));
        }

        // POST: Administrator/ObrisiKorisnika
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiKorisnika(int id)
        {
            var korisnik = await _userManager.FindByIdAsync(id.ToString());
            if (korisnik == null)
            {
                TempData["Greska"] = "Korisnik nije pronađen.";
                return RedirectToAction(nameof(UnosIzmjena));
            }

            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                var upisi = await _context.UpisaNaPredmet.Where(u => u.StudentId == id).ToListAsync();
                _context.UpisaNaPredmet.RemoveRange(upisi);

                var ocjene = await _context.Ocjene.Where(o => o.Student.Id == id).ToListAsync();
                _context.Ocjene.RemoveRange(ocjene);

                var prisustva = await _context.Prisustva.Where(p => p.Student.Id == id).ToListAsync();
                _context.Prisustva.RemoveRange(prisustva);

                var zahtjevi = await _context.ZahtjeviDokumenata.Where(z => z.Student.Id == id).ToListAsync();
                _context.ZahtjeviDokumenata.RemoveRange(zahtjevi);

                await _context.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(korisnik);
            if (result.Succeeded)
                TempData["Uspjeh"] = "Korisnik je uspješno obrisan.";
            else
                TempData["Greska"] = "Greška pri brisanju: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(UnosIzmjena));
        }

        // POST: Administrator/KreirajKorisnika
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreirajKorisnika(string ime, string prezime,
            string email, string indeks, int godinaStudija, string privilegije, string lozinka)
        {
            // --- Validacija imena i prezimena ---
            ime = ime?.Trim();
            prezime = prezime?.Trim();

            bool ValidnoIme(string vrijednost) =>
                !string.IsNullOrWhiteSpace(vrijednost) &&
                vrijednost.Length >= 2 &&
                vrijednost.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'');

            if (!ValidnoIme(ime))
            {
                TempData["Greska"] = "Ime mora imati najmanje 2 slova i ne smije sadržavati brojeve.";
                return RedirectToAction(nameof(UnosIzmjena));
            }

            if (!ValidnoIme(prezime))
            {
                TempData["Greska"] = "Prezime mora imati najmanje 2 slova i ne smije sadržavati brojeve.";
                return RedirectToAction(nameof(UnosIzmjena));
            }
            // Ako admin nije unio email, automatski ga generiši
            var finalEmail = string.IsNullOrWhiteSpace(email)
                ? await GenerirajEmail(ime, prezime)
                : email.Trim();

            // Provjeri duplikat samo kad je email ručno unesen
            if (!string.IsNullOrWhiteSpace(email))
            {
                var postojeciEmail = await _userManager.FindByEmailAsync(finalEmail);
                if (postojeciEmail != null)
                {
                    TempData["Greska"] = $"Korisnik s emailom '{finalEmail}' već postoji.";
                    return RedirectToAction(nameof(UnosIzmjena));
                }
            }

            var username = await GenerirajUsername(ime, prezime);
            IdentityResult result;

            if (privilegije == "Student")
            {
                var student = new Student
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = finalEmail,
                    Indeks = indeks,
                    GodinaStudija = godinaStudija,
                    Uloga = Uloga.Student,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(student, lozinka);
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(student, "Student");
            }
            else if (privilegije == "Profesor")
            {
                var profesor = new Profesor
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = finalEmail,
                    Uloga = Uloga.Profesor,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(profesor, lozinka);
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(profesor, "Profesor");
            }
            else if (privilegije == "Administrator")
            {
                var admin = new Administrator
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = finalEmail,
                    Uloga = Uloga.Administrator,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(admin, lozinka);
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(admin, "Administrator");
            }
            else
            {
                var sluzba = new StudentskaSluzba
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = finalEmail,
                    Uloga = Uloga.StudentskaSluzba,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(sluzba, lozinka);
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(sluzba, "StudentskaSluzba");
            }

            if (result.Succeeded)
            {
                var emailNapomena = string.IsNullOrWhiteSpace(email)
                    ? $"{finalEmail} (automatski generisan)"
                    : finalEmail;
                TempData["Uspjeh"] = $"Korisnik kreiran! Email: {emailNapomena} | Username: {username} | Lozinka: {lozinka}";
            }
            else
            {
                TempData["Greska"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(UnosIzmjena));
        }

        // Helper – generiše email automatski
        private async Task<string> GenerirajEmail(string ime, string prezime)
        {
            var imeClean = ime.ToLower()
                .Replace("č", "c").Replace("ć", "c")
                .Replace("š", "s").Replace("ž", "z")
                .Replace("đ", "d").Replace(" ", "");

            var prezimeClean = prezime.ToLower()
                .Replace("č", "c").Replace("ć", "c")
                .Replace("š", "s").Replace("ž", "z")
                .Replace("đ", "d").Replace(" ", "");

            var baseEmail = $"{imeClean}.{prezimeClean}@etf.unsa.ba";

            var postojeci = await _userManager.FindByEmailAsync(baseEmail);
            if (postojeci == null) return baseEmail;

            int broj = 1;
            while (true)
            {
                var emailSaBrojem = $"{imeClean}.{prezimeClean}{broj}@etf.unsa.ba";
                var provjera = await _userManager.FindByEmailAsync(emailSaBrojem);
                if (provjera == null) return emailSaBrojem;
                broj++;
            }
        }

        // Helper – generiše username automatski
        private async Task<string> GenerirajUsername(string ime, string prezime)
        {
            var imeClean = ime.ToLower()
                .Replace("č", "c").Replace("ć", "c")
                .Replace("š", "s").Replace("ž", "z")
                .Replace("đ", "d").Replace(" ", "");

            var prezimeClean = prezime.ToLower()
                .Replace("č", "c").Replace("ć", "c")
                .Replace("š", "s").Replace("ž", "z")
                .Replace("đ", "d").Replace(" ", "");

            var baseUsername = $"{imeClean}.{prezimeClean}";

            var postojeci = await _userManager.FindByNameAsync(baseUsername);
            if (postojeci == null) return baseUsername;

            int broj = 1;
            while (true)
            {
                var usernamesBrojem = $"{imeClean}.{prezimeClean}{broj}";
                var provjera = await _userManager.FindByNameAsync(usernamesBrojem);
                if (provjera == null) return usernamesBrojem;
                broj++;
            }
        }

        public IActionResult Pocetna()
        {
            return RedirectToAction("Index");
        }

        // GET: Administrator/Statistika
        public async Task<IActionResult> Statistika()
        {
            var korisnik = await _userManager.GetUserAsync(User);

            var zahtjevi = await _context.Obavijesti
                .Include(o => o.Posiljалac)
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .Where(o => o.PrimalacId == korisnik.Id && !o.Procitana)
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            var studenti = await _context.Studenti.ToListAsync();
            var profesori = await _context.Profesori.ToListAsync();
            var predmeti = await _context.Predmeti.ToListAsync();

            ViewBag.Zahtjevi = zahtjevi;
            ViewBag.Studenti = studenti;
            ViewBag.Profesori = profesori;
            ViewBag.Predmeti = predmeti;

            return View();
        }

        // POST: Administrator/GenerirajIzZahtjeva
        [HttpPost]
        public async Task<IActionResult> GenerirajIzZahtjeva(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            var student = obavijest.Zahtjev?.Student;
            if (student == null) return NotFound();

            var ocjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Where(o => o.Student.Id == student.Id)
                .ToListAsync();

            var prisustva = await _context.Prisustva
                .Include(p => p.Predmet)
                .Where(p => p.Student.Id == student.Id)
                .ToListAsync();

            var pdf = GenerirајPdfStudenta(student, ocjene, prisustva);
            var fileName = $"{student.Ime}{student.Prezime}Statistika.pdf";

            obavijest.Procitana = true;
            await _context.SaveChangesAsync();

            return File(pdf, "application/pdf", fileName);
        }

        // POST: Administrator/PošaljiPdfStudentskaSluzba
        [HttpPost]
        public async Task<IActionResult> PošaljiPdfStudentskaSluzba(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .Include(o => o.Posiljалac)
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            var korisnik = await _userManager.GetUserAsync(User);
            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();

            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska služba nije pronađena.";
                return RedirectToAction(nameof(Statistika));
            }

            var novaObavijest = new Obavijest
            {
                Naslov = $"Statistika generisana: {obavijest.Zahtjev?.TipDokumenta}",
                Poruka = $"Administrator je generisao statistiku za zahtjev: {obavijest.Naslov}. Dokument je spreman.",
                PošiljalacId = korisnik.Id,
                PrimalacId = studentskaSluzba.Id,
                ZahtjevId = obavijest.ZahtjevId,
                DatumSlanja = DateTime.Now
            };

            _context.Obavijesti.Add(novaObavijest);

            obavijest.Procitana = true;
            if (obavijest.Zahtjev != null)
                obavijest.Zahtjev.Status = true;

            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "PDF je poslan studentskoj službi!";
            return RedirectToAction(nameof(Statistika));
        }

        // POST: Administrator/GenerirajStatistikuStudenta
        [HttpPost]
        public async Task<IActionResult> GenerirajStatistikuStudenta(int studentId)
        {
            var student = await _context.Studenti.FindAsync(studentId);
            if (student == null) return NotFound();

            var ocjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Where(o => o.Student.Id == studentId)
                .ToListAsync();

            var prisustva = await _context.Prisustva
                .Include(p => p.Predmet)
                .Where(p => p.Student.Id == studentId)
                .ToListAsync();

            var pdf = GenerirајPdfStudenta(student, ocjene, prisustva);
            var fileName = $"{student.Ime}{student.Prezime}Statistika.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        // POST: Administrator/GenerirajStatistikuProfesora
        [HttpPost]
        public async Task<IActionResult> GenerirajStatistikuProfesora(int profesorId)
        {
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == profesorId);
            if (profesor == null) return NotFound();

            var zadace = await _context.Zadace
                .Include(z => z.Predmet)
                .Where(z => z.Predmet.Profesor.Id == profesorId)
                .ToListAsync();

            var pdf = GenerirajPdfProfesora(profesor, zadace);
            var fileName = $"{profesor.Ime}{profesor.Prezime}Statistika.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        // POST: Administrator/GenerirajStatistikuStudentskeSluzbe
        [HttpPost]
        public async Task<IActionResult> GenerirajStatistikuStudentskeSluzbe()
        {
            var zahtjevi = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .ToListAsync();

            var pdf = GenerirajPdfStudentskeSluzbe(zahtjevi);
            return File(pdf, "application/pdf", "StatistikaStudentskaSluzba.pdf");
        }

        // POST: Administrator/GenerirajStatistikuPredmeta
        [HttpPost]
        public async Task<IActionResult> GenerirajStatistikuPredmeta(int predmetId)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Profesor)
                .Include(p => p.Studenti)
                .Include(p => p.Ocjene)
                .Include(p => p.Prisustva)
                .FirstOrDefaultAsync(p => p.Id == predmetId);
            if (predmet == null) return NotFound();

            var pdf = GenerirajPdfPredmeta(predmet);
            var fileName = $"{predmet.Naziv}Statistika.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        // ==================== PDF GENERATORI ====================

        private byte[] GenerirајPdfStudenta(Student student, List<Ocjena> ocjene, List<Prisustvo> prisustva)
        {
            using var ms = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph($"Statistika za studenta: {student.Ime} {student.Prezime}")
                .SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Indeks: {student.Indeks}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Godina studija: {student.GodinaStudija}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Datum generisanja: {DateTime.Now:dd.MM.yyyy HH:mm}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // OCJENE PO SEMESTRIMA
            doc.Add(new iText.Layout.Element.Paragraph("OCJENE PO SEMESTRIMA").SetFontSize(14).SetBold());

            if (ocjene.Any())
            {
                var ocjenePoPredmetu = ocjene
                    .Where(o => o.Predmet != null)
                    .GroupBy(o => o.Predmet.Semestar)
                    .OrderBy(g => g.Key);

                foreach (var semestarGrupa in ocjenePoPredmetu)
                {
                    doc.Add(new iText.Layout.Element.Paragraph($"{semestarGrupa.Key}. semestar")
                        .SetFontSize(12).SetBold());

                    var tabela = new iText.Layout.Element.Table(2).UseAllAvailableWidth();
                    tabela.AddHeaderCell("Predmet");
                    tabela.AddHeaderCell("Ocjena");

                    foreach (var o in semestarGrupa)
                    {
                        tabela.AddCell(o.Predmet?.Naziv ?? "N/A");
                        tabela.AddCell(o.Vrijednost.ToString());
                    }

                    doc.Add(tabela);

                    var prosjekSemestar = semestarGrupa.Average(o => o.Vrijednost);
                    doc.Add(new iText.Layout.Element.Paragraph($"Prosjek za {semestarGrupa.Key}. semestar: {prosjekSemestar:F2}")
                        .SetFontSize(11).SetItalic());
                    doc.Add(new iText.Layout.Element.Paragraph(" "));
                }

                var ukupniProsjek = ocjene.Average(o => o.Vrijednost);
                doc.Add(new iText.Layout.Element.Paragraph($"UKUPNI PROSJEK OCJENA: {ukupniProsjek:F2}")
                    .SetFontSize(13).SetBold());
            }
            else
            {
                doc.Add(new iText.Layout.Element.Paragraph("Nema unesenih ocjena.").SetFontSize(12));
            }

            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // PRISUSTVO
            doc.Add(new iText.Layout.Element.Paragraph("PRISUSTVO NA NASTAVI").SetFontSize(14).SetBold());

            if (prisustva.Any())
            {
                var tabela2 = new iText.Layout.Element.Table(3).UseAllAvailableWidth();
                tabela2.AddHeaderCell("Predmet");
                tabela2.AddHeaderCell("Semestar");
                tabela2.AddHeaderCell("Prisustvo");

                var grupaPoPredmetu = prisustva
                    .GroupBy(p => p.Predmet?.Naziv ?? "N/A")
                    .OrderBy(g => prisustva.FirstOrDefault(p => p.Predmet?.Naziv == g.Key)?.Predmet?.Semestar ?? 0);

                double ukupnoPosto = 0;
                int brojPredmeta = 0;

                foreach (var grupa in grupaPoPredmetu)
                {
                    var ukupnoGrupa = grupa.Count();
                    var prisutnoGrupa = grupa.Count(p => p.Prisutan);
                    var postoGrupa = ukupnoGrupa > 0 ? (double)prisutnoGrupa / ukupnoGrupa * 100 : 0;
                    var semestarPredmeta = prisustva.FirstOrDefault(p => p.Predmet?.Naziv == grupa.Key)?.Predmet?.Semestar;

                    tabela2.AddCell(grupa.Key);
                    tabela2.AddCell(semestarPredmeta.HasValue ? $"{semestarPredmeta}. sem." : "-");
                    tabela2.AddCell($"{prisutnoGrupa}/{ukupnoGrupa} ({postoGrupa:F0}%)");

                    ukupnoPosto += postoGrupa;
                    brojPredmeta++;
                }

                doc.Add(tabela2);

                var ukupno = prisustva.Count;
                var prisutno = prisustva.Count(p => p.Prisutan);
                var ukupniPostotak = ukupno > 0 ? (double)prisutno / ukupno * 100 : 0;
                var prosjekPoPoredmetima = brojPredmeta > 0 ? ukupnoPosto / brojPredmeta : 0;

                doc.Add(new iText.Layout.Element.Paragraph(" "));
                doc.Add(new iText.Layout.Element.Paragraph($"Ukupno prisustvo: {prisutno}/{ukupno} ({ukupniPostotak:F0}%)")
                    .SetFontSize(12).SetBold());
                doc.Add(new iText.Layout.Element.Paragraph($"Prosječno prisustvo po predmetima: {prosjekPoPoredmetima:F1}%")
                    .SetFontSize(12).SetBold());
            }
            else
            {
                doc.Add(new iText.Layout.Element.Paragraph("Nema evidencije prisustva.").SetFontSize(12));
            }

            doc.Close();
            return ms.ToArray();
        }

        private byte[] GenerirajPdfProfesora(Profesor profesor, List<Zadaca> zadace)
        {
            using var ms = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph($"Statistika za profesora: {profesor.Ime} {profesor.Prezime}").SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Titula: {profesor.Titula}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            doc.Add(new iText.Layout.Element.Paragraph("PREDMETI").SetFontSize(14).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Ukupan broj predmeta: {profesor.Predmeti?.Count ?? 0}").SetFontSize(12));

            if (profesor.Predmeti != null && profesor.Predmeti.Any())
            {
                var tabela = new iText.Layout.Element.Table(2).UseAllAvailableWidth();
                tabela.AddHeaderCell("Predmet");
                tabela.AddHeaderCell("Šifra");
                foreach (var p in profesor.Predmeti)
                {
                    tabela.AddCell(p.Naziv);
                    tabela.AddCell(p.SifraPredmeta);
                }
                doc.Add(tabela);
            }

            doc.Add(new iText.Layout.Element.Paragraph(" "));
            doc.Add(new iText.Layout.Element.Paragraph("ZADAĆE").SetFontSize(14).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Ukupan broj kreiranih zadaća: {zadace.Count}").SetFontSize(12));

            var ocijenjene = zadace.SelectMany(z => z.Predaje ?? new List<PredajaZadace>())
                .Count(p => p.Status == StatusZadace.Pregledana);
            doc.Add(new iText.Layout.Element.Paragraph($"Ukupno ocijenjenih predaja: {ocijenjene}").SetFontSize(12));

            doc.Close();
            return ms.ToArray();
        }

        private byte[] GenerirajPdfStudentskeSluzbe(List<ZahtjevZaDokument> zahtjevi)
        {
            using var ms = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph("Statistika studentske službe").SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Datum generisanja: {DateTime.Now:dd.MM.yyyy}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            doc.Add(new iText.Layout.Element.Paragraph($"Ukupan broj zahtjeva: {zahtjevi.Count}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Obrađenih zahtjeva: {zahtjevi.Count(z => z.Status)}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Neobrađenih zahtjeva: {zahtjevi.Count(z => !z.Status)}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            doc.Add(new iText.Layout.Element.Paragraph("TIPOVI ZAHTJEVA").SetFontSize(14).SetBold());

            var tipoviGrupa = zahtjevi.GroupBy(z => z.TipDokumenta).OrderByDescending(g => g.Count());

            var tabela = new iText.Layout.Element.Table(2).UseAllAvailableWidth();
            tabela.AddHeaderCell("Tip dokumenta");
            tabela.AddHeaderCell("Broj zahtjeva");
            foreach (var tip in tipoviGrupa)
            {
                tabela.AddCell(tip.Key ?? "N/A");
                tabela.AddCell(tip.Count().ToString());
            }
            doc.Add(tabela);

            doc.Close();
            return ms.ToArray();
        }

        private byte[] GenerirajPdfPredmeta(Predmet predmet)
        {
            using var ms = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph($"Statistika za predmet: {predmet.Naziv}").SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Šifra predmeta: {predmet.SifraPredmeta}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Profesor: {predmet.Profesor?.Ime} {predmet.Profesor?.Prezime}").SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            doc.Add(new iText.Layout.Element.Paragraph($"Broj upisanih studenata: {predmet.Studenti?.Count ?? 0}").SetFontSize(12).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            doc.Add(new iText.Layout.Element.Paragraph("STATISTIKA OCJENA").SetFontSize(14).SetBold());

            if (predmet.Ocjene != null && predmet.Ocjene.Any())
            {
                var prosjek = predmet.Ocjene.Average(o => o.Vrijednost);
                var prolaznost = predmet.Ocjene.Count(o => o.Vrijednost >= 6) * 100.0 / predmet.Ocjene.Count;

                doc.Add(new iText.Layout.Element.Paragraph($"Prosječna ocjena: {prosjek:F2}").SetFontSize(12));
                doc.Add(new iText.Layout.Element.Paragraph($"Prolaznost: {prolaznost:F0}%").SetFontSize(12));

                var tabela = new iText.Layout.Element.Table(2).UseAllAvailableWidth();
                tabela.AddHeaderCell("Ocjena");
                tabela.AddHeaderCell("Broj studenata");

                var ocjeneGrupa = predmet.Ocjene.GroupBy(o => o.Vrijednost).OrderBy(g => g.Key);
                foreach (var grupa in ocjeneGrupa)
                {
                    tabela.AddCell(grupa.Key.ToString());
                    tabela.AddCell(grupa.Count().ToString());
                }
                doc.Add(tabela);
            }
            else
            {
                doc.Add(new iText.Layout.Element.Paragraph("Nema unesenih ocjena.").SetFontSize(12));
            }

            doc.Close();
            return ms.ToArray();
        }

        // ==================== NOVE AKCIJE ====================

        // GET: Administrator/SviZahtjevi
        public async Task<IActionResult> SviZahtjevi()
        {
            var korisnik = await _userManager.GetUserAsync(User);

            var zahtjeviPodaci = await _context.Obavijesti
                .Include(o => o.Posiljалac)
                .Where(o => o.PrimalacId == korisnik.Id &&
                       (o.Naslov.Contains("Zahtjev za Upis") ||
                        o.Naslov.Contains("Zahtjev za Izmjena") ||
                        o.Naslov.Contains("podataka")))
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            var zahtjeviStatistika = await _context.Obavijesti
                .Include(o => o.Posiljалac)
                .Where(o => o.PrimalacId == korisnik.Id &&
                       (o.Naslov.Contains("statistiku") ||
                        o.Naslov.Contains("Statistika") ||
                        o.Naslov.Contains("Zahtjev profesora")))
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            ViewBag.ZahtjeviPodaci = zahtjeviPodaci;
            ViewBag.ZahtjeviStatistika = zahtjeviStatistika;
            return View();
        }

        // POST: Administrator/OznaciObradenim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OznaciObradenim(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            obavijest.Procitana = true;
            if (obavijest.Zahtjev != null)
                obavijest.Zahtjev.Status = true;

            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev oznacen kao obradjeno!";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);
            return RedirectToAction(nameof(Index));
        }

        // POST: Administrator/IzmijeniIzZahtjeva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzmijeniIzZahtjeva(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .Include(o => o.Posiljалac)
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            var poruka = obavijest.Poruka ?? "";
            var linije = poruka.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var indeks = linije.FirstOrDefault(l => l.StartsWith("Indeks:"))?.Replace("Indeks:", "").Trim();

            if (!string.IsNullOrEmpty(indeks))
            {
                var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Indeks == indeks);
                if (student != null)
                {
                    foreach (var linija in linije)
                    {
                        if (linija.StartsWith("Ime:")) student.Ime = linija.Replace("Ime:", "").Trim();
                        else if (linija.StartsWith("Prezime:")) student.Prezime = linija.Replace("Prezime:", "").Trim();
                        else if (linija.StartsWith("Email:")) student.Email = linija.Replace("Email:", "").Trim();
                        else if (linija.StartsWith("Godina studija:") && int.TryParse(linija.Replace("Godina studija:", "").Trim(), out int god))
                            student.GodinaStudija = god;
                        else if (linija.StartsWith("Odsjek:")) student.Odsjek = linija.Replace("Odsjek:", "").Trim();
                        else if (linija.StartsWith("Smjer:")) student.Odsjek = linija.Replace("Smjer:", "").Trim();
                    }
                    await _userManager.UpdateAsync(student);
                }
            }

            obavijest.Procitana = true;
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Podaci studenta su azurirani!";
            return RedirectToAction(nameof(SviZahtjevi));
        }

        // POST: Administrator/PosaljiPdfStudentskaSluzba
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiPdfStudentskaSluzba(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .Include(o => o.Posiljалac)
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            var korisnik = await _userManager.GetUserAsync(User);
            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();

            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska sluzba nije pronadjena.";
                return RedirectToAction(nameof(Statistika));
            }

            var student = obavijest.Zahtjev?.Student;

            if (student != null)
            {
                var ocjene = await _context.Ocjene
                    .Include(o => o.Predmet)
                    .Where(o => o.Student.Id == student.Id)
                    .ToListAsync();
                var prisustva = await _context.Prisustva
                    .Include(p => p.Predmet)
                    .Where(p => p.Student.Id == student.Id)
                    .ToListAsync();

                GenerirајPdfStudenta(student, ocjene, prisustva);
            }

            var novaObavijest = new Obavijest
            {
                Naslov = "Statistika generisana: " + (obavijest.Zahtjev?.TipDokumenta ?? obavijest.Naslov),
                Poruka = "Administrator je generisao statistiku za zahtjev: " + obavijest.Naslov + ". Dokument je spreman.",
                PošiljalacId = korisnik.Id,
                PrimalacId = studentskaSluzba.Id,
                ZahtjevId = obavijest.ZahtjevId,
                DatumSlanja = DateTime.Now
            };

            _context.Obavijesti.Add(novaObavijest);
            obavijest.Procitana = true;
            if (obavijest.Zahtjev != null)
                obavijest.Zahtjev.Status = true;

            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Obavijest o statistici je poslana studentskoj sluzbi!";
            return RedirectToAction(nameof(Statistika));
        }

        // POST: Administrator/PosaljiStatistikuStudentskaSluzba
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiStatistikuStudentskaSluzba(int studentId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var student = await _context.Studenti.FindAsync(studentId);
            if (student == null) return NotFound();

            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska sluzba nije pronadjena.";
                return RedirectToAction(nameof(Statistika));
            }

            _context.Obavijesti.Add(new Obavijest
            {
                Naslov = "Statistika studenta: " + student.Ime + " " + student.Prezime,
                Poruka = "Administrator je generisao statistiku za studenta " + student.Ime + " " + student.Prezime + " (indeks: " + student.Indeks + "). Dokument je spreman.",
                PošiljalacId = korisnik.Id,
                PrimalacId = studentskaSluzba.Id,
                DatumSlanja = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Statistika poslana studentskoj sluzbi!";
            return RedirectToAction(nameof(Statistika));
        }

        // POST: Administrator/PosaljiStatistikuPredmetaStudentskaSluzba
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiStatistikuPredmetaStudentskaSluzba(int predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska sluzba nije pronadjena.";
                return RedirectToAction(nameof(Statistika));
            }

            _context.Obavijesti.Add(new Obavijest
            {
                Naslov = "Statistika predmeta: " + predmet.Naziv,
                Poruka = "Administrator je generisao statistiku za predmet " + predmet.Naziv + ". Dokument je spreman.",
                PošiljalacId = korisnik.Id,
                PrimalacId = studentskaSluzba.Id,
                DatumSlanja = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Statistika predmeta poslana studentskoj sluzbi!";
            return RedirectToAction(nameof(Statistika));
        }

        public async Task<IActionResult> KreiranjePredmeta()
        {
            var profesori = await _context.Profesori.ToListAsync();
            var predmeti = await _context.Predmeti
                .Include(p => p.Profesor)
                .OrderBy(p => p.Semestar)
                .ToListAsync();

            // Ažuriraj broj studenata kroz UpisaNaPredmet
            foreach (var predmet in predmeti)
            {
                predmet.Studenti = await _context.UpisaNaPredmet
                    .Include(u => u.Student)
                    .Where(u => u.PredmetId == predmet.Id)
                    .Select(u => u.Student)
                    .ToListAsync();
            }

            ViewBag.Profesori = profesori;
            ViewBag.Predmeti = predmeti;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SinhronizujUpise()
        {
            var sviPredmeti = await _context.Predmeti.ToListAsync();
            var sviStudenti = await _context.Studenti.ToListAsync();
            var sviUpisi = await _context.UpisaNaPredmet.ToListAsync();

            int dodano = 0;

            foreach (var student in sviStudenti)
            {
                var predmetiZaSemestar = sviPredmeti
                    .Where(p => p.Semestar == student.Semestar)
                    .ToList();

                foreach (var predmet in predmetiZaSemestar)
                {
                    var vecPostoji = sviUpisi.Any(u => u.StudentId == student.Id && u.PredmetId == predmet.Id);
                    if (!vecPostoji)
                    {
                        _context.UpisaNaPredmet.Add(new UpisNaPredmet
                        {
                            StudentId = student.Id,
                            Student = student,
                            PredmetId = predmet.Id,
                            Predmet = predmet,
                            GodinaStudija = student.GodinaStudija,
                            DatumUpisa = DateTime.Now
                        });
                        dodano++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = $"Sinhronizacija završena! Dodano {dodano} novih upisa.";
            return RedirectToAction(nameof(KreiranjePredmeta));
        }

        // POST: Administrator/KreirajPredmet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreirajPredmet(string naziv, string sifraPredmeta, int semestar, int odabraniProfesorId)
        {
            if (string.IsNullOrEmpty(naziv) || string.IsNullOrEmpty(sifraPredmeta))
            {
                TempData["Greska"] = "Naziv i sifra predmeta su obavezni.";
                return RedirectToAction(nameof(KreiranjePredmeta));
            }

            var profesor = await _context.Profesori.FindAsync(odabraniProfesorId);
            if (profesor == null)
            {
                TempData["Greska"] = "Odabrani profesor nije pronadjen.";
                return RedirectToAction(nameof(KreiranjePredmeta));
            }

            var studenti = await _context.Studenti
           .Where(s => s.Semestar == semestar)
           .ToListAsync();

            var predmet = new Predmet
            {
                Naziv = naziv,
                SifraPredmeta = sifraPredmeta,
                Semestar = semestar,
                Profesor = profesor
            };

            foreach (var student in studenti)
            {
                predmet.Studenti.Add(student);
                _context.UpisaNaPredmet.Add(new UpisNaPredmet
                {
                    Student = student,
                    Predmet = predmet,
                    StudentId = student.Id
                });
            }

            _context.Predmeti.Add(predmet);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Predmet '" + naziv + "' je uspjesno kreiran i dodijeljen " + studenti.Count + " studenata!";
            return RedirectToAction(nameof(KreiranjePredmeta));
        }

        // POST: Administrator/ObrisiPredmet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiPredmet(int predmetId)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            // Nulliraj PredmetId direktno u bazi raw SQL-om
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE AspNetUsers SET PredmetId = NULL WHERE PredmetId = {0}", predmetId);

            // Ukloni upise
            var upisi = await _context.UpisaNaPredmet.Where(u => u.PredmetId == predmetId).ToListAsync();
            _context.UpisaNaPredmet.RemoveRange(upisi);

            // Ukloni ocjene
            var ocjene = await _context.Ocjene.Where(o => o.Predmet.Id == predmetId).ToListAsync();
            _context.Ocjene.RemoveRange(ocjene);

            // Ukloni prisustva
            var prisustva = await _context.Prisustva.Where(p => p.Predmet.Id == predmetId).ToListAsync();
            _context.Prisustva.RemoveRange(prisustva);

            await _context.SaveChangesAsync();

            _context.Predmeti.Remove(predmet);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Predmet je obrisan.";
            return RedirectToAction(nameof(KreiranjePredmeta));
        }

        // GET: Administrator/Notifikacije
        public async Task<IActionResult> Notifikacije()
        {
            var studenti = await _context.Studenti.OrderBy(s => s.Prezime).ToListAsync();
            var profesori = await _context.Profesori.OrderBy(p => p.Prezime).ToListAsync();
            ViewBag.Studenti = studenti;
            ViewBag.Profesori = profesori;
            return View();
        }

        // POST: Administrator/PosaljiNotifikaciju
        // POST: Administrator/PosaljiNotifikaciju
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiNotifikaciju(
            string naslov, string poruka, string tipNotifikacije,
            string primateljTip, string odabraniIds, string odabraneGodine)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            int brojPoslanih = 0;
            var naslovSaTipom = "[" + tipNotifikacije + "] " + naslov;

            var primatelji = new List<Korisnik>();

            if (primateljTip == "student")
            {
                var ids = (odabraniIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in ids)
                    if (int.TryParse(idStr, out int sid))
                    {
                        var s = await _context.Studenti.FindAsync(sid);
                        if (s != null) primatelji.Add(s);
                    }
            }
            else if (primateljTip == "profesor")
            {
                var ids = (odabraniIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in ids)
                    if (int.TryParse(idStr, out int pid))
                    {
                        var p = await _context.Profesori.FindAsync(pid);
                        if (p != null) primatelji.Add(p);
                    }
            }
            else if (primateljTip == "svi_studenti")
                primatelji.AddRange(await _context.Studenti.ToListAsync());
            else if (primateljTip == "svi_profesori")
                primatelji.AddRange(await _context.Profesori.ToListAsync());
            else if (primateljTip == "godina")
            {
                var godine = (odabraneGodine ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => int.Parse(g)).ToList();
                primatelji.AddRange(await _context.Studenti
                    .Where(s => godine.Contains(s.GodinaStudija)).ToListAsync());
            }
            else if (primateljTip == "sluzba")
            {
                var sluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
                if (sluzba != null) primatelji.Add(sluzba);
            }

            // In-app obavijesti za sve
            foreach (var p in primatelji)
            {
                _context.Obavijesti.Add(new Obavijest
                {
                    Naslov = naslovSaTipom,
                    Poruka = poruka,
                    PošiljalacId = korisnik.Id,
                    PrimalacId = p.Id,
                    DatumSlanja = DateTime.Now
                });
                brojPoslanih++;
            }
            await _context.SaveChangesAsync();

            // Email — jedna konekcija za sve primatelje (bulk)
            try
            {
                var emailPrimatelji = primatelji
                    .Where(p => !string.IsNullOrWhiteSpace(p.Email))
                    .Select(p => (p.Email!, p.GetImeIPrezime()));

                await _emailService.PošaljiBulkEmail(emailPrimatelji, naslovSaTipom, poruka);
            }
            catch (Exception ex)
            {
                TempData["Greska"] = $"Notifikacija poslana, ali email nije uspio: {ex.Message}";
                return RedirectToAction(nameof(Notifikacije));
            }

            TempData["Uspjeh"] = $"Notifikacija poslana! Primatelja: {brojPoslanih}";
            return RedirectToAction(nameof(Notifikacije));
        }
        public async Task<IActionResult> UnosIzmjenaIzZahtjeva(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            var studenti = await _context.Studenti.ToListAsync();
            ViewBag.Studenti = studenti;

            // Parsiraj poruku i proslijedi podatke u ViewBag
            var poruka = obavijest.Poruka ?? "";
            var linije = poruka.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            ViewBag.PrefilledIme = linije.FirstOrDefault(l => l.StartsWith("Ime:"))?.Replace("Ime:", "").Trim();
            ViewBag.PrefilledPrezime = linije.FirstOrDefault(l => l.StartsWith("Prezime:"))?.Replace("Prezime:", "").Trim();
            ViewBag.PrefilledIndeks = linije.FirstOrDefault(l => l.StartsWith("Indeks:"))?.Replace("Indeks:", "").Trim();
            ViewBag.PrefilledEmail = linije.FirstOrDefault(l => l.StartsWith("Email:"))?.Replace("Email:", "").Trim();
            ViewBag.PrefilledGodina = linije.FirstOrDefault(l => l.StartsWith("Godina studija:"))?.Replace("Godina studija:", "").Trim();
            ViewBag.PrefilledSemestar = linije.FirstOrDefault(l => l.StartsWith("Semestar:"))?.Replace("Semestar:", "").Trim();
            ViewBag.PrefilledJmbg = linije.FirstOrDefault(l => l.StartsWith("JMBG:"))?.Replace("JMBG:", "").Trim();
            ViewBag.PrefilledDatum = linije.FirstOrDefault(l => l.StartsWith("Datum rodjenja:"))?.Replace("Datum rodjenja:", "").Trim();
            ViewBag.PrefilledImeOca = linije.FirstOrDefault(l => l.StartsWith("Ime oca:"))?.Replace("Ime oca:", "").Trim();
            ViewBag.PrefilledImeMajke = linije.FirstOrDefault(l => l.StartsWith("Ime majke:"))?.Replace("Ime majke:", "").Trim();
            ViewBag.PrefilledMjesto = linije.FirstOrDefault(l => l.StartsWith("Mjesto:"))?.Replace("Mjesto:", "").Trim();
            ViewBag.PrefilledOdsjek = linije.FirstOrDefault(l => l.StartsWith("Odsjek:"))?.Replace("Odsjek:", "").Trim();
            ViewBag.PrefilledCiklus = linije.FirstOrDefault(l => l.StartsWith("Ciklus:"))?.Replace("Ciklus:", "").Trim();
            ViewBag.PrefilledTipStudija = linije.FirstOrDefault(l => l.StartsWith("Tip studija:"))?.Replace("Tip studija:", "").Trim();
            ViewBag.PrefilledStatus = linije.FirstOrDefault(l => l.StartsWith("Status:"))?.Replace("Status:", "").Trim();
            ViewBag.ObavijestId = obavijestId;

            return View("UnosIzmjena");
        }
    }
}