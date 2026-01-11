using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class WorkplaceController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: Workplace
        public async Task<IActionResult> Index()
        {
           return View(await context.Workplaces.Include(w => w.Classroom).IgnoreQueryFilters().ToListAsync());
        }

        // GET: Workplace/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplaceEntity = await context.Workplaces
                .IgnoreQueryFilters()
                .Include(m => m.Classroom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workplaceEntity == null)
            {
                return NotFound();
            }

            return View(workplaceEntity);
        }

        // GET: Workplace/Create
        public IActionResult Create()
        { 
            ViewData["Classroom"] = new SelectList(context.Classrooms, "Id", "Classroom");
            return View();
        }

        // POST: Workplace/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Identifier,ClassroomId,ClassRoom,ComputerCode,CreatedBy,UpdatedBy,Deleted")] WorkplaceEntity workplaceEntity)
        {
            if (ModelState.IsValid)
            {
                workplaceEntity.UpdatedAt = DateTime.UtcNow;
                workplaceEntity.CreatedAt = DateTime.UtcNow;
                context.Add(workplaceEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Classroom"] = new SelectList(context.Classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // GET: Workplace/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplaceEntity = await context.Workplaces
                .Include(w => w.Classroom)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (workplaceEntity == null)
            {
                return NotFound();
            }
            ViewData["Classroom"] = new SelectList(context.Classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // POST: Workplace/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Identifier,ClassroomId,ComputerCode,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] WorkplaceEntity workplaceEntity)
        {
            if (id != workplaceEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    workplaceEntity.CreatedAt = DateTime.SpecifyKind(workplaceEntity.CreatedAt, DateTimeKind.Utc);
                    workplaceEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{workplaceEntity.Id.ToString()}*");
                    context.Update(workplaceEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkplaceEntityExists(workplaceEntity.Id))
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
            ViewData["Classroom"] = new SelectList(context.Classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // GET: Workplace/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplaceEntity = await context.Workplaces
                .IgnoreQueryFilters()
                .Include(m => m.Classroom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workplaceEntity == null)
            {
                return NotFound();
            }
            ViewData["Classroom"] = new SelectList(context.Classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // POST: Workplace/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var workplaceEntity = await context.Workplaces
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (workplaceEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{workplaceEntity.Id.ToString()}*");
                context.Workplaces.Remove(workplaceEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkplaceEntityExists(Guid id)
        {
            return context.Workplaces.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
