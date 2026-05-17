using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<Korisnik> userManager, SignInManager<Korisnik> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Account
        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.ToListAsync());
        }

        // GET: Account/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var korisnik = await _userManager.FindByIdAsync(id.ToString());
            if (korisnik == null) return NotFound();
            return View(korisnik);
        }

        // GET: Account/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,Username,Email,Uloga")] Korisnik korisnik, string Lozinka)
        {
            if (ModelState.IsValid)
            {
                korisnik.Uloga = Uloga.Student;
                var result = await _userManager.CreateAsync(korisnik, Lozinka);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(korisnik);
        }

        // GET: Account/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var korisnik = await _userManager.FindByIdAsync(id.ToString());
            if (korisnik == null) return NotFound();
            return View(korisnik);
        }

        // POST: Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Prezime,UserName,Email,Uloga")] Korisnik korisnik)
        {
            if (id != korisnik.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var postojeci = await _userManager.FindByIdAsync(id.ToString());
                if (postojeci == null) return NotFound();
                postojeci.Ime = korisnik.Ime;
                postojeci.Prezime = korisnik.Prezime;
                postojeci.UserName = korisnik.UserName;
                postojeci.Email = korisnik.Email;
                postojeci.Uloga = korisnik.Uloga;
                var result = await _userManager.UpdateAsync(postojeci);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(korisnik);
        }

        // GET: Account/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var korisnik = await _userManager.FindByIdAsync(id.ToString());
            if (korisnik == null) return NotFound();
            return View(korisnik);
        }

        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var korisnik = await _userManager.FindByIdAsync(id.ToString());
            if (korisnik != null)
                await _userManager.DeleteAsync(korisnik);
            return RedirectToAction(nameof(Index));
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string lozinka)
        {
            var korisnik = await _userManager.FindByEmailAsync(email);
            if (korisnik == null)
            {
                ModelState.AddModelError("", "Pogrešan email ili lozinka.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(korisnik, lozinka, false, false);
            if (result.Succeeded)
            {
                return korisnik.Uloga switch
                {
                    Uloga.Student => RedirectToAction("Index", "Student"),
                    Uloga.Profesor => RedirectToAction("Index", "Profesor"),
                    Uloga.Administrator => RedirectToAction("Index", "Administrator"),
                    Uloga.StudentskaSluzba => RedirectToAction("Index", "StudentskaSluzba"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ModelState.AddModelError("", "Pogrešan email ili lozinka.");
            return View();
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}