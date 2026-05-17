using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

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
            return View(await _context.Administratori.ToListAsync());
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
    }
}