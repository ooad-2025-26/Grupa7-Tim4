using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace ZamETF.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public AdministratorController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

        // POST: Administrator/IzmijeniKorisnika
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzmijeniKorisnika(int id, string ime, string prezime,
            string username, string email, string indeks, string odsjek,
            int semestar, int godinaStudija)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null) return NotFound();

            student.Ime = ime;
            student.Prezime = prezime;
            student.UserName = username;
            student.Email = email;
            student.Indeks = indeks;
            student.GodinaStudija = godinaStudija;

            await _userManager.UpdateAsync(student);
            TempData["Uspjeh"] = "Podaci su uspješno izmijenjeni!";
            return RedirectToAction(nameof(UnosIzmjena));
        }

        // POST: Administrator/KreirajKorisnika
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreirajKorisnika(string ime, string prezime,
    string indeks, int godinaStudija, string privilegije, string lozinka)
        {
            // Automatski generiši email i username
            var email = await GenerirajEmail(ime, prezime);
            var username = await GenerirajUsername(ime, prezime);

            IdentityResult result;

            if (privilegije == "Student")
            {
                var student = new Student
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = email,
                    Indeks = indeks,
                    GodinaStudija = godinaStudija,
                    Uloga = Uloga.Student,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(student, lozinka);
            }
            else if (privilegije == "Profesor")
            {
                var profesor = new Profesor
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = email,
                    Uloga = Uloga.Profesor,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(profesor, lozinka);
            }
            else if (privilegije == "Administrator")
            {
                var admin = new Administrator
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = email,
                    Uloga = Uloga.Administrator,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(admin, lozinka);
            }
            else
            {
                var sluzba = new StudentskaSluzba
                {
                    Ime = ime,
                    Prezime = prezime,
                    UserName = username,
                    Email = email,
                    Uloga = Uloga.StudentskaSluzba,
                    EmailConfirmed = true
                };
                result = await _userManager.CreateAsync(sluzba, lozinka);
            }

            if (result.Succeeded)
            {
                TempData["Uspjeh"] = $"Korisnik kreiran! Email: {email} | Username: {username} | Lozinka: {lozinka}";
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
        // GET: Administrator/Statistika
        public async Task<IActionResult> Statistika()
        {
            var korisnik = await _userManager.GetUserAsync(User);

            // Zahtjevi od studentske sluzbe
            var zahtjevi = await _context.Obavijesti
                .Include(o => o.Posiljалac)
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .Where(o => o.PrimalacId == korisnik.Id && !o.Procitana)
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            // Za manualno
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

            // Označi obavijest kao pročitanu
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

            // Pošalji obavijest studentskoj službi
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

            // Označi originalni zahtjev kao obrađen
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

        private byte[] GenerirајPdfStudenta(Student student,
            List<Ocjena> ocjene, List<Prisustvo> prisustva)
        {
            using var ms = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            // Naslov
            doc.Add(new iText.Layout.Element.Paragraph($"Statistika za studenta: {student.Ime} {student.Prezime}")
                .SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Indeks: {student.Indeks}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Godina studija: {student.GodinaStudija}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // Ocjene
            doc.Add(new iText.Layout.Element.Paragraph("OCJENE")
                .SetFontSize(14).SetBold());

            if (ocjene.Any())
            {
                var tabela = new iText.Layout.Element.Table(3).UseAllAvailableWidth();
                tabela.AddHeaderCell("Predmet");
                tabela.AddHeaderCell("Ocjena");
                tabela.AddHeaderCell("Datum");

                foreach (var o in ocjene)
                {
                    tabela.AddCell(o.Predmet?.Naziv ?? "N/A");
                    tabela.AddCell(o.Vrijednost.ToString());
                    tabela.AddCell("-");
                }

                doc.Add(tabela);

                var prosjek = ocjene.Average(o => o.Vrijednost);
                doc.Add(new iText.Layout.Element.Paragraph($"Prosječna ocjena: {prosjek:F2}")
                    .SetFontSize(12).SetBold());
            }
            else
            {
                doc.Add(new iText.Layout.Element.Paragraph("Nema unesenih ocjena.")
                    .SetFontSize(12));
            }

            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // Prisustvo
            doc.Add(new iText.Layout.Element.Paragraph("PRISUSTVO")
                .SetFontSize(14).SetBold());

            if (prisustva.Any())
            {
                var ukupno = prisustva.Count;
                var prisutno = prisustva.Count(p => p.Prisutan);
                var posto = (double)prisutno / ukupno * 100;

                var tabela2 = new iText.Layout.Element.Table(2).UseAllAvailableWidth();
                tabela2.AddHeaderCell("Predmet");
                tabela2.AddHeaderCell("Prisustvo");

                var grupaPoPredmetu = prisustva.GroupBy(p => p.Predmet?.Naziv ?? "N/A");
                foreach (var grupa in grupaPoPredmetu)
                {
                    var ukupnoGrupa = grupa.Count();
                    var prisutnoGrupa = grupa.Count(p => p.Prisutan);
                    tabela2.AddCell(grupa.Key);
                    tabela2.AddCell($"{prisutnoGrupa}/{ukupnoGrupa} ({(double)prisutnoGrupa / ukupnoGrupa * 100:F0}%)");
                }

                doc.Add(tabela2);
                doc.Add(new iText.Layout.Element.Paragraph($"Ukupno prisustvo: {prisutno}/{ukupno} ({posto:F0}%)")
                    .SetFontSize(12).SetBold());
            }
            else
            {
                doc.Add(new iText.Layout.Element.Paragraph("Nema evidencije prisustva.")
                    .SetFontSize(12));
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

            doc.Add(new iText.Layout.Element.Paragraph($"Statistika za profesora: {profesor.Ime} {profesor.Prezime}")
                .SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Titula: {profesor.Titula}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // Predmeti
            doc.Add(new iText.Layout.Element.Paragraph("PREDMETI")
                .SetFontSize(14).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Ukupan broj predmeta: {profesor.Predmeti?.Count ?? 0}")
                .SetFontSize(12));

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

            // Zadace
            doc.Add(new iText.Layout.Element.Paragraph("ZADAĆE")
                .SetFontSize(14).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Ukupan broj kreiranih zadaća: {zadace.Count}")
                .SetFontSize(12));

            var ocijenjene = zadace.SelectMany(z => z.Predaje ?? new List<PredajaZadace>())
                .Count(p => p.Status == StatusZadace.Pregledana);
            doc.Add(new iText.Layout.Element.Paragraph($"Ukupno ocijenjenih predaja: {ocijenjene}")
                .SetFontSize(12));

            doc.Close();
            return ms.ToArray();
        }

        private byte[] GenerirajPdfStudentskeSluzbe(List<ZahtjevZaDokument> zahtjevi)
        {
            using var ms = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph("Statistika studentske službe")
                .SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Datum generisanja: {DateTime.Now:dd.MM.yyyy}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            doc.Add(new iText.Layout.Element.Paragraph($"Ukupan broj zahtjeva: {zahtjevi.Count}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Obrađenih zahtjeva: {zahtjevi.Count(z => z.Status)}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Neobrađenih zahtjeva: {zahtjevi.Count(z => !z.Status)}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // Najzastupljeniji tipovi
            doc.Add(new iText.Layout.Element.Paragraph("TIPOVI ZAHTJEVA")
                .SetFontSize(14).SetBold());

            var tipoviGrupa = zahtjevi.GroupBy(z => z.TipDokumenta)
                .OrderByDescending(g => g.Count());

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

            doc.Add(new iText.Layout.Element.Paragraph($"Statistika za predmet: {predmet.Naziv}")
                .SetFontSize(16).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph($"Šifra predmeta: {predmet.SifraPredmeta}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph($"Profesor: {predmet.Profesor?.Ime} {predmet.Profesor?.Prezime}")
                .SetFontSize(12));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // Studenti
            doc.Add(new iText.Layout.Element.Paragraph($"Broj upisanih studenata: {predmet.Studenti?.Count ?? 0}")
                .SetFontSize(12).SetBold());
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            // Statistika ocjena
            doc.Add(new iText.Layout.Element.Paragraph("STATISTIKA OCJENA")
                .SetFontSize(14).SetBold());

            if (predmet.Ocjene != null && predmet.Ocjene.Any())
            {
                var prosjek = predmet.Ocjene.Average(o => o.Vrijednost);
                var prolaznost = predmet.Ocjene.Count(o => o.Vrijednost >= 6) * 100.0 / predmet.Ocjene.Count;

                doc.Add(new iText.Layout.Element.Paragraph($"Prosječna ocjena: {prosjek:F2}")
                    .SetFontSize(12));
                doc.Add(new iText.Layout.Element.Paragraph($"Prolaznost: {prolaznost:F0}%")
                    .SetFontSize(12));

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
                doc.Add(new iText.Layout.Element.Paragraph("Nema unesenih ocjena.")
                    .SetFontSize(12));
            }

            doc.Close();
            return ms.ToArray();
        }

    }
}