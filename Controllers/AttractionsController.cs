using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TurkeyAttractionsMVC.Data;
using TurkeyAttractionsMVC.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TurkeyAttractionsMVC.Controllers
{
    public class AttractionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AttractionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? cityId)
        {
            var cities = await _context.Cities.ToListAsync();
            var options = "<option value=\"\">-- Select a City --</option>";

            foreach (var city in cities)
            {
                options += $"<option value=\"{city.ID}\" {(cityId == city.ID ? "selected" : "")}>{city.Name}</option>";
            }

            ViewBag.CityOptions = options;
            ViewBag.SelectedCityId = cityId;

            var attractions = cityId == null
                ? Enumerable.Empty<Attraction>()
                : await _context.Attractions.Where(a => a.CityID == cityId).ToListAsync();

            return View(attractions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var attraction = await _context.Attractions
                .Include(a => a.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (attraction == null)
                return NotFound();

            bool visited = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    visited = await _context.UserAttractions
                        .AnyAsync(ua => ua.AttractionId == id && ua.UserId == user.Id && ua.Visited);
                }
            }

            ViewBag.Visited = visited;
            return View(attraction);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(int cityId = 0)
        {
            // Populate cities for the dropdown
            var cities = await _context.Cities.ToListAsync();
            ViewBag.Cities = new SelectList(cities, "ID", "Name", cityId);
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(Attraction attraction)
        {
            try
            {
                // Validate CityID exists in the database
                var cityExists = await _context.Cities.AnyAsync(c => c.ID == attraction.CityID);
                if (!cityExists)
                {
                    ModelState.AddModelError("CityID", "The selected city does not exist.");
                    var cities = await _context.Cities.ToListAsync();
                    ViewBag.Cities = new SelectList(cities, "ID", "Name", attraction.CityID);
                    return View(attraction);
                }
                System.Diagnostics.Debug.WriteLine("\n\n\nsave\n\n\n");
                // Save the new attraction to the database
                _context.Attractions.Add(attraction);
                await _context.SaveChangesAsync();

                // Redirect to the Index view with the cityId parameter
                return RedirectToAction("Index", new { cityId = attraction.CityID });
            }
            catch (Exception ex)
            {
                // Log the error if necessary
                System.Diagnostics.Debug.WriteLine($"Error saving attraction: {ex.Message}");

                // Add a generic error message to ModelState
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                var cities = await _context.Cities.ToListAsync();
                ViewBag.Cities = new SelectList(cities, "ID", "Name", attraction.CityID);
                return View(attraction);
            }
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var attraction = await _context.Attractions.FindAsync(id);
            if (attraction == null)
                return NotFound();

            var cities = await _context.Cities.ToListAsync();
            ViewBag.Cities = new SelectList(cities, "ID", "Name", attraction.CityID);

            return View(attraction);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Attraction attraction)
        {
            // Model validation failed, re-populate cities
            var cities = await _context.Cities.ToListAsync();
            ViewBag.Cities = new SelectList(cities, "ID", "Name", attraction.CityID);

            return View(attraction);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var attraction = await _context.Attractions
                .Include(a => a.City)
                .FirstOrDefaultAsync(a => a.ID == id); // Include related city for confirmation view
            if (attraction == null)
                return NotFound();

            return View(attraction); // Pass the attraction to the Delete confirmation view
        }

        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attraction = await _context.Attractions.FindAsync(id);
  

            _context.Attractions.Remove(attraction);
            await _context.SaveChangesAsync();

            // Redirect to the city details or index page after successful deletion
            return RedirectToAction("Index", new { cityId = attraction.CityID });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleVisited(int attractionId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Details", new { id = attractionId });

            var userAttraction = await _context.UserAttractions
                .FirstOrDefaultAsync(ua => ua.AttractionId == attractionId && ua.UserId == user.Id);

            if (userAttraction == null)
            {
                userAttraction = new UserAttraction
                {
                    AttractionId = attractionId,
                    UserId = user.Id,
                    Visited = true
                };
                _context.UserAttractions.Add(userAttraction);
            }
            else
            {
                userAttraction.Visited = !userAttraction.Visited;
                _context.UserAttractions.Update(userAttraction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = attractionId });
        }
        [Authorize]
        public async Task<IActionResult> Visited()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var visitedAttractions = await _context.UserAttractions
                .Where(ua => ua.UserId == user.Id && ua.Visited)
                .Include(ua => ua.Attraction) // Include Attraction entity
                .ThenInclude(a => a.City)    // Include related City entity
                .Select(ua => ua.Attraction) // Project only the Attraction entity
                .ToListAsync();

            return View(visitedAttractions);
        }
    }
}
