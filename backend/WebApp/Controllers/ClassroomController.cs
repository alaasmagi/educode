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
    public class ClassroomController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: Classroom
        public async Task<IActionResult> Index()
        {
            return View(await context.Classrooms.Include(c => c.School).IgnoreQueryFilters().ToListAsync());
        }

        // GET: Classroom/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroomEntity = await context.Classrooms
                .Include(c => c.School)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classroomEntity == null)
            {
                return NotFound();
            }

            return View(classroomEntity);
        }

        // GET: Classroom/Create
        public IActionResult Create()
        {
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View();
        }

        // POST: Classroom/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Classroom,SchoolId,CreatedBy,UpdatedBy,Deleted")] ClassroomEntity classroomEntity)
        {
            if (ModelState.IsValid)
            {
                classroomEntity.UpdatedAt = DateTime.UtcNow;
                classroomEntity.CreatedAt = DateTime.UtcNow;
                context.Add(classroomEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View(classroomEntity);
        }

        // GET: Classroom/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroomEntity = await context.Classrooms
                .Include(c => c.School)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (classroomEntity == null)
            {
                return NotFound();
            }
            
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View(classroomEntity);
        }

        // POST: Classroom/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Classroom,SchoolId,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] ClassroomEntity classroomEntity)
        {
            if (id != classroomEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    classroomEntity.CreatedAt = DateTime.SpecifyKind(classroomEntity.CreatedAt, DateTimeKind.Utc);
                    classroomEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{classroomEntity.Id.ToString()}*");
                    context.Update(classroomEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassroomEntityExists(classroomEntity.Id))
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
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name", classroomEntity.SchoolId);
            return View(classroomEntity);
        }

        // GET: Classroom/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroomEntity = await context.Classrooms
                .Include(c => c.School)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classroomEntity == null)
            {
                return NotFound();
            }

            return View(classroomEntity);
        }

        // POST: Classroom/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var classroomEntity = await context.Classrooms
                .Include(c => c.School)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (classroomEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{classroomEntity.Id.ToString()}*");
                context.Classrooms.Remove(classroomEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassroomEntityExists(Guid id)
        {
            return context.Classrooms.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
