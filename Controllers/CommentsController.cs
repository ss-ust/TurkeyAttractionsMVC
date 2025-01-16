using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TurkeyAttractionsMVC.Data;
using TurkeyAttractionsMVC.Models;

namespace TurkeyAttractionsMVC.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int attractionId, string text)
        {
            var userId = _context.Users.First(u => u.UserName == User.Identity.Name).Id;

            var comment = new Comment
            {
                AttractionID = attractionId,
                UserId = userId,
                Text = text
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Attractions", new { id = attractionId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = _context.Users.First(u => u.UserName == User.Identity.Name).Id;
            if (comment.UserId != userId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            return View(comment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Comment model)
        {
            var comment = await _context.Comments.FindAsync(model.ID);
            if (comment == null) return NotFound();

            var userId = _context.Users.First(u => u.UserName == User.Identity.Name).Id;
            if (comment.UserId != userId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            comment.Text = model.Text;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Attractions", new { id = comment.AttractionID });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = _context.Users.First(u => u.UserName == User.Identity.Name).Id;
            // admin can delete any comment, user can only delete their own
            if (comment.UserId != userId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            return View(comment);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(); // Return 404 if the comment doesn't exist
            }

            // Check if the user is authorized to delete the comment
            var userId = _context.Users.First(u => u.UserName == User.Identity.Name).Id;
            if (comment.UserId != userId && !User.IsInRole("admin"))
            {
                return Forbid(); // Return 403 Forbidden if not authorized
            }

            try
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                // Redirect back to the attraction details page
                return RedirectToAction("Details", "Attractions", new { id = comment.AttractionID });
            }
            catch (Exception)
            {
                // Handle errors (e.g., database issues)
                return StatusCode(500, "An error occurred while deleting the comment.");
            }
        }
    }
}
