using App.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseStatusController(
        ICourseStatusRepository courseStatusRepository,
        ICacheRepository cache) : Controller
    {
        // GET: CourseStatus
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await courseStatusRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await courseStatusRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<CourseStatusEntity>(
                items ?? new List<CourseStatusEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: CourseStatus/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseStatusEntity = await courseStatusRepository.GetByIdAsync(id.Value, true);
            if (courseStatusEntity == null)
            {
                return NotFound();
            }

            return View(courseStatusEntity);
        }

        // GET: CourseStatus/Create
        public IActionResult Create()
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            var courseStatusEntity = new CourseStatusEntity
            {
                CreatedBy = email,
                CreatedByClient = clientApp,
                UpdatedBy = email,
                UpdatedByClient = clientApp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return View(courseStatusEntity);
        }

        // POST: CourseStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("StatusName,Deleted,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt")] CourseStatusEntity courseStatusEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            var now = DateTime.UtcNow;
            
            courseStatusEntity.CreatedBy = email;
            courseStatusEntity.CreatedByClient = clientApp;
            courseStatusEntity.UpdatedBy = email;
            courseStatusEntity.UpdatedByClient = clientApp;
            courseStatusEntity.CreatedAt = now;
            courseStatusEntity.UpdatedAt = now;
            
            if (ModelState.IsValid)
            {
                await courseStatusRepository.CreateAsync(courseStatusEntity);
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

            var courseStatusEntity = await courseStatusRepository.GetByIdAsync(id.Value, true);
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
        public async Task<IActionResult> Edit(Guid id, [Bind("StatusName,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt,Deleted")] CourseStatusEntity courseStatusEntity)
        {
            if (id != courseStatusEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            courseStatusEntity.UpdatedBy = email;
            courseStatusEntity.UpdatedByClient = clientApp;
            courseStatusEntity.UpdatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await cache.DeletePatternAsync($"*{courseStatusEntity.Id.ToString()}*");
                var result = await courseStatusRepository.UpdateAsync(courseStatusEntity);

                if (result == null)
                {
                    return NotFound();
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

            var courseStatusEntity = await courseStatusRepository.GetByIdAsync(id.Value, true);
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
            var courseStatusEntity = await courseStatusRepository.GetByIdAsync(id, true);
            if (courseStatusEntity != null)
            {
                await courseStatusRepository.RemoveAsync(courseStatusEntity);
                await cache.DeletePatternAsync($"*{courseStatusEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

