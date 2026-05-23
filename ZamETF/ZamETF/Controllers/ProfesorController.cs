using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    public class ProfesorController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfesorController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Profesor/Index
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Zadace)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            // Obavijesti koje su upućene profesoru
            var obavijesti = await _context.Obavijesti
                .Where(o => o.PrimalacId == profesor.Id)
                .OrderByDescending(o => o.DatumSlanja)
                .Take(10)
                .ToListAsync();

            // Aktuelne zadaće po predmetima profesora
            var predmetIds = profesor.Predmeti.Select(p => p.Id).ToList();
            var zadace = await _context.Zadace
                .Include(z => z.Predmet)
                .Where(z => predmetIds.Contains(z.Predmet.Id))
                .OrderByDescending(z => z.Rok)
                .Take(10)
                .ToListAsync();

            ViewBag.Profesor = profesor;
            ViewBag.Obavijesti = obavijesti;
            ViewBag.Zadace = zadace;
            ViewBag.Predmeti = profesor.Predmeti.ToList();

            return View();
        }

        // GET: Profesor/DetaljiPredmeta/5
        public async Task<IActionResult> DetaljiPredmeta(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Studenti)
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Zadace)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var predmet = profesor?.Predmeti.FirstOrDefault(p => p.Id == id);
            if (predmet == null) return NotFound();

            return View(predmet);
        }

        // GET: Profesor/KreiranjeZadace
        public async Task<IActionResult> KreiranjeZadace()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            ViewBag.Predmeti = profesor.Predmeti.ToList();
            return View();
        }

        // POST: Profesor/KreiranjeZadace
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreiranjeZadace(string nazividID, string opis, DateTime rok, int predmetId)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Zadace)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (predmet == null)
            {
                ModelState.AddModelError("", "Predmet nije pronađen.");
            }

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

        // GET: Profesor/OcjenjivanjeZadace/5
        public async Task<IActionResult> OcjenjivanjeZadace(int id)
        {
            var zadaca = await _context.Zadace
                .Include(z => z.Predmet)
                .Include(z => z.Predaje)
                    .ThenInclude(pz => pz.Student)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zadaca == null) return NotFound();

            return View(zadaca);
        }

        // POST: Profesor/SacuvajBodove
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

        // GET: Profesor/EvidencijaPrisustva/5
        public async Task<IActionResult> EvidencijaPrisustva(int predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Studenti)
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Prisustva)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            var predmet = profesor?.Predmeti.FirstOrDefault(p => p.Id == predmetId);
            if (predmet == null) return NotFound();

            ViewBag.DatumDanas = DateTime.Today;
            return View(predmet);
        }

        // POST: Profesor/SacuvajPrisustvo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajPrisustvo(int predmetId, DateTime datum, List<int> prisutniIds)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Studenti)
                .Include(p => p.Prisustva)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (predmet == null) return NotFound();

            foreach (var student in predmet.Studenti)
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

        // GET: Profesor/ZahtjevZaDokument
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

        // POST: Profesor/PosaljiZahtjevZaDokument
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiZahtjevZaDokument(string tipDokumenta, string napomena)
        {
            var korisnik = await _userManager.GetUserAsync(User);

            // Nađi studentsku službu kao primatelja
            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync();
            if (studentskaSluzba == null)
            {
                TempData["Greska"] = "Studentska služba nije pronađena u sistemu.";
                return RedirectToAction(nameof(ZahtjevZaDokument));
            }

            // Pošalji obavijest studentskoj službi
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

        // GET: Profesor/SlanjeNotifikacija
        public async Task<IActionResult> SlanjeNotifikacija()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Studenti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            ViewBag.Predmeti = profesor.Predmeti.ToList();
            return View();
        }

        // POST: Profesor/PosaljiNotifikaciju
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PosaljiNotifikaciju(string naslov, string poruka, int? predmetId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var profesor = await _context.Profesori
                .Include(p => p.Predmeti)
                    .ThenInclude(pr => pr.Studenti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

            if (profesor == null) return NotFound();

            IEnumerable<Student> primatelji;

            if (predmetId.HasValue)
            {
                var predmet = profesor.Predmeti.FirstOrDefault(p => p.Id == predmetId.Value);
                primatelji = predmet?.Studenti ?? Enumerable.Empty<Student>();
            }
            else
            {
                primatelji = profesor.Predmeti
                    .SelectMany(p => p.Studenti)
                    .DistinctBy(s => s.Id);
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
                    .ThenInclude(pr => pr.Studenti)
                .FirstOrDefaultAsync(p => p.Id == korisnik.Id);
            if (profesor == null) return NotFound();

            var naslovSaTipom = "[" + tipNotifikacije + "] " + naslov;
            var primatelji = new List<int>();

            if (primateljTip == "predmet")
            {
                var ids = (odabraniPredmeti ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(x => int.Parse(x)).ToList();
                primatelji = profesor.Predmeti
                    .Where(p => ids.Contains(p.Id))
                    .SelectMany(p => p.Studenti)
                    .Select(s => s.Id)
                    .Distinct().ToList();
            }
            else if (primateljTip == "svi")
            {
                primatelji = profesor.Predmeti
                    .SelectMany(p => p.Studenti)
                    .Select(s => s.Id)
                    .Distinct().ToList();
            }
            else if (primateljTip == "godina")
            {
                var godine = (odabraneGodine ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => int.Parse(x)).ToList();
                primatelji = profesor.Predmeti
                    .SelectMany(p => p.Studenti)
                    .Where(s => godine.Contains(s.GodinaStudija))
                    .Select(s => s.Id)
                    .Distinct().ToList();
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
