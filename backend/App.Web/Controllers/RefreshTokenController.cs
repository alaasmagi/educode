﻿using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class RefreshTokenController(
        IRefreshTokenRepository refreshTokenRepository,
        ICacheRepository cache) : Controller
    {
        // GET: RefreshToken
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await refreshTokenRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await refreshTokenRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<RefreshTokenEntity>(
                items ?? new List<RefreshTokenEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
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
        public IActionResult Create()
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            var refreshTokenEntity = new RefreshTokenEntity
            {
                CreatedBy = email,
                CreatedByClient = clientApp,
                UpdatedBy = email,
                UpdatedByClient = clientApp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return View(refreshTokenEntity);
        }

        // POST: RefreshToken/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("UserId,Token,Client,ClientIp,PushNotificationToken,ExpirationTime,Deleted,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt")]
            RefreshTokenEntity refreshTokenEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            var now = DateTime.UtcNow;
            
            // Override with current values
            refreshTokenEntity.CreatedBy = email;
            refreshTokenEntity.CreatedByClient = clientApp;
            refreshTokenEntity.UpdatedBy = email;
            refreshTokenEntity.UpdatedByClient = clientApp;
            refreshTokenEntity.CreatedAt = now;
            refreshTokenEntity.UpdatedAt = now;
            
            if (ModelState.IsValid)
            {
                await refreshTokenRepository.CreateAsync(refreshTokenEntity);
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
            [Bind("UserId,Token,Client,ClientIp,PushNotificationToken,ExpirationTime,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt,Deleted")]
            RefreshTokenEntity refreshTokenEntity)
        {
            if (id != refreshTokenEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            refreshTokenEntity.UpdatedBy = email;
            refreshTokenEntity.UpdatedByClient = clientApp;
            refreshTokenEntity.UpdatedAt = DateTime.UtcNow;

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
            var refreshTokenEntity = await refreshTokenRepository.GetByIdAsync(id, true);
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

