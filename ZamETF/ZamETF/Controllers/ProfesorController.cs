using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;
using ZamETF.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ZamETF.Controllers
{
    [Authorize(Roles = "Profesor")]
    public class ProfesorController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfesorController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> UnosOcjena(int predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var predmet = profesor?.Predmeti.FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null) return NotFound();

            var studenti = await _context.UpisaNaPredmet
                .Include(u => u.Student)
                .Where(u => u.PredmetId == predmetId)
                .Select(u => u.Student)
                .ToListAsync();

            var bodovanjaIspit = await _context.BodovanjaIspit
                .Where(b => b.PredmetId == predmetId)
                .ToListAsync();

            var ocjene = await _context.Ocjene
                .Where(o => o.PredmetId == predmetId)
                .ToListAsync();

            // Bodovi iz zadaća = suma ocijenjenih predaja
            var predajeZadaca = await _context.PredajeZadace
                .Include(p => p.Zadaca)
                .Where(p => p.Zadaca.PredmetID == predmetId && p.Bodovi.HasValue)
                .ToListAsync();

            var model = new UnosOcjenaVM
            {
                Predmet = predmet,
                Studenti = studenti.OrderBy(s => s.Prezime).Select(s => new StudentBodVM
                {
                    StudentId = s.Id,
                    ImePrezime = s.Ime + " " + s.Prezime,
                    Indeks = s.Indeks,
                    BodoviZadace = predajeZadaca
                        .Where(p => p.StudentID == s.Id)
                        .Sum(p => p.Bodovi ?? 0),
                    BodoviParcijalni = bodovanjaIspit.FirstOrDefault(b => b.StudentId == s.Id && b.Tip == TipIspita.Parcijalni)?.Bodovi,
                    BodoviZavrsni = bodovanjaIspit.FirstOrDefault(b => b.StudentId == s.Id && b.Tip == TipIspita.Zavrsni)?.Bodovi,
                    BodoviIntegralni = bodovanjaIspit.FirstOrDefault(b => b.StudentId == s.Id && b.Tip == TipIspita.Integralni)?.Bodovi,
                    BodoviTeorija = bodovanjaIspit.FirstOrDefault(b => b.StudentId == s.Id && b.Tip == TipIspita.Teorija)?.Bodovi,
                    FinalnaOcjena = ocjene.FirstOrDefault(o => o.StudentId == s.Id)?.Vrijednost
                }).ToList()
            };

            ViewBag.Predmeti = profesor.Predmeti.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajBodovanjeIspit(int predmetId,
            List<int> studentId,
            List<string> bodoviParcijalni,
            List<string> bodoviZavrsni,
            List<string> bodoviIntegralni,
            List<string> bodoviTeorija)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            var postojecaBodovanja = await _context.BodovanjaIspit
                .Where(b => b.PredmetId == predmetId)
                .ToListAsync();

            for (int i = 0; i < studentId.Count; i++)
            {
                var sid = studentId[i];
                var student = await _context.Studenti.FindAsync(sid);
                if (student == null) continue;

                await SpremiIspitBodove(predmetId, sid, student, predmet, postojecaBodovanja,
                    TipIspita.Parcijalni, i < bodoviParcijalni.Count ? bodoviParcijalni[i] : null);
                await SpremiIspitBodove(predmetId, sid, student, predmet, postojecaBodovanja,
                    TipIspita.Zavrsni, i < bodoviZavrsni.Count ? bodoviZavrsni[i] : null);
                await SpremiIspitBodove(predmetId, sid, student, predmet, postojecaBodovanja,
                    TipIspita.Integralni, i < bodoviIntegralni.Count ? bodoviIntegralni[i] : null);
                await SpremiIspitBodove(predmetId, sid, student, predmet, postojecaBodovanja,
                    TipIspita.Teorija, i < bodoviTeorija.Count ? bodoviTeorija[i] : null);
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Bodovi iz ispita su sačuvani.";
            return RedirectToAction(nameof(UnosOcjena), new { predmetId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajFinalneOcjene(int predmetId,
            List<int> studentId,
            List<string> finalnaOcjena)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            var postojeceOcjene = await _context.Ocjene
                .Where(o => o.PredmetId == predmetId)
                .ToListAsync();

            var korisnik = await _userManager.GetUserAsync(User);

            for (int i = 0; i < studentId.Count; i++)
            {
                var sid = studentId[i];
                var ocjenaRaw = i < finalnaOcjena.Count ? finalnaOcjena[i] : null;
                if (string.IsNullOrWhiteSpace(ocjenaRaw)) continue;
                if (!int.TryParse(ocjenaRaw, out int ocjenaVrijednost)) continue;
                ocjenaVrijednost = Math.Clamp(ocjenaVrijednost, 5, 10);

                var student = await _context.Studenti.FindAsync(sid);
                if (student == null) continue;

                var postojecaOcjena = postojeceOcjene.FirstOrDefault(o => o.StudentId == sid);
                if (postojecaOcjena != null)
                {
                    postojecaOcjena.Vrijednost = ocjenaVrijednost;
                    postojecaOcjena.JeFinalna = true;
                    postojecaOcjena.DatumUnosa = DateTime.Now;
                }
                else
                {
                    _context.Ocjene.Add(new Ocjena
                    {
                        StudentId = sid,
                        Student = student,
                        PredmetId = predmetId,
                        Predmet = predmet,
                        Vrijednost = ocjenaVrijednost,
                        JeFinalna = true,
                        DatumUnosa = DateTime.Now
                    });
                }

                // Obavijest studentu
                _context.Obavijesti.Add(new Obavijest
                {
                    Naslov = $"[Obavijest] Unesena ocjena — {predmet.Naziv}",
                    Poruka = $"Vaša finalna ocjena iz predmeta {predmet.Naziv} je {ocjenaVrijednost}.",
                    PošiljalacId = korisnik.Id,
                    PrimalacId = sid,
                    DatumSlanja = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Finalne ocjene su sačuvane.";
            return RedirectToAction(nameof(UnosOcjena), new { predmetId });
        }

        private async Task SpremiIspitBodove(int predmetId, int studentId, Student student,
            Predmet predmet, List<BodovanjeIspit> postojeca, TipIspita tip, string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;
            if (!int.TryParse(raw, out int bodovi)) return;
            bodovi = Math.Clamp(bodovi, 0, 100);

            var zapis = postojeca.FirstOrDefault(b => b.StudentId == studentId && b.Tip == tip);
            if (zapis != null)
            {
                zapis.Bodovi = bodovi;
                zapis.DatumUnosa = DateTime.Now;
            }
            else
            {
                var novi = new BodovanjeIspit
                {
                    StudentId = studentId,
                    Student = student,
                    PredmetId = predmetId,
                    Predmet = predmet,
                    Tip = tip,
                    Bodovi = bodovi,
                    DatumUnosa = DateTime.Now
                };
                _context.BodovanjaIspit.Add(novi);
                postojeca.Add(novi);
            }
        }
        
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajBodovanje(int predmetId, List<int> studentId, List<string> bodovi)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            var postojeca = await _context.Bodovanja
                .Where(b => b.PredmetId == predmetId)
                .ToListAsync();

            for (int i = 0; i < studentId.Count; i++)
            {
                var sid = studentId[i];
                var raw = (i < bodovi.Count) ? bodovi[i] : null;

                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (!int.TryParse(raw, out var b)) continue;
                b = Math.Clamp(b, 0, 100);

                var zapis = postojeca.FirstOrDefault(x => x.StudentId == sid);
                if (zapis != null)
                    zapis.Bodovi = b;
                else
                    _context.Bodovanja.Add(new Bodovanje
                    {
                        StudentId = sid,
                        PredmetId = predmetId,
                        Bodovi = b
                    });
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Bodovi su uspješno sačuvani.";
            return RedirectToAction(nameof(UnosOcjena), new { predmetId });
        }

        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Zadace)
                        .ThenInclude(z => z.Predaje)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            foreach (var predmet in profesor.Predmeti)
            {
                predmet.Studenti = await _context.UpisaNaPredmet
                    .Include(u => u.Student)
                    .Where(u => u.PredmetId == predmet.Id)
                    .Select(u => u.Student)
                    .ToListAsync();
            }

            var obavijesti = await _context.Obavijesti
                .Where(o => o.PrimalacId == profesor.Id)
                .OrderByDescending(o => o.DatumSlanja)
                .Take(10)
                .ToListAsync();

            var predmetIds = profesor.Predmeti.Select(p => p.Id).ToList();

            var zadace = await _context.Zadace
                .Include(z => z.Predmet)
                .Include(z => z.Predaje)
                .Where(z => predmetIds.Contains(z.Predmet.Id))
                .OrderBy(z => z.Rok)
                .Take(10)
                .ToListAsync();

            var ispiti = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.Prijave)
                    .ThenInclude(p => p.Student)
                .Where(i => predmetIds.Contains(i.Predmet.Id))
                .OrderBy(i => i.Datum)
                .Take(10)
                .ToListAsync();

            ViewBag.Profesor = profesor;
            ViewBag.Obavijesti = obavijesti;
            ViewBag.Zadace = zadace;
            ViewBag.Ispiti = ispiti;
            ViewBag.Predmeti = profesor.Predmeti.ToList();

            return View();
        }
        public async Task<IActionResult> PreuzmiPdf(int id)
        {
            var predaja = await _context.PredajeZadace.FindAsync(id);
            if (predaja == null) return NotFound();

            if (predaja.FajlBytes != null)
                return File(predaja.FajlBytes, "application/pdf", predaja.FajlIme ?? "zadaca.pdf");

            return NotFound();
        }
        public async Task<IActionResult> DetaljiPredmeta(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var predmet = profesor?.Predmeti.FirstOrDefault(p => p.Id == id);
            if (predmet == null) return NotFound();

            var predmetSaDetalima = await _context.Predmeti
                .Include(p => p.Zadace)
                    .ThenInclude(z => z.Predaje)
                .Include(p => p.Ocjene)
                .Include(p => p.Prisustva)
                .FirstOrDefaultAsync(p => p.Id == id);

            var studenti = await _context.UpisaNaPredmet
                .Include(u => u.Student)
                .Where(u => u.PredmetId == id)
                .Select(u => u.Student)
                .ToListAsync();

            predmetSaDetalima.Studenti = studenti;

            var ispiti = await _context.Ispiti
                .Include(i => i.Prijave)
                    .ThenInclude(p => p.Student)
                .Where(i => i.Predmet.Id == id)
                .OrderByDescending(i => i.Datum)
                .ToListAsync();

            ViewBag.Ispiti = ispiti;
            ViewBag.Predmeti = profesor.Predmeti.ToList();

            return View(predmetSaDetalima);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreiranjeZadace(string nazividID, string opis, DateTime rok, int predmetId, int maxBodovi = 100)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Zadace)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (predmet == null)
                ModelState.AddModelError("", "Predmet nije pronađen.");

            if (ModelState.IsValid)
            {
                var zadaca = new Zadaca
                {
                    NazivID = nazividID,
                    Opis = opis,
                    Rok = rok,
                    MaxBodovi = maxBodovi,
                    Predmet = predmet
                };

                _context.Zadace.Add(zadaca);
                await _context.SaveChangesAsync();
                TempData["Uspjeh"] = "Zadaća je uspješno kreirana.";
                return RedirectToAction(nameof(Index));
            }

            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori.Include(p => p.Predmeti).FirstOrDefaultAsync(p => p.Id == korisnik.Id);
            ViewBag.Predmeti = profesor?.Predmeti.ToList();
            return View();
        }

        public async Task<IActionResult> KreiranjeIspita(int? predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            var predmetIds = profesor.Predmeti.Select(p => p.Id).ToList();
            var ispiti = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.Prijave)
                .Where(i => predmetIds.Contains(i.Predmet.Id))
                .OrderByDescending(i => i.Datum)
                .ToListAsync();

            ViewBag.Predmeti = profesor.Predmeti.ToList();
            ViewBag.OdabraniPredmetId = predmetId;
            ViewBag.Ispiti = ispiti;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreiranjeIspita(int predmetId, DateTime datum, DateTime rokZaPrijavu)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null)
            {
                TempData["Greska"] = "Predmet nije pronadjen.";
                return RedirectToAction(nameof(KreiranjeIspita));
            }

            var ispit = new Ispit
            {
                Predmet = predmet,
                Datum = datum,
                RokZaPrijavu = rokZaPrijavu
            };

            _context.Ispiti.Add(ispit);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Ispit je uspjesno kreiran!";
            return RedirectToAction(nameof(KreiranjeIspita), new { predmetId });
        }

        public async Task<IActionResult> DetaljiIspita(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                    .ThenInclude(p => p.Studenti)
                .Include(i => i.Prijave)
                    .ThenInclude(p => p.Student)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ispit == null) return NotFound();

            ViewBag.Predmeti = profesor?.Predmeti.ToList() ?? new List<Predmet>();
            return View(ispit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzmijeniIspit(int ispitId, DateTime datum, DateTime rokZaPrijavu)
        {
            var ispit = await _context.Ispiti.FindAsync(ispitId);
            if (ispit == null) return NotFound();

            ispit.Datum = datum;
            ispit.RokZaPrijavu = rokZaPrijavu;
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Ispit je azuriran!";
            return RedirectToAction(nameof(DetaljiIspita), new { id = ispitId });
        }

        public async Task<IActionResult> DetaljiObavijesti(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var obavijest = await _context.Obavijesti
                .FirstOrDefaultAsync(o => o.Id == id && o.PrimalacId == korisnik.Id);

            if (obavijest == null) return NotFound();

            if (!obavijest.Procitana)
            {
                obavijest.Procitana = true;
                await _context.SaveChangesAsync();
            }

            var posiljалac = await _context.Users.FindAsync(obavijest.PošiljalacId) as Korisnik;
            string posiljалacIme = posiljалac != null ? posiljалac.Ime + " " + posiljалac.Prezime : "Sistem";

            var naslov = obavijest.Naslov ?? "";
            int brojSaIstimNaslovom = await _context.Obavijesti
                .CountAsync(o => o.Naslov == naslov && o.DatumSlanja == obavijest.DatumSlanja);
            bool masovnoPoslano = brojSaIstimNaslovom > 1;

            ViewBag.Obavijest = obavijest;
            ViewBag.Posiljалac = posiljалacIme;
            ViewBag.MasovnoPoslano = masovnoPoslano;
            ViewBag.Predmeti = profesor?.Predmeti.ToList() ?? new List<Predmet>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreiranjeZadace(string nazividID, string opis, DateTime rok, int predmetId)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Zadace)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (predmet == null)
                ModelState.AddModelError("", "Predmet nije pronađen.");

            if (ModelState.IsValid)
            {
                var zadaca = new Zadaca
                {
                    NazivID = nazividID,
                    Opis = opis,
                    Rok = rok,
                    Predmet = predmet
                };

                _context.Zadace.Add(zadaca);
                await _context.SaveChangesAsync();
                TempData["Uspjeh"] = "Zadaća je uspješno kreirana.";
                return RedirectToAction(nameof(Index));
            }

            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori.Include(p => p.Predmeti).FirstOrDefaultAsync(p => p.Id == korisnik.Id);
            ViewBag.Predmeti = profesor?.Predmeti.ToList();
            return View();
        }

        public async Task<IActionResult> OcjenjivanjeZadace(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var zadaca = await _context.Zadace
                .Include(z => z.Predmet)
                .Include(z => z.Predaje)
                    .ThenInclude(pz => pz.Student)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zadaca == null) return NotFound();

            ViewBag.AktivniPredmetId = zadaca.Predmet?.Id;
            ViewBag.Predmeti = profesor?.Predmeti.ToList() ?? new List<Predmet>();

            return View(zadaca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajBodove(int predajaId, int bodovi)
        {
            var predaja = await _context.PredajeZadace
                .Include(pz => pz.Zadaca)
                .FirstOrDefaultAsync(pz => pz.Id == predajaId);

            if (predaja == null) return NotFound();

            predaja.Bodovi = bodovi;
            predaja.Status = StatusZadace.Pregledana;
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Bodovi su uspješno sačuvani.";
            return RedirectToAction(nameof(OcjenjivanjeZadace), new { id = predaja.ZadacaId });
        }

        public async Task<IActionResult> EvidencijaPrisustva(int predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Prisustva)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var predmet = profesor?.Predmeti.FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null) return NotFound();

            // Učitaj studente kroz UpisaNaPredmet
            var studenti = await _context.UpisaNaPredmet
                .Include(u => u.Student)
                .Where(u => u.PredmetId == predmetId)
                .Select(u => u.Student)
                .ToListAsync();

            predmet.Studenti = studenti;

            ViewBag.DatumDanas = DateTime.Today;
            return View(predmet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajPrisustvo(int predmetId, DateTime datum, List<int> prisutniIds)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Prisustva)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (predmet == null) return NotFound();

            // Učitaj studente kroz UpisaNaPredmet
            var studenti = await _context.UpisaNaPredmet
                .Include(u => u.Student)
                .Where(u => u.PredmetId == predmetId)
                .Select(u => u.Student)
                .ToListAsync();

            foreach (var student in studenti)
            {
                var postojece = predmet.Prisustva
                    .FirstOrDefault(pr => pr.Student.Id == student.Id && pr.Datum.Date == datum.Date);

                if (postojece != null)
                {
                    postojece.Prisutan = prisutniIds.Contains(student.Id);
                }
                else
                {
                    _context.Prisustva.Add(new Prisustvo
                    {
                        Student = student,
                        Predmet = predmet,
                        Datum = datum,
                        Prisutan = prisutniIds.Contains(student.Id)
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = $"Prisustvo za {datum:dd.MM.yyyy} je uspješno evidentirano.";
            return RedirectToAction(nameof(EvidencijaPrisustva), new { predmetId });
        }

        public async Task<IActionResult> ZahtjevZaDokument()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);
            if (profesor == null) return NotFound();

            var zahtjevi = await _context.Obavijesti
                .Where(o => o.PošiljalacId == korisnik.Id && o.Naslov.StartsWith("Zahtjev profesora"))
                .OrderByDescending(o => o.DatumSlanja)
                .ToListAsync();

            ViewBag.Predmeti = profesor.Predmeti.ToList();
            ViewBag.Zahtjevi = zahtjevi;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiZahtjevZaDokument(string tipDokumenta, string napomena)
        {
            var korisnik = await _userManager.GetUserAsync(User);

            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska služba nije pronađena u sistemu.";
                return RedirectToAction(nameof(ZahtjevZaDokument));
            }

            var obavijest = new Obavijest
            {
                Naslov = $"Zahtjev profesora: {tipDokumenta}",
                Poruka = $"Profesor {korisnik.UserName} zahtjeva dokument tipa '{tipDokumenta}'." +
                         (string.IsNullOrWhiteSpace(napomena) ? "" : $" Napomena: {napomena}"),
                PošiljalacId = korisnik.Id,
                PrimalacId = studentskaSluzba.Id,
                DatumSlanja = DateTime.Now
            };

            _context.Obavijesti.Add(obavijest);
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = "Zahtjev je uspješno poslan studentskoj službi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SlanjeNotifikacija()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            // Učitaj studente za svaki predmet kroz UpisaNaPredmet
            foreach (var predmet in profesor.Predmeti)
            {
                predmet.Studenti = await _context.UpisaNaPredmet
                    .Include(u => u.Student)
                    .Where(u => u.PredmetId == predmet.Id)
                    .Select(u => u.Student)
                    .ToListAsync();
            }

            ViewBag.Predmeti = profesor.Predmeti.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiNotifikaciju(string naslov, string poruka, int? predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            // Učitaj studente kroz UpisaNaPredmet
            List<Student> primatelji;

            if (predmetId.HasValue)
            {
                primatelji = await _context.UpisaNaPredmet
                    .Include(u => u.Student)
                    .Where(u => u.PredmetId == predmetId.Value)
                    .Select(u => u.Student)
                    .ToListAsync();
            }
            else
            {
                var predmetIds = profesor.Predmeti.Select(p => p.Id).ToList();
                primatelji = await _context.UpisaNaPredmet
                    .Include(u => u.Student)
                    .Where(u => predmetIds.Contains(u.PredmetId))
                    .Select(u => u.Student)
                    .Distinct()
                    .ToListAsync();
            }

            foreach (var student in primatelji)
            {
                _context.Obavijesti.Add(new Obavijest
                {
                    Naslov = naslov,
                    Poruka = poruka,
                    PošiljalacId = korisnik.Id,
                    PrimalacId = student.Id,
                    DatumSlanja = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Notifikacija je uspješno poslana svim studentima.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiNotifikacijuNova(
            string naslov, string poruka, string tipNotifikacije,
            string primateljTip, string odabraniPredmeti, string odabraneGodine)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);
            if (profesor == null) return NotFound();

            var naslovSaTipom = "[" + tipNotifikacije + "] " + naslov;
            var predmetIds = profesor.Predmeti.Select(p => p.Id).ToList();
            var primatelji = new List<int>();

            if (primateljTip == "predmet")
            {
                var ids = (odabraniPredmeti ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(x => int.Parse(x)).ToList();
                primatelji = await _context.UpisaNaPredmet
                    .Where(u => ids.Contains(u.PredmetId))
                    .Select(u => u.StudentId)
                    .Distinct()
                    .ToListAsync();
            }
            else if (primateljTip == "svi")
            {
                primatelji = await _context.UpisaNaPredmet
                    .Where(u => predmetIds.Contains(u.PredmetId))
                    .Select(u => u.StudentId)
                    .Distinct()
                    .ToListAsync();
            }
            else if (primateljTip == "godina")
            {
                var godine = (odabraneGodine ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => int.Parse(x)).ToList();
                primatelji = await _context.UpisaNaPredmet
                    .Include(u => u.Student)
                    .Where(u => predmetIds.Contains(u.PredmetId) && godine.Contains(u.Student.GodinaStudija))
                    .Select(u => u.StudentId)
                    .Distinct()
                    .ToListAsync();
            }
            else if (primateljTip == "sluzba")
            {
                var sluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
                if (sluzba != null) primatelji.Add(sluzba.Id);
            }

            foreach (var primalacId in primatelji)
            {
                _context.Obavijesti.Add(new Obavijest
                {
                    Naslov = naslovSaTipom,
                    Poruka = poruka,
                    PošiljalacId = korisnik.Id,
                    PrimalacId = primalacId,
                    DatumSlanja = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Uspjeh"] = "Notifikacija poslana! Broj primatelja: " + primatelji.Count;
            return RedirectToAction(nameof(SlanjeNotifikacija));
        }
    }
}