using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "TrainingCoordinator")]
public class PaymentsController : Controller
{
    private readonly CertifyDbContext _db;
    public PaymentsController(CertifyDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Show outstanding balances per trainee
        var trainees = await _db.Users.ToListAsync();
        var rows = new List<dynamic>();
        foreach (var t in trainees)
        {
            var owed = await _db.Enrollments
                .Where(e => e.TraineeId == t.Id && e.Status != "Dropped")
                .SumAsync(e => (decimal?)e.EnrollmentFee) ?? 0m;
            var paid = await _db.Payments
                .Where(p => p.TraineeId == t.Id && p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
            if (owed == 0) continue;
            rows.Add(new
            {
                User = t,
                Owed = owed,
                Paid = paid,
                Outstanding = owed - paid,
                Overdue = (owed - paid) > 0 && _db.Enrollments.Any(e => e.TraineeId == t.Id && e.PaymentStatus == "Pending")
            });
        }
        return View(rows);
    }

    public async Task<IActionResult> Record(string traineeId)
    {
        var trainee = await _db.Users.FindAsync(traineeId);
        if (trainee == null) return NotFound();
        ViewBag.Trainee = trainee;
        ViewBag.Outstanding = await GetOutstandingAsync(traineeId);
        return View(new Payment { TraineeId = traineeId, PaymentDate = DateTime.UtcNow, Status = "Completed" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(Payment payment)
    {
        ModelState.Remove(nameof(Payment.Trainee));

        // Cap payment at outstanding balance
        var outstanding = await GetOutstandingAsync(payment.TraineeId);
        if (payment.Status == "Completed" && payment.Amount > outstanding)
        {
            ModelState.AddModelError(nameof(payment.Amount),
                $"Amount exceeds outstanding balance of BHD {outstanding:F2}.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Trainee = await _db.Users.FindAsync(payment.TraineeId);
            ViewBag.Outstanding = outstanding;
            return View(payment);
        }
        _db.Payments.Add(payment);

        // Mark fully-paid enrollments as Paid
        var totalPaid = await _db.Payments.Where(p => p.TraineeId == payment.TraineeId && p.Status == "Completed").SumAsync(p => p.Amount) + payment.Amount;
        var enrollments = await _db.Enrollments.Where(e => e.TraineeId == payment.TraineeId && e.PaymentStatus == "Pending").OrderBy(e => e.Id).ToListAsync();
        decimal running = 0;
        foreach (var e in enrollments)
        {
            running += e.EnrollmentFee;
            if (running <= totalPaid) e.PaymentStatus = "Paid";
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Payment recorded.";
        return RedirectToAction(nameof(Index));
    }

    // Helper: outstanding balance for a trainee
    private async Task<decimal> GetOutstandingAsync(string traineeId)
    {
        var owed = await _db.Enrollments
            .Where(e => e.TraineeId == traineeId && e.Status != "Dropped")
            .SumAsync(e => (decimal?)e.EnrollmentFee) ?? 0m;
        var paid = await _db.Payments
            .Where(p => p.TraineeId == traineeId && p.Status == "Completed")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        return owed - paid;
    }
}