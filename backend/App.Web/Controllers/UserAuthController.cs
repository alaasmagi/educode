using App.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class UserAuthController(
        IUserAuthRepository userAuthRepository,
        IUserRepository userRepository,
        ICacheRepository cache) : Controller
    {
        // GET: UserAuth
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await userAuthRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await userAuthRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<UserAuthEntity>(
                items ?? new List<UserAuthEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: UserAuth/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAuthEntity = await userAuthRepository.GetByIdAsync(id.Value, true);
            if (userAuthEntity == null)
            {
                return NotFound();
            }

            return View(userAuthEntity);
        }

        // GET: UserAuth/Create
        public async Task<IActionResult> Create()
        {
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["UserId"] = new SelectList(users, "Id", "Email");
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
                await userAuthRepository.CreateAsync(userAuthEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["UserId"] = new SelectList(users, "Id", "Email", userAuthEntity.UserId);
            return View(userAuthEntity);
        }

        // GET: UserAuth/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAuthEntity = await userAuthRepository.GetByIdAsync(id.Value, true);
            if (userAuthEntity == null)
            {
                return NotFound();
            }
            
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["UserId"] = new SelectList(users, "Id", "Email");
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

                await cache.DeletePatternAsync($"*{userAuthEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{userAuthEntity.UserId.ToString()}*");
                var result = await userAuthRepository.UpdateAsync(userAuthEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["UserId"] = new SelectList(users, "Id", "Email", userAuthEntity.UserId);
            return View(userAuthEntity);
        }

        // GET: UserAuth/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAuthEntity = await userAuthRepository.GetByIdAsync(id.Value, true);
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
            var userAuthEntity = await userAuthRepository.GetByIdAsync(id, true);
            if (userAuthEntity != null)
            {
                await userAuthRepository.RemoveAsync(userAuthEntity);
                await cache.DeletePatternAsync($"*{userAuthEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{userAuthEntity.UserId.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

