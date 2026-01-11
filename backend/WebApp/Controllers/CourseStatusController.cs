using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseStatusController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: CourseStatus
        public async Task<IActionResult> Index()
        {
            return View(await context.CourseStatuses.IgnoreQueryFilters().ToListAsync());
        }

        // GET: CourseStatus/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseStatusEntity = await context.CourseStatuses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseStatusEntity == null)
            {
                return NotFound();
            }

            return View(courseStatusEntity);
        }

        // GET: CourseStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CourseStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseStatus,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")] CourseStatusEntity courseStatusEntity)
        {
            if (ModelState.IsValid)
            {
                courseStatusEntity.Id = Guid.NewGuid();
                courseStatusEntity.CreatedAt = DateTime.UtcNow;
                courseStatusEntity.UpdatedAt = DateTime.UtcNow;
                context.Add(courseStatusEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(courseStatusEntity);
        }

        // GET: CourseStatus/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseStatusEntity = await context.CourseStatuses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseStatusEntity == null)
            {
                return NotFound();
            }
            return View(courseStatusEntity);
        }

        // POST: CourseStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseStatus,Id,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")] CourseStatusEntity courseStatusEntity)
        {
            if (id != courseStatusEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    courseStatusEntity.CreatedAt = DateTime.SpecifyKind(courseStatusEntity.CreatedAt, DateTimeKind.Utc);
                    courseStatusEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{courseStatusEntity.Id.ToString()}*");
                    context.Update(courseStatusEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseStatusEntityExists(courseStatusEntity.Id))
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
            return View(courseStatusEntity);
        }

        // GET: CourseStatus/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseStatusEntity = await context.CourseStatuses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseStatusEntity == null)
            {
                return NotFound();
            }

            return View(courseStatusEntity);
        }

        // POST: CourseStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseStatusEntity = await context.CourseStatuses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courseStatusEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{courseStatusEntity.Id.ToString()}*");
                context.CourseStatuses.Remove(courseStatusEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseStatusEntityExists(Guid id)
        {
            return context.CourseStatuses.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
