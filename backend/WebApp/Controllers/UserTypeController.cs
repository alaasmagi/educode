using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class UserTypeController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: UserType
        public async Task<IActionResult> Index()
        {
            return View(await context.UserTypes.IgnoreQueryFilters().ToListAsync());
        }

        // GET: UserType/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTypeEntity = await context.UserTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userTypeEntity == null)
            {
                return NotFound();
            }

            return View(userTypeEntity);
        }

        // GET: UserType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("UserType,CreatedBy,UpdatedBy,Deleted")] UserTypeEntity userTypeEntity)
        {
            if (ModelState.IsValid)
            {
                userTypeEntity.UpdatedAt = DateTime.UtcNow;
                userTypeEntity.CreatedAt = DateTime.UtcNow;
                context.Add(userTypeEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userTypeEntity);
        }

        // GET: UserType/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTypeEntity = await context.UserTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userTypeEntity == null)
            {
                return NotFound();
            }
            return View(userTypeEntity);
        }

        // POST: UserType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("UserType,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] UserTypeEntity userTypeEntity)
        {
            if (id != userTypeEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    userTypeEntity.CreatedAt = DateTime.SpecifyKind(userTypeEntity.CreatedAt, DateTimeKind.Utc);
                    userTypeEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{userTypeEntity.Id.ToString()}*");
                    context.Update(userTypeEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTypeEntityExists(userTypeEntity.Id))
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
            return View(userTypeEntity);
        }

        // GET: UserType/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTypeEntity = await context.UserTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userTypeEntity == null)
            {
                return NotFound();
            }

            return View(userTypeEntity);
        }

        // POST: UserType/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userTypeEntity = await context.UserTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userTypeEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{userTypeEntity.Id.ToString()}*");
                context.UserTypes.Remove(userTypeEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserTypeEntityExists(Guid id)
        {
            return context.UserTypes.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
