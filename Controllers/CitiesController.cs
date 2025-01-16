using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurkeyAttractionsMVC.Data;
using TurkeyAttractionsMVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace TurkeyAttractionsMVC.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cities = await _context.Cities.ToListAsync();
            return View(cities);
        }

        public async Task<IActionResult> Details(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Attractions)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(City city)
        {
            if (ModelState.IsValid)
            {
                _context.Cities.Add(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch the city from the database
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.ID == id);
            if (city == null)
            {
                return NotFound(); // Return 404 if the city is not found
            }

            return View(city); // Pass the city to the edit view
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, City city)
        {
            if (id != city.ID)
            {
                return BadRequest(); // Ensure the ID in the route matches the city ID
            }

            if (!ModelState.IsValid)
            {
                return View(city); // Return the same view with validation messages
            }

            try
            {
                _context.Cities.Update(city); // Mark the city as modified
                await _context.SaveChangesAsync(); // Save changes to the database
                return RedirectToAction(nameof(Index)); // Redirect to the index page
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(city.ID))
                {
                    return NotFound(); // Handle cases where the city was deleted concurrently
                }
                else
                {
                    throw; // Re-throw other concurrency exceptions
                }
            }
        }

        // Helper method to check if a city exists by its ID
        private bool CityExists(int id)
        {
            return _context.Cities.Any(c => c.ID == id);
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null) return NotFound();
            return View(city);
        }

        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var cities = await _context.Cities.ToListAsync();
            return View(cities);
        }

    }

}
