using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZamETF.Data;
using ZamETF.Models;

namespace ZamETF.Controllers
{
    public class StudentskaSluzbaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentskaSluzbaController(ApplicationDbContext context)
        {
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
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StudentskaSluzba/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ime,Prezime,Username,Email,Lozinka,Uloga")] StudentskaSluzba studentskaSluzba)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studentskaSluzba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }
            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Prezime,Username,Email,Lozinka,Uloga")] StudentskaSluzba studentskaSluzba)
        {
            if (id != studentskaSluzba.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studentskaSluzba);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentskaSluzbaExists(studentskaSluzba.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba != null)
            {
                _context.StudentskeSluzbe.Remove(studentskaSluzba);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentskaSluzbaExists(int id)
        {
            return _context.StudentskeSluzbe.Any(e => e.Id == id);
        }
    }
}
