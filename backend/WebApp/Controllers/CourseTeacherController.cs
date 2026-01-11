using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseTeacherController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: CourseTeacher
        public async Task<IActionResult> Index()
        {
           var appDbContext = context.CourseTeachers.Include(c => c.Course).Include(c => c.Teacher);
            return View(await appDbContext.IgnoreQueryFilters().ToListAsync());
        }

        // GET: CourseTeacher/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseTeacherEntity = await context.CourseTeachers
                .IgnoreQueryFilters()
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseTeacherEntity == null)
            {
                return NotFound();
            }

            return View(courseTeacherEntity);
        }

        // GET: CourseTeacher/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode");
            ViewData["TeacherId"] = new SelectList(context.Users, "Id", "Email");
            return View();
        }

        // POST: CourseTeacher/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseId,TeacherId,CreatedBy,UpdatedBy,Deleted")] CourseTeacherEntity courseTeacherEntity)
        {
            if (ModelState.IsValid)
            {
                courseTeacherEntity.UpdatedAt = DateTime.UtcNow;
                courseTeacherEntity.CreatedAt = DateTime.UtcNow;
                context.Add(courseTeacherEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(context.Users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // GET: CourseTeacher/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseTeacherEntity = await context.CourseTeachers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseTeacherEntity == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(context.Users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // POST: CourseTeacher/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseId,TeacherId,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] CourseTeacherEntity courseTeacherEntity)
        {
            if (id != courseTeacherEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    courseTeacherEntity.CreatedAt = DateTime.SpecifyKind(courseTeacherEntity.CreatedAt, DateTimeKind.Utc);
                    courseTeacherEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{courseTeacherEntity.Id.ToString()}*");
                    await redis.DeleteKeysByPatternAsync($"*{courseTeacherEntity.CourseId.ToString()}*");
                    await redis.DeleteKeysByPatternAsync($"*{courseTeacherEntity.TeacherId.ToString()}*");
                    context.Update(courseTeacherEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseTeacherEntityExists(courseTeacherEntity.Id))
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
            ViewData["CourseId"] = new SelectList(context.Courses, "Id", "CourseCode", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(context.Users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // GET: CourseTeacher/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseTeacherEntity = await context.CourseTeachers
                .IgnoreQueryFilters()
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseTeacherEntity == null)
            {
                return NotFound();
            }

            return View(courseTeacherEntity);
        }

        // POST: CourseTeacher/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseTeacherEntity = await context.CourseTeachers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseTeacherEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{courseTeacherEntity.Id.ToString()}*");
                await redis.DeleteKeysByPatternAsync($"*{courseTeacherEntity.CourseId.ToString()}*");
                await redis.DeleteKeysByPatternAsync($"*{courseTeacherEntity.TeacherId.ToString()}*");
                context.CourseTeachers.Remove(courseTeacherEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseTeacherEntityExists(Guid id)
        {
            return context.CourseTeachers.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
