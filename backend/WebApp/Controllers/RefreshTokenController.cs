using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class RefreshTokenController(AppDbContext context, RedisRepository redis) : Controller
    {
        // GET: RefreshToken
        public async Task<IActionResult> Index()
        {
            return View(await context.RefreshTokens.IgnoreQueryFilters().ToListAsync());
        }

        // GET: RefreshToken/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refreshTokenEntity = await context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (refreshTokenEntity == null)
            {
                return NotFound();
            }

            return View(refreshTokenEntity);
        }

        // GET: RefreshToken/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RefreshToken/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("UserId,Token,CreatedByIp,ExpirationTime,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")]
            RefreshTokenEntity refreshTokenEntity)
        {
            if (ModelState.IsValid)
            {
                refreshTokenEntity.Id = Guid.NewGuid();
                refreshTokenEntity.CreatedAt = DateTime.UtcNow;
                refreshTokenEntity.UpdatedAt = DateTime.UtcNow;
                context.Add(refreshTokenEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(refreshTokenEntity);
        }

        // GET: RefreshToken/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refreshTokenEntity = await context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (refreshTokenEntity == null)
            {
                return NotFound();
            }

            return View(refreshTokenEntity);
        }

        // POST: RefreshToken/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("UserId,Token,CreatedByIp,ExpirationTime,Id,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")]
            RefreshTokenEntity refreshTokenEntity)
        {
            if (id != refreshTokenEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    refreshTokenEntity.CreatedAt = DateTime.SpecifyKind(refreshTokenEntity.CreatedAt, DateTimeKind.Utc);
                    refreshTokenEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{refreshTokenEntity.Id.ToString()}*");
                    await redis.DeleteKeysByPatternAsync($"*{refreshTokenEntity.Token}*");
                    context.Update(refreshTokenEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {}

                return RedirectToAction(nameof(Index));
            }

            return View(refreshTokenEntity);
        }

        // GET: RefreshToken/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refreshTokenEntity = await context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (refreshTokenEntity == null)
            {
                return NotFound();
            }

            return View(refreshTokenEntity);
        }

        // POST: RefreshToken/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var refreshTokenEntity = await context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (refreshTokenEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{refreshTokenEntity.Id.ToString()}*");
                await redis.DeleteKeysByPatternAsync($"*{refreshTokenEntity.Token}*");
                context.RefreshTokens.Remove(refreshTokenEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}