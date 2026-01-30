using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseAttendanceController(AppDbContext context, RedisRepository redis) : Controller
    {

        // GET: CourseAttendance
        public async Task<IActionResult> Index()
        {
            var appDbContext = context.Attendances.Include(c => c.AttendanceType).Include(c => c.Course);
            return View(await appDbContext.IgnoreQueryFilters().ToListAsync());
        }

        // GET: CourseAttendance/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAttendanceEntity = await context.Attendances
                .IgnoreQueryFilters()
                .Include(c => c.AttendanceType)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseAttendanceEntity == null)
            {
                return NotFound();
            }

            return View(courseAttendanceEntity);
        }

        // GET: CourseAttendance/Create
        public IActionResult Create()
        {
            ViewData["AttendanceTypeId"] = new SelectList(context.AttendanceTypes, "Id", "AttendanceType");
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode");
            return View();
        }

        // POST: CourseAttendance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseId,Identifier,AttendanceTypeId,StartTime,EndTime,CreatedBy,UpdatedBy,Delete")] AttendanceEntity attendanceEntity)
        {
            if (ModelState.IsValid)
            {
                attendanceEntity.UpdatedAt = DateTime.UtcNow;
                attendanceEntity.CreatedAt = DateTime.UtcNow;
                context.Add(attendanceEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AttendanceTypeId"] = new SelectList(context.AttendanceTypes, "Id", "AttendanceType", attendanceEntity.AttendanceTypeId);
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode", attendanceEntity.CourseId);
            return View(attendanceEntity);
        }

        // GET: CourseAttendance/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAttendanceEntity = await context.Attendances
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseAttendanceEntity == null)
            {
                return NotFound();
            }
            ViewData["AttendanceTypeId"] = new SelectList(context.AttendanceTypes, "Id", "AttendanceType", courseAttendanceEntity.AttendanceTypeId);
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode", courseAttendanceEntity.CourseId);
            return View(courseAttendanceEntity);
        }

        // POST: CourseAttendance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseId,Identifier,AttendanceTypeId,StartTime,EndTime,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] AttendanceEntity attendanceEntity)
        {
            if (id != attendanceEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    attendanceEntity.CreatedAt = DateTime.SpecifyKind(attendanceEntity.CreatedAt, DateTimeKind.Utc);
                    attendanceEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{attendanceEntity.Id.ToString()}*");
                    await redis.DeleteKeysByPatternAsync($"*{attendanceEntity.Identifier}*");                     
                    context.Update(attendanceEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseAttendanceEntityExists(attendanceEntity.Id))
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
            ViewData["AttendanceTypeId"] = new SelectList(context.AttendanceTypes, "Id", "AttendanceType", attendanceEntity.AttendanceTypeId);
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode", attendanceEntity.CourseId);
            return View(attendanceEntity);
        }

        // GET: CourseAttendance/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAttendanceEntity = await context.Attendances
                .IgnoreQueryFilters()
                .Include(c => c.AttendanceType)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseAttendanceEntity == null)
            {
                return NotFound();
            }

            return View(courseAttendanceEntity);
        }

        // POST: CourseAttendance/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseAttendanceEntity = await context.Attendances
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseAttendanceEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{courseAttendanceEntity.Id.ToString()}*");
                await redis.DeleteKeysByPatternAsync($"*{courseAttendanceEntity.Identifier}*");
                context.Attendances.Remove(courseAttendanceEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseAttendanceEntityExists(Guid id)
        {
            return context.Attendances.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
