using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class UserAuthController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: UserAuth
        public async Task<IActionResult> Index()
        {
            var appDbContext = context.UserAuthData.Include(u => u.User);
            return View(await appDbContext.IgnoreQueryFilters().ToListAsync());
        }

        // GET: UserAuth/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAuthEntity = await context.UserAuthData
                .IgnoreQueryFilters()
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userAuthEntity == null)
            {
                return NotFound();
            }

            return View(userAuthEntity);
        }

        // GET: UserAuth/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(context.Users, "Id", "Email");
            return View();
        }

        // POST: UserAuth/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("UserId,PasswordHash,CreatedBy,UpdatedBy,Deleted")] UserAuthEntity userAuthEntity)
        {
            if (ModelState.IsValid)
            {
                userAuthEntity.UpdatedAt = DateTime.UtcNow;
                userAuthEntity.CreatedAt = DateTime.UtcNow;
                context.Add(userAuthEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(context.Users, "Id", "Email", userAuthEntity.UserId);
            return View(userAuthEntity);
        }

        // GET: UserAuth/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAuthEntity = await context.UserAuthData
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userAuthEntity == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(context.Users, "Id", "Email");
            return View(userAuthEntity);
        }

        // POST: UserAuth/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("UserId,PasswordHash,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] UserAuthEntity userAuthEntity)
        {
            if (id != userAuthEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    userAuthEntity.CreatedAt = DateTime.SpecifyKind(userAuthEntity.CreatedAt, DateTimeKind.Utc);
                    userAuthEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{userAuthEntity.Id.ToString()}*");
                    await redis.DeleteKeysByPatternAsync($"*{userAuthEntity.UserId.ToString()}*");
                    context.Update(userAuthEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserAuthEntityExists(userAuthEntity.Id))
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
            ViewData["UserId"] = new SelectList(context.Users, "Id", "Email", userAuthEntity.UserId);
            return View(userAuthEntity);
        }

        // GET: UserAuth/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAuthEntity = await context.UserAuthData
                .IgnoreQueryFilters()
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userAuthEntity == null)
            {
                return NotFound();
            }

            return View(userAuthEntity);
        }

        // POST: UserAuth/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userAuthEntity = await context.UserAuthData
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userAuthEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{userAuthEntity.Id.ToString()}*");
                await redis.DeleteKeysByPatternAsync($"*{userAuthEntity.UserId.ToString()}*");
                context.UserAuthData.Remove(userAuthEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserAuthEntityExists(Guid id)
        {
            return context.UserAuthData.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
