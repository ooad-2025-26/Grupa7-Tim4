using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    public class StudentskaSluzbaController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentskaSluzbaController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: StudentskaSluzba
        public async Task<IActionResult> Index()
        {
            return View(await _context.StudentskeSluzbe.ToListAsync());
        }

        // GET: StudentskaSluzba/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync(m => m.Id == id);
            if (studentskaSluzba == null) return NotFound();
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StudentskaSluzba/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,Username,Email,Uloga")] StudentskaSluzba studentskaSluzba, string Lozinka)
        {
            if (ModelState.IsValid)
            {
                studentskaSluzba.Uloga = Uloga.StudentskaSluzba;
                var result = await _userManager.CreateAsync(studentskaSluzba, Lozinka);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba == null) return NotFound();
            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Prezime,UserName,Email,Uloga")] StudentskaSluzba studentskaSluzba)
        {
            if (id != studentskaSluzba.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var postojeci = await _context.StudentskeSluzbe.FindAsync(id);
                if (postojeci == null) return NotFound();
                postojeci.Ime = studentskaSluzba.Ime;
                postojeci.Prezime = studentskaSluzba.Prezime;
                postojeci.UserName = studentskaSluzba.UserName;
                postojeci.Email = studentskaSluzba.Email;
                await _userManager.UpdateAsync(postojeci);
                return RedirectToAction(nameof(Index));
            }
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync(m => m.Id == id);
            if (studentskaSluzba == null) return NotFound();
            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba != null)
                await _userManager.DeleteAsync(studentskaSluzba);
            return RedirectToAction(nameof(Index));
        }
    }
}