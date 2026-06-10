using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    [Authorize]
    public class IspitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public IspitController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===================== PROFESOR: POSTAVLJANJE ISPITA =====================

        // Pregled svih ispita za predmete prijavljenog profesora
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not Profesor profesor)
                return Forbid();

            var ispiti = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.Prijave)
                .Where(i => i.Predmet.ProfesorId == profesor.Id)
                .OrderBy(i => i.Datum)
                .ToListAsync();

            return View(ispiti);
        }

        // GET: forma za novi ispit
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not Profesor profesor)
                return Forbid();

            await PopuniPredmete(profesor.Id);

            return View(new IspitCreateVM
            {
                Datum = DateTime.Today.AddDays(7).AddHours(9),
                RokZaPrijavu = DateTime.Today.AddDays(5).AddHours(23)
            });
        }

        // POST: kreiranje ispita
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IspitCreateVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not Profesor profesor)
                return Forbid();

            var predmet = await _context.Predmeti
                .FirstOrDefaultAsync(p => p.Id == model.PredmetId && p.ProfesorId == profesor.Id);
            if (predmet == null)
                ModelState.AddModelError(nameof(model.PredmetId), "Odaberite jedan od svojih predmeta.");

            if (model.RokZaPrijavu > model.Datum)
                ModelState.AddModelError(nameof(model.RokZaPrijavu), "Rok za prijavu mora biti prije datuma ispita.");

            if (!ModelState.IsValid)
            {
                await PopuniPredmete(profesor.Id);
                return View(model);
            }

            _context.Ispiti.Add(new Ispit
            {
                PredmetId = model.PredmetId,
                Datum = model.Datum,
                RokZaPrijavu = model.RokZaPrijavu
            });
            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Ispit je uspješno postavljen.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== STUDENT: PRIJAVA ISPITA (kombinovana stranica) =====================

        // Jedna stranica: lijevo dostupni ispiti, desno studentove prijave
        public async Task<IActionResult> Prijava()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not Student student)
                return Forbid();

            var sada = DateTime.Now;

            var mojePrijave = await _context.PrijaveIspita
                .Include(p => p.Ispit).ThenInclude(i => i.Predmet)
                .Where(p => p.StudentId == student.Id)
                .OrderByDescending(p => p.DatumPrijave)
                .ToListAsync();

            var prijavljeniIds = mojePrijave
                .Where(p => p.Status == StatusPrijaveIspit.PrijavljenIspit)
                .Select(p => p.IspitId)
                .ToList();

            var dostupni = await _context.Ispiti
                .Include(i => i.Predmet)
                .Where(i => i.RokZaPrijavu >= sada && !prijavljeniIds.Contains(i.Id))
                .OrderBy(i => i.Datum)
                .ToListAsync();

            return View(new PrijavaIspitaVM
            {
                Dostupni = dostupni,
                MojePrijave = mojePrijave
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prijavi(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not Student student)
                return Forbid();

            var ispit = await _context.Ispiti.FirstOrDefaultAsync(i => i.Id == id);
            if (ispit == null)
                return NotFound();

            if (DateTime.Now > ispit.RokZaPrijavu)
            {
                TempData["Greska"] = "Rok za prijavu ovog ispita je istekao.";
                return RedirectToAction(nameof(Prijava));
            }

            bool vecPrijavljen = await _context.PrijaveIspita.AnyAsync(p =>
                p.IspitId == id && p.StudentId == student.Id &&
                p.Status == StatusPrijaveIspit.PrijavljenIspit);

            if (vecPrijavljen)
            {
                TempData["Greska"] = "Već ste prijavljeni na ovaj ispit.";
                return RedirectToAction(nameof(Prijava));
            }

            _context.PrijaveIspita.Add(new PrijavaIspit
            {
                IspitId = id,
                StudentId = student.Id,
                DatumPrijave = DateTime.Now,
                Status = StatusPrijaveIspit.PrijavljenIspit
            });
            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Uspješno ste prijavili ispit.";
            return RedirectToAction(nameof(Prijava));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Odjavi(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not Student student)
                return Forbid();

            var prijava = await _context.PrijaveIspita
                .Include(p => p.Ispit)
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == student.Id);

            if (prijava == null)
                return NotFound();

            if (DateTime.Now > prijava.Ispit.RokZaPrijavu)
            {
                TempData["Greska"] = "Rok je istekao, ne možete se više odjaviti.";
                return RedirectToAction(nameof(Prijava));
            }

            prijava.Status = StatusPrijaveIspit.Odjavljen;
            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Odjavili ste se sa ispita.";
            return RedirectToAction(nameof(Prijava));
        }

        // ===================== POMOĆNO =====================

        private async Task PopuniPredmete(int profesorId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.ProfesorId == profesorId)
                .OrderBy(p => p.Naziv)
                .ToListAsync();

            ViewBag.Predmeti = new SelectList(predmeti, "Id", "Naziv");
        }
    }
}
