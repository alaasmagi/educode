using App.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using App.Domain;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class WorkplaceController(
        IWorkplaceRepository workplaceRepository,
        IClassroomRepository classroomRepository,
        ICacheRepository cache) : Controller
    {
        // GET: Workplace
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await workplaceRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await workplaceRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<WorkplaceEntity>(
                items ?? new List<WorkplaceEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: Workplace/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplaceEntity = await workplaceRepository.GetByIdAsync(id.Value, true);
            if (workplaceEntity == null)
            {
                return NotFound();
            }

            return View(workplaceEntity);
        }

        // GET: Workplace/Create
        public async Task<IActionResult> Create()
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            var workplaceEntity = new WorkplaceEntity
            {
                CreatedBy = email,
                CreatedByClient = clientApp,
                UpdatedBy = email,
                UpdatedByClient = clientApp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var classrooms = await classroomRepository.GetAllAsync(1, 100);
            ViewData["Classroom"] = new SelectList(classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // POST: Workplace/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Identifier,ClassroomId,ClassRoom,ComputerCode,Deleted,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt")] WorkplaceEntity workplaceEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            var now = DateTime.UtcNow;
            
            workplaceEntity.CreatedBy = email;
            workplaceEntity.CreatedByClient = clientApp;
            workplaceEntity.UpdatedBy = email;
            workplaceEntity.UpdatedByClient = clientApp;
            workplaceEntity.CreatedAt = now;
            workplaceEntity.UpdatedAt = now;
            
            if (ModelState.IsValid)
            {
                await workplaceRepository.CreateAsync(workplaceEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var classrooms = await classroomRepository.GetAllAsync(1, 100);
            ViewData["Classroom"] = new SelectList(classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // GET: Workplace/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplaceEntity = await workplaceRepository.GetByIdAsync(id.Value, true);
            if (workplaceEntity == null)
            {
                return NotFound();
            }
            
            var classrooms = await classroomRepository.GetAllAsync(1, 100);
            ViewData["Classroom"] = new SelectList(classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // POST: Workplace/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Identifier,ClassroomId,ComputerCode,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt,Deleted")] WorkplaceEntity workplaceEntity)
        {
            if (id != workplaceEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            workplaceEntity.UpdatedBy = email;
            workplaceEntity.UpdatedByClient = clientApp;
            workplaceEntity.UpdatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await cache.DeletePatternAsync($"*{workplaceEntity.Id.ToString()}*");
                var result = await workplaceRepository.UpdateAsync(workplaceEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var classrooms = await classroomRepository.GetAllAsync(1, 100);
            ViewData["Classroom"] = new SelectList(classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // GET: Workplace/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplaceEntity = await workplaceRepository.GetByIdAsync(id.Value, true);
            if (workplaceEntity == null)
            {
                return NotFound();
            }
            
            var classrooms = await classroomRepository.GetAllAsync(1, 100);
            ViewData["Classroom"] = new SelectList(classrooms, "Id", "Classroom");
            return View(workplaceEntity);
        }

        // POST: Workplace/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var workplaceEntity = await workplaceRepository.GetByIdAsync(id);
            if (workplaceEntity != null)
            {
                await workplaceRepository.RemoveAsync(workplaceEntity);
                await cache.DeletePatternAsync($"*{workplaceEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

