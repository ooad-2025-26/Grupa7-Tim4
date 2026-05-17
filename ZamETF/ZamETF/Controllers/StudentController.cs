using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    public class StudentController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            return View(await _context.Studenti.ToListAsync());
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
    }
}