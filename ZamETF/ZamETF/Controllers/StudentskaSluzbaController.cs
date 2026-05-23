using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;
using ZamETF.Services;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace ZamETF.Controllers
{
    public class StudentskaSluzbaController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public StudentskaSluzbaController(
            UserManager<Korisnik> userManager,
            ApplicationDbContext context,
            EmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }

        // GET: StudentskaSluzba/Index
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);

            var zahtjevi = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .Where(z => !z.Status)
                .OrderByDescending(z => z.Datum)
                .ToListAsync();

            ViewBag.Zahtjevi = zahtjevi;
            return View();
        }

        // GET: StudentskaSluzba/GeneriranjeIzvjestaja/5
        public async Task<IActionResult> GeneriranjeIzvjestaja(int zahtjevId)
        {
            var zahtjev = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            ViewBag.Zahtjev = zahtjev;
            return View();
        }

        // POST: StudentskaSluzba/GenerirajPdf
        [HttpPost]
        public async Task<IActionResult> GenerirajPdf(int zahtjevId, string tipIzvjestaja)
        {
            var zahtjev = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            var student = zahtjev.Student;

            var ocjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Where(o => o.Student.Id == student.Id)
                .ToListAsync();

            byte[] pdf;
            string fileName;

            switch (tipIzvjestaja)
            {
                case "PrepisOcjena":
                    pdf = GenerirajPrepisOcjena(student, ocjene);
                    fileName = $"{student.Ime}{student.Prezime}PrepisOcjena.pdf";
                    break;
                case "OcjenePoGodinama":
                    pdf = GenerirajOcjenePoGodinama(student, ocjene);
                    fileName = $"{student.Ime}{student.Prezime}OcjenePoGodinama.pdf";
                    break;
                case "StatusnaPotvrda":
                    pdf = GenerirajStatusnuPotvrdu(student);
                    fileName = $"{student.Ime}{student.Prezime}StatusnaPotvrda.pdf";
                    break;
                default:
                    return BadRequest();
            }

            return File(pdf, "application/pdf", fileName);
        }

        // POST: StudentskaSluzba/PošaljiNaMail
        [HttpPost]
        public async Task<IActionResult> PošaljiNaMail(int zahtjevId, string tipIzvjestaja)
        {
            var zahtjev = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            var student = zahtjev.Student;

            var ocjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Where(o => o.Student.Id == student.Id)
                .ToListAsync();

            byte[] pdf;
            string fileName;

            switch (tipIzvjestaja)
            {
                case "PrepisOcjena":
                    pdf = GenerirajPrepisOcjena(student, ocjene);
                    fileName = $"{student.Ime}{student.Prezime}PrepisOcjena.pdf";
                    break;
                case "OcjenePoGodinama":
                    pdf = GenerirajOcjenePoGodinama(student, ocjene);
                    fileName = $"{student.Ime}{student.Prezime}OcjenePoGodinama.pdf";
                    break;
                case "StatusnaPotvrda":
                    pdf = GenerirajStatusnuPotvrdu(student);
                    fileName = $"{student.Ime}{student.Prezime}StatusnaPotvrda.pdf";
                    break;
                default:
                    return BadRequest();
            }

            try
            {
                await _emailService.PošaljiEmail(
                    student.Email,
                    $"{student.Ime} {student.Prezime}",
                    $"ZamETF – {tipIzvjestaja}",
                    pdf,
                    fileName);

                // Označi zahtjev kao obrađen
                zahtjev.Status = true;
                await _context.SaveChangesAsync();

                // Pošalji obavijest studentu
                var korisnik = await _userManager.GetUserAsync(User);
                var obavijest = new Obavijest
                {
                    Naslov = $"Vaš dokument je spreman!",
                    Poruka = $"Studentska služba: vaš {tipIzvjestaja} je poslan na email {student.Email}.",
                    PošiljalacId = korisnik.Id,
                    PrimalacId = student.Id,
                    ZahtjevId = zahtjev.Id,
                    DatumSlanja = DateTime.Now
                };
                _context.Obavijesti.Add(obavijest);
                await _context.SaveChangesAsync();

                TempData["Uspjeh"] = $"Dokument poslan na {student.Email}!";
            }
            catch (Exception ex)
            {
                TempData["Greska"] = $"Greška pri slanju emaila: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: StudentskaSluzba/ProslijediAdminu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProslijediAdminu(int zahtjevId)
        {
            var korisnik = await _userManager.GetUserAsync(User);

            var zahtjev = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            var admin = await _context.Administratori.FirstOrDefaultAsync();
            if (admin == null)
            {
                TempData["Greska"] = "Administrator nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            var obavijest = new Obavijest
            {
                Naslov = $"Zahtjev: {zahtjev.TipDokumenta} za studenta {zahtjev.Student.Ime} {zahtjev.Student.Prezime}",
                Poruka = $"Studentska služba prosljeđuje zahtjev za {zahtjev.TipDokumenta} za studenta {zahtjev.Student.Ime} {zahtjev.Student.Prezime} (indeks: {zahtjev.Student.Indeks}).",
                PošiljalacId = korisnik.Id,
                PrimalacId = admin.Id,
                ZahtjevId = zahtjev.Id,
                DatumSlanja = DateTime.Now
            };

            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev je proslijeđen administratoru!";
            return RedirectToAction(nameof(Index));
        }

        // GET: StudentskaSluzba/SviZahtjevi
        public async Task<IActionResult> SviZahtjevi()
        {
            var zahtjevi = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .OrderByDescending(z => z.Datum)
                .ToListAsync();
            return View(zahtjevi);
        }

        // ==================== PDF GENERATORI ====================

        private byte[] GenerirajPrepisOcjena(Student student, List<Ocjena> ocjene)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("UNIVERZITET U SARAJEVU")
                .SetFontSize(14).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph("ELEKTROTEHNIČKI FAKULTET")
                .SetFontSize(12).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph("PREPIS OCJENA")
                .SetFontSize(16).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph(" "));

            doc.Add(new Paragraph($"Ime i prezime: {student.Ime} {student.Prezime}").SetFontSize(11));
            doc.Add(new Paragraph($"Broj indeksa: {student.Indeks}").SetFontSize(11));
            doc.Add(new Paragraph($"Datum: {DateTime.Now:dd.MM.yyyy}").SetFontSize(11));
            doc.Add(new Paragraph(" "));

            if (ocjene.Any())
            {
                var tabela = new Table(3).UseAllAvailableWidth();
                tabela.AddHeaderCell("Predmet");
                tabela.AddHeaderCell("Ocjena");
                tabela.AddHeaderCell("Status");

                foreach (var o in ocjene)
                {
                    tabela.AddCell(o.Predmet?.Naziv ?? "N/A");
                    tabela.AddCell(o.Vrijednost.ToString());
                    tabela.AddCell(o.Vrijednost >= 6 ? "Položen" : "Nije položen");
                }
                doc.Add(tabela);

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"Prosječna ocjena: {ocjene.Average(o => o.Vrijednost):F2}")
                    .SetFontSize(11).SetBold());
            }
            else
            {
                doc.Add(new Paragraph("Nema unesenih ocjena.").SetFontSize(11));
            }

            doc.Close();
            return ms.ToArray();
        }

        private byte[] GenerirajOcjenePoGodinama(Student student, List<Ocjena> ocjene)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("PREPIS OCJENA PO GODINAMA")
                .SetFontSize(16).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph($"Student: {student.Ime} {student.Prezime} | Indeks: {student.Indeks}")
                .SetFontSize(11));
            doc.Add(new Paragraph(" "));

            // Grupiši po godini studija
            for (int godina = 1; godina <= student.GodinaStudija; godina++)
            {
                doc.Add(new Paragraph($"{godina}. godina studija")
                    .SetFontSize(13).SetBold());

                var ocjeneZaGodinu = ocjene
                    .Skip((godina - 1) * 6)
                    .Take(6)
                    .ToList();

                if (ocjeneZaGodinu.Any())
                {
                    var tabela = new Table(2).UseAllAvailableWidth();
                    tabela.AddHeaderCell("Predmet");
                    tabela.AddHeaderCell("Ocjena");
                    foreach (var o in ocjeneZaGodinu)
                    {
                        tabela.AddCell(o.Predmet?.Naziv ?? "N/A");
                        tabela.AddCell(o.Vrijednost.ToString());
                    }
                    doc.Add(tabela);
                }
                else
                {
                    doc.Add(new Paragraph("Nema ocjena za ovu godinu.").SetFontSize(10));
                }
                doc.Add(new Paragraph(" "));
            }

            doc.Close();
            return ms.ToArray();
        }

        private byte[] GenerirajStatusnuPotvrdu(Student student)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("UNIVERZITET U SARAJEVU")
                .SetFontSize(14).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph("ELEKTROTEHNIČKI FAKULTET")
                .SetFontSize(12).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph("POTVRDA O STATUSU STUDENTA")
                .SetFontSize(16).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph(" "));

            doc.Add(new Paragraph($"Potvrđuje se da je {student.Ime} {student.Prezime}, " +
                $"broj indeksa {student.Indeks}, student/ica " +
                $"Elektrotehničkog fakulteta Univerziteta u Sarajevu, " +
                $"{student.GodinaStudija}. godina studija.")
                .SetFontSize(12));

            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph($"Datum izdavanja: {DateTime.Now:dd.MM.yyyy}")
                .SetFontSize(11));
            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph("_______________________")
                .SetFontSize(11));
            doc.Add(new Paragraph("Studentska služba ETF")
                .SetFontSize(11));

            doc.Close();
            return ms.ToArray();
        }
        // POST: StudentskaSluzba/ObradiZahtjev
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObradiZahtjev(int zahtjevId)
        {
            var zahtjev = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            // Označi zahtjev kao obrađen
            zahtjev.Status = true;
            await _context.SaveChangesAsync();

            // Pošalji obavijest studentu
            var korisnik = await _userManager.GetUserAsync(User);
            var student = zahtjev.Student;

            if (student != null)
            {
                var obavijest = new Obavijest
                {
                    Naslov = "Vaš zahtjev je obrađen!",
                    Poruka = $"Studentska služba: vaš zahtjev za {zahtjev.TipDokumenta} je gotov!",
                    PošiljalacId = korisnik.Id,
                    PrimalacId = student.Id,
                    ZahtjevId = zahtjev.Id,
                    DatumSlanja = DateTime.Now
                };
                _context.Obavijesti.Add(obavijest);
                await _context.SaveChangesAsync();
            }

            TempData["Uspjeh"] = "Zahtjev označen kao obrađen, student obaviješten!";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> OznaciObradenim(int obavijestId)
        {
            var obavijest = await _context.Obavijesti
                .Include(o => o.Zahtjev)
                    .ThenInclude(z => z.Student)
                .FirstOrDefaultAsync(o => o.Id == obavijestId);

            if (obavijest == null) return NotFound();

            // Označi obavijest kao pročitanu
            obavijest.Procitana = true;

            // Označi zahtjev kao obrađen
            if (obavijest.Zahtjev != null)
                obavijest.Zahtjev.Status = true;

            await _context.SaveChangesAsync();

            // Pošalji obavijest studentu da je zahtjev obrađen
            var korisnik = await _userManager.GetUserAsync(User);
            var student = obavijest.Zahtjev?.Student;

            if (student != null)
            {
                var obavijestStudentu = new Obavijest
                {
                    Naslov = "Vaš zahtjev je obrađen!",
                    Poruka = $"Studentska služba: vaš zahtjev za {obavijest.Zahtjev?.TipDokumenta} je gotov!",
                    PošiljalacId = korisnik.Id,
                    PrimalacId = student.Id,
                    ZahtjevId = obavijest.ZahtjevId,
                    DatumSlanja = DateTime.Now
                };
                _context.Obavijesti.Add(obavijestStudentu);
                await _context.SaveChangesAsync();
            }

            TempData["Uspjeh"] = "Zahtjev označen kao obrađen, student obaviješten!";
            return RedirectToAction(nameof(Index));
        }
        // ==================== NOVE AKCIJE ====================

        // GET: StudentskaSluzba/OdaberiZahtjevZaIzvjestaj
        public async Task<IActionResult> OdaberiZahtjevZaIzvjestaj()
        {
            var zahtjevi = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .OrderByDescending(z => z.Datum)
                .ToListAsync();

            return View(zahtjevi);
        }

        // GET: StudentskaSluzba/ZahtjevStatistika
        public IActionResult ZahtjevStatistika()
        {
            return View();
        }

        // POST: StudentskaSluzba/PosaljiZahtjevStatistike
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiZahtjevStatistike(string tipStatistike, string opisZahtjeva, string periodOd, string periodDo)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var admin = await _context.Administratori.FirstOrDefaultAsync();

            if (admin == null)
            {
                TempData["Greska"] = "Administrator nije pronađen.";
                return RedirectToAction(nameof(ZahtjevStatistika));
            }

            var obavijest = new Obavijest
            {
                Naslov = "Zahtjev za statistiku: " + tipStatistike,
                Poruka = "Tip: " + tipStatistike + "\nPeriod: " + periodOd + " - " + periodDo + "\nOpis: " + opisZahtjeva,
                PošiljalacId = korisnik.Id,
                PrimalacId = admin.Id,
                DatumSlanja = DateTime.Now
            };

            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev za statistiku je poslan administratoru!";
            return RedirectToAction(nameof(ZahtjevStatistika));
        }

        // GET: StudentskaSluzba/ZahtjevPodaci
        public IActionResult ZahtjevPodaci()
        {
            return View();
        }

        // POST: StudentskaSluzba/PosaljiZahtjevPodataka
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiZahtjevPodataka(
            string tipZahtjeva,
            string imeStudenta,
            string prezimeStudenta,
            string indeksStudenta,
            string godinaStudija,
            string emailStudenta,
            string smjer,
            string napomena)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var admin = await _context.Administratori.FirstOrDefaultAsync();

            if (admin == null)
            {
                TempData["Greska"] = "Administrator nije pronađen.";
                return RedirectToAction(nameof(ZahtjevPodaci));
            }

            var poruka = "Tip zahtjeva: " + tipZahtjeva + "\n" +
                         "Indeks: " + indeksStudenta + "\n" +
                         "Ime: " + imeStudenta + "\n" +
                         "Prezime: " + prezimeStudenta + "\n" +
                         "Email: " + emailStudenta + "\n" +
                         "Godina studija: " + godinaStudija + "\n" +
                         "Smjer: " + smjer + "\n" +
                         "Napomena: " + napomena;

            var obavijest = new Obavijest
            {
                Naslov = "Zahtjev za " + tipZahtjeva + " podataka - " + indeksStudenta,
                Poruka = poruka,
                PošiljalacId = korisnik.Id,
                PrimalacId = admin.Id,
                DatumSlanja = DateTime.Now
            };

            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev za " + tipZahtjeva + " je poslan administratoru!";
            return RedirectToAction(nameof(ZahtjevPodaci));
        }

        // POST: StudentskaSluzba/PosaljiNaMail (bez sumera u nazivu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiNaMail(int zahtjevId, string tipIzvjestaja)
        {
            var zahtjev = await _context.ZahtjeviDokumenata
                .Include(z => z.Student)
                .FirstOrDefaultAsync(z => z.Id == zahtjevId);

            if (zahtjev == null) return NotFound();

            var student = zahtjev.Student;

            var ocjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Where(o => o.Student.Id == student.Id)
                .ToListAsync();

            byte[] pdf;
            string fileName;

            switch (tipIzvjestaja)
            {
                case "PrepisOcjena":
                    pdf = GenerirajPrepisOcjena(student, ocjene);
                    fileName = student.Ime + student.Prezime + "PrepisOcjena.pdf";
                    break;
                case "OcjenePoGodinama":
                    pdf = GenerirajOcjenePoGodinama(student, ocjene);
                    fileName = student.Ime + student.Prezime + "OcjenePoGodinama.pdf";
                    break;
                case "StatusnaPotvrda":
                    pdf = GenerirajStatusnuPotvrdu(student);
                    fileName = student.Ime + student.Prezime + "StatusnaPotvrda.pdf";
                    break;
                default:
                    return BadRequest();
            }

            try
            {
                await _emailService.PošaljiEmail(
                    student.Email,
                    student.Ime + " " + student.Prezime,
                    "ZamETF - " + tipIzvjestaja,
                    pdf,
                    fileName);

                zahtjev.Status = true;
                await _context.SaveChangesAsync();

                var korisnik = await _userManager.GetUserAsync(User);
                var obavijest = new Obavijest
                {
                    Naslov = "Vas dokument je spreman!",
                    Poruka = "Studentska sluzba: vas " + tipIzvjestaja + " je poslan na email " + student.Email + ".",
                    PošiljalacId = korisnik.Id,
                    PrimalacId = student.Id,
                    ZahtjevId = zahtjev.Id,
                    DatumSlanja = DateTime.Now
                };
                _context.Obavijesti.Add(obavijest);
                await _context.SaveChangesAsync();

                TempData["Uspjeh"] = "Dokument poslan na " + student.Email + "!";
            }
            catch (Exception ex)
            {
                TempData["Greska"] = "Greska pri slanju emaila: " + ex.Message;
            }

            return RedirectToAction(nameof(SviZahtjevi));
        }
        // GET: StudentskaSluzba/Notifikacije
        public async Task<IActionResult> Notifikacije()
        {
            var studenti = await _context.Users
                .OfType<Student>()
                .OrderBy(s => s.Prezime)
                .ToListAsync();

            ViewBag.Studenti = studenti;
            return View();
        }

        // POST: StudentskaSluzba/PosaljiNotifikaciju
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiNotifikaciju(
            string naslov,
            string poruka,
            string tipNotifikacije,
            string primateljTip,
            string odabraniIds,
            string odabraneGodine)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            int brojPoslanih = 0;

            var naslovSaTipom = "[" + tipNotifikacije + "] " + naslov;

            if (primateljTip == "student")
            {
                // Jedan ili vise specificnih studenata
                if (string.IsNullOrEmpty(odabraniIds))
                {
                    TempData["Greska"] = "Niste odabrali nijednog studenta.";
                    return RedirectToAction(nameof(Notifikacije));
                }

                var ids = odabraniIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in ids)
                {
                    if (int.TryParse(idStr, out int studentId))
                    {
                        var obavijest = new Obavijest
                        {
                            Naslov = naslovSaTipom,
                            Poruka = poruka,
                            PošiljalacId = korisnik.Id,
                            PrimalacId = studentId,
                            DatumSlanja = DateTime.Now
                        };
                        _context.Obavijesti.Add(obavijest);
                        brojPoslanih++;
                    }
                }
            }
            else if (primateljTip == "godina")
            {
                // Grupa po godini studija
                if (string.IsNullOrEmpty(odabraneGodine))
                {
                    TempData["Greska"] = "Niste odabrali nijednu godinu.";
                    return RedirectToAction(nameof(Notifikacije));
                }

                var godine = odabraneGodine.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(g => int.Parse(g))
                                           .ToList();

                var studenti = await _context.Users
                    .OfType<Student>()
                    .Where(s => godine.Contains(s.GodinaStudija))
                    .ToListAsync();

                foreach (var student in studenti)
                {
                    var obavijest = new Obavijest
                    {
                        Naslov = naslovSaTipom,
                        Poruka = poruka,
                        PošiljalacId = korisnik.Id,
                        PrimalacId = student.Id,
                        DatumSlanja = DateTime.Now
                    };
                    _context.Obavijesti.Add(obavijest);
                    brojPoslanih++;
                }
            }
            else if (primateljTip == "svi")
            {
                // Svi studenti
                var studenti = await _context.Users
                    .OfType<Student>()
                    .ToListAsync();

                foreach (var student in studenti)
                {
                    var obavijest = new Obavijest
                    {
                        Naslov = naslovSaTipom,
                        Poruka = poruka,
                        PošiljalacId = korisnik.Id,
                        PrimalacId = student.Id,
                        DatumSlanja = DateTime.Now
                    };
                    _context.Obavijesti.Add(obavijest);
                    brojPoslanih++;
                }
            }
            else if (primateljTip == "admin")
            {
                // Admin
                var admin = await _context.Administratori.FirstOrDefaultAsync();
                if (admin == null)
                {
                    TempData["Greska"] = "Administrator nije pronađen.";
                    return RedirectToAction(nameof(Notifikacije));
                }

                var obavijest = new Obavijest
                {
                    Naslov = naslovSaTipom,
                    Poruka = poruka,
                    PošiljalacId = korisnik.Id,
                    PrimalacId = admin.Id,
                    DatumSlanja = DateTime.Now
                };
                _context.Obavijesti.Add(obavijest);
                brojPoslanih++;
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Notifikacija je uspjesno poslana! Broj primatelja: " + brojPoslanih;
            return RedirectToAction(nameof(Notifikacije));
        }
        public async Task<IActionResult> ZahtjeviProfesora()
        {
            var zahtjevi = await _context.Obavijesti
                .Where(o => o.Naslov.StartsWith("Zahtjev profesora"))
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();
            ViewBag.Zahtjevi = zahtjevi;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OznaciZahtjevProfesora(int obavijestId)
        {
            var obavijest = await _context.Obavijesti.FindAsync(obavijestId);
            if (obavijest == null) return NotFound();
            obavijest.Procitana = true;
            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Zahtjev oznacen kao obradjeno!";
            return RedirectToAction(nameof(ZahtjeviProfesora));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProslijediZahtjevProfesora(int obavijestId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var obavijest = await _context.Obavijesti.FindAsync(obavijestId);
            if (obavijest == null) return NotFound();
            var admin = await _context.Administratori.FirstOrDefaultAsync();
            if (admin == null) { TempData["Greska"] = "Admin nije pronadjen."; return RedirectToAction(nameof(ZahtjeviProfesora)); }
            _context.Obavijesti.Add(new Obavijest
            {
                Naslov = obavijest.Naslov,
                Poruka = obavijest.Poruka,
                PošiljalacId = korisnik.Id,
                PrimalacId = admin.Id,
                DatumSlanja = DateTime.Now
            });
            obavijest.Procitana = true;
            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Zahtjev proslijedjen adminu!";
            return RedirectToAction(nameof(ZahtjeviProfesora));
        }
    }
}