using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian,Admin")]
    public class ReservationsController : Controller
    {
        private readonly LibraryContext _context;

        public ReservationsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            // dropdown list of books
            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title");
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", reservation.BookId);
                return View(reservation);
            }

            // basic safety defaults
            reservation.RequestedAt = DateTime.UtcNow;

            reservation.QueuePosition = await _context.Reservations
                .CountAsync(r => r.BookId == reservation.BookId &&
                                 (r.Status == ReservationStatus.Pending ||
                                  r.Status == ReservationStatus.Active)) + 1;

            _context.Add(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", reservation.BookId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", reservation.BookId);
                return View(reservation);
            }

            try
            {
                _context.Update(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservations.Any(e => e.Id == reservation.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
