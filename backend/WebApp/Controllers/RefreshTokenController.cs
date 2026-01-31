using App.DAL.Contracts;
using App.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class RefreshTokenController(
        IRefreshTokenRepository refreshTokenRepository,
        ICacheRepository cache) : Controller
    {
        // GET: RefreshToken
        public async Task<IActionResult> Index()
        {
            var result = await refreshTokenRepository.GetAllAsync(1, 100, true);
            return View(result);
        }

        // GET: RefreshToken/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refreshTokenEntity = await refreshTokenRepository.GetByIdAsync(id.Value, true);
            if (refreshTokenEntity == null)
            {
                return NotFound();
            }

            return View(refreshTokenEntity);
        }

        // GET: RefreshToken/Create
        public async Task<IActionResult> Create()
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
                await refreshTokenRepository.UpdateAsync(refreshTokenEntity);
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

            var refreshTokenEntity = await refreshTokenRepository.GetByIdAsync(id.Value, true);
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

                await cache.DeletePatternAsync($"*{refreshTokenEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{refreshTokenEntity.Token}*");
                var result = await refreshTokenRepository.UpdateAsync(refreshTokenEntity);

                if (result == null)
                {
                    return NotFound();
                }

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

            var refreshTokenEntity = await refreshTokenRepository.GetByIdAsync(id.Value, true);
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
            var refreshTokenEntity = await refreshTokenRepository.GetByIdAsync(id);
            if (refreshTokenEntity != null)
            {
                await refreshTokenRepository.RemoveAsync(refreshTokenEntity);
                await cache.DeletePatternAsync($"*{refreshTokenEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{refreshTokenEntity.Token}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

