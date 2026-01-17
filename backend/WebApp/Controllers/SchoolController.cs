using App.BLL;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class SchoolController(AppDbContext context, RedisRepository redis, EnvInitializer envInitializer)
        : Controller
    {
        // GET: CourseStatus
        public async Task<IActionResult> Index()
        {
           return View(await context.Schools.IgnoreQueryFilters().ToListAsync());
        }

        // GET: CourseStatus/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEntity = await context.Schools
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolEntity == null)
            {
                return NotFound();
            }

            return View(schoolEntity);
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
        public async Task<IActionResult> Create(
            [Bind("Name,ShortName,Domain,PhotoPath,StudentCodePattern,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")]
            SchoolEntity schoolEntity)
        {
            if (ModelState.IsValid)
            {
                schoolEntity.Id = Guid.NewGuid();
                schoolEntity.CreatedAt = DateTime.UtcNow;
                schoolEntity.UpdatedAt = DateTime.UtcNow;
                context.Add(schoolEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(schoolEntity);
        }

        // GET: CourseStatus/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEntity = await context.Schools
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schoolEntity == null)
            {
                return NotFound();
            }

            return View(schoolEntity);
        }

        // POST: CourseStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("Name,ShortName,Domain,PhotoPath,StudentCodePattern,Id,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")]
            SchoolEntity schoolEntity)
        {
            if (id != schoolEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    schoolEntity.CreatedAt = DateTime.SpecifyKind(schoolEntity.CreatedAt, DateTimeKind.Utc);
                    schoolEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{schoolEntity.Id.ToString()}*");
                    context.Update(schoolEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException){}

                return RedirectToAction(nameof(Index));
            }

            return View(schoolEntity);
        }

        // GET: CourseStatus/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEntity = await context.Schools
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolEntity == null)
            {
                return NotFound();
            }

            return View(schoolEntity);
        }

        // POST: CourseStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var schoolEntity = await context.Schools
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schoolEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{schoolEntity.Id.ToString()}*");
                context.Schools.Remove(schoolEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}