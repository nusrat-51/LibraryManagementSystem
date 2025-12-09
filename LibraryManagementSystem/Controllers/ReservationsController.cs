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

        // =======================
        // LIST RESERVATIONS
        // =======================
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            // just so we can see something happened
            ViewBag.Debug = TempData["ReservationDebug"];

            return View(reservations);
        }

        // =======================
        // CREATE (GET)
        // =======================
        public IActionResult Create()
        {
            ViewBag.BookId = new SelectList(
                _context.Books.Where(b => b.AvailableCopies > 0),
                "Id",
                "Title"
            );

            return View(new Reservation());
        }

        // =======================
        // CREATE (POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                // send model state errors to Index so we can see them
                TempData["ReservationDebug"] = string.Join(" | ",
                    ModelState.Where(kvp => kvp.Value.Errors.Any())
                              .Select(kvp => $"{kvp.Key}: {string.Join(",", kvp.Value.Errors.Select(e => e.ErrorMessage))}")
                );

                ViewBag.BookId = new SelectList(
                    _context.Books.Where(b => b.AvailableCopies > 0),
                    "Id",
                    "Title",
                    reservation.BookId
                );

                return View(reservation);
            }

            // good data – save
            reservation.RequestedAt = DateTime.UtcNow;
            reservation.Status = ReservationStatus.Pending;

            reservation.QueuePosition = await _context.Reservations
                .CountAsync(r => r.BookId == reservation.BookId &&
                                 (r.Status == ReservationStatus.Pending ||
                                  r.Status == ReservationStatus.Active)) + 1;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["ReservationDebug"] = "Saved OK";
            return RedirectToAction(nameof(Index));
        }

        // other actions (Details/Edit/Delete) can stay as they were before
        // ...
    }
}
