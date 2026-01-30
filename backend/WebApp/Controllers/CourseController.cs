using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: Course
        public async Task<IActionResult> Index()
        {
            return View(await context.Courses.IgnoreQueryFilters().ToListAsync());
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseEntity = await context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseEntity == null)
            {
                return NotFound();
            }

            return View(courseEntity);
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            ViewData["CourseStatus"] = new SelectList(context.CourseStatuses, "Id", "CourseStatus");
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View();
        }

        // POST: Course/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseCode,CourseName,SchoolId,CrossUniRegistration,CourseStatusId,CreatedBy,UpdatedBy,Deleted")] CourseEntity courseEntity)
        {
            if (ModelState.IsValid)
            {
                courseEntity.UpdatedAt = DateTime.UtcNow;
                courseEntity.CreatedAt = DateTime.UtcNow;
                context.Add(courseEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseStatus"] = new SelectList(context.CourseStatuses, "Id", "CourseStatus", courseEntity.CourseStatusId);
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name", courseEntity.SchoolId);
            return View(courseEntity);
        }

        // GET: Course/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseEntity = await context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseEntity == null)
            {
                return NotFound();
            }
            
            ViewData["CourseStatus"] = new SelectList(context.CourseStatuses, "Id", "CourseStatus");
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View(courseEntity);
        }

        // POST: Course/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseCode,CourseName,SchoolId,CrossUniRegistration,CourseStatusId,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] CourseEntity courseEntity)
        {
            if (id != courseEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    courseEntity.CreatedAt = DateTime.SpecifyKind(courseEntity.CreatedAt, DateTimeKind.Utc);
                    courseEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{courseEntity.Id.ToString()}*");
                    context.Update(courseEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseEntityExists(courseEntity.Id))
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
            ViewData["CourseStatus"] = new SelectList(context.CourseStatuses, "Id", "CourseStatus", courseEntity.CourseStatusId);
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name", courseEntity.SchoolId);
            return View(courseEntity);
        }

        // GET: Course/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseEntity = await context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseEntity == null)
            {
                return NotFound();
            }

            return View(courseEntity);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseEntity = await context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{courseEntity.Id.ToString()}*");
                context.Courses.Remove(courseEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseEntityExists(Guid id)
        {
            return context.Courses.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
