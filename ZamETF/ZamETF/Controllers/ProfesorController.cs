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

        // GET: Profesor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Profesori.ToListAsync());
        }

        // GET: Profesor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var profesor = await _context.Profesori.FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // GET: Profesor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Profesor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titula,Ime,Prezime,UserName,Email,Uloga")] Profesor profesor, string Lozinka)
        {
            if (ModelState.IsValid)
            {
                profesor.Uloga = Uloga.Profesor;
                var result = await _userManager.CreateAsync(profesor, Lozinka);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(profesor);
        }

        // GET: Profesor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // POST: Profesor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titula,Ime,Prezime,UserName,Email,Uloga")] Profesor profesor)
        {
            if (id != profesor.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var postojeci = await _context.Profesori.FindAsync(id);
                if (postojeci == null) return NotFound();
                postojeci.Ime = profesor.Ime;
                postojeci.Prezime = profesor.Prezime;
                postojeci.UserName = profesor.UserName;
                postojeci.Email = profesor.Email;
                postojeci.Titula = profesor.Titula;
                await _userManager.UpdateAsync(postojeci);
                return RedirectToAction(nameof(Index));
            }
            return View(profesor);
        }

        // GET: Profesor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var profesor = await _context.Profesori.FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // POST: Profesor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor != null)
                await _userManager.DeleteAsync(profesor);
            return RedirectToAction(nameof(Index));
        }
    }
}