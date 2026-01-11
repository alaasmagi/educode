using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class AttendanceTypeController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: AttendanceType
        public async Task<IActionResult> Index()
        {
            return View(await context.AttendanceTypes.IgnoreQueryFilters().ToListAsync());
        }

        // GET: AttendanceType/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceTypeEntity = await context.AttendanceTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendanceTypeEntity == null)
            {
                return NotFound();
            }

            return View(attendanceTypeEntity);
        }

        // GET: AttendanceType/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: AttendanceType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("AttendanceType,CreatedBy,UpdatedBy,Deleted")] AttendanceTypeEntity attendanceTypeEntity)
        {
            if (ModelState.IsValid)
            {
                attendanceTypeEntity.UpdatedAt = DateTime.UtcNow;
                attendanceTypeEntity.CreatedAt = DateTime.UtcNow;
                context.Add(attendanceTypeEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendanceTypeEntity);
        }

        // GET: AttendanceType/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceTypeEntity = await context.AttendanceTypes            
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (attendanceTypeEntity == null)
            {
                return NotFound();
            }
            return View(attendanceTypeEntity);
        }

        // POST: AttendanceType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("AttendanceType,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] AttendanceTypeEntity attendanceTypeEntity)
        {
            if (id != attendanceTypeEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    attendanceTypeEntity.CreatedAt = DateTime.SpecifyKind(attendanceTypeEntity.CreatedAt, DateTimeKind.Utc);
                    attendanceTypeEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{attendanceTypeEntity.Id.ToString()}*");
                    context.Update(attendanceTypeEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceTypeEntityExists(attendanceTypeEntity.Id))
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
            return View(attendanceTypeEntity);
        }

        // GET: AttendanceType/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceTypeEntity = await context.AttendanceTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendanceTypeEntity == null)
            {
                return NotFound();
            }

            return View(attendanceTypeEntity);
        }

        // POST: AttendanceType/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var attendanceTypeEntity = await context.AttendanceTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (attendanceTypeEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{attendanceTypeEntity.Id.ToString()}*");
                context.AttendanceTypes.Remove(attendanceTypeEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceTypeEntityExists(Guid id)
        {
            return context.AttendanceTypes.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
