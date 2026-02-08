using App.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class AttendanceCheckController(
        IAttendanceCheckRepository attendanceCheckRepository, 
        IAttendanceRepository attendanceRepository,
        IWorkplaceRepository workplaceRepository, 
        ICacheRepository cache) : Controller
    {
        // GET: AttendanceCheck
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await attendanceCheckRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await attendanceCheckRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<AttendanceCheckEntity>(
                items ?? new List<AttendanceCheckEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: AttendanceCheck/Details/ID
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var attendanceCheckEntity =  await attendanceCheckRepository.GetByIdAsync(id.Value, true);
            if (attendanceCheckEntity == null)
            {
                return NotFound();
            }

            return View(attendanceCheckEntity);
        }

        // GET: AttendanceCheck/Create
        public async Task<IActionResult> Create()
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            var attendanceCheckEntity = new AttendanceCheckEntity
            {
                CreatedBy = email,
                CreatedByClient = clientApp,
                UpdatedBy = email,
                UpdatedByClient = clientApp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var workplaces = await workplaceRepository.GetAllAsync(1, 100);
            var attendances = await attendanceRepository.GetAllAsync(1, 100);
            ViewData["WorkplaceId"] = new SelectList(workplaces, "Id", "ClassRoom");
            ViewData["AttendanceId"] = new SelectList(attendances, "Id", "Id");
            return View(attendanceCheckEntity);
        }

        // POST: AttendanceCheck/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("StudentCode,FullName,AttendanceIdentifier,WorkplaceIdentifier,Deleted,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt")] AttendanceCheckEntity attendanceCheckEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            var now = DateTime.UtcNow;
            
            attendanceCheckEntity.CreatedBy = email;
            attendanceCheckEntity.CreatedByClient = clientApp;
            attendanceCheckEntity.UpdatedBy = email;
            attendanceCheckEntity.UpdatedByClient = clientApp;
            attendanceCheckEntity.CreatedAt = now;
            attendanceCheckEntity.UpdatedAt = now;
            
            if (ModelState.IsValid)
            {
                await attendanceCheckRepository.CreateAsync(attendanceCheckEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var workplaces = await workplaceRepository.GetAllAsync(1, 100);
            ViewData["WorkplaceId"] = new SelectList(workplaces, "Id", "ClassRoom", attendanceCheckEntity.WorkplaceIdentifier);
            return View(attendanceCheckEntity);
        }

        // GET: AttendanceCheck/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceCheckEntity = await attendanceCheckRepository.GetByIdAsync(id.Value, true);
            if (attendanceCheckEntity == null)
            {
                return NotFound();
            }
            
            var workplaces = await workplaceRepository.GetAllAsync(1, 100);
            var attendances = await attendanceRepository.GetAllAsync(1, 100);
            ViewData["WorkplaceIdentifier"] = new SelectList(workplaces, "Id", "ClassRoom", attendanceCheckEntity.WorkplaceIdentifier);
            ViewData["AttendanceIdentifier"] = new SelectList(attendances, "Id", "Id", attendanceCheckEntity.AttendanceIdentifier);
            return View(attendanceCheckEntity);
        }

        // POST: AttendanceCheck/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("StudentId,FullName,AttendanceIdentifier,WorkplaceIdentifier,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt,Deleted")] AttendanceCheckEntity attendanceCheckEntity)
        {
            if (id != attendanceCheckEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            attendanceCheckEntity.UpdatedBy = email;
            attendanceCheckEntity.UpdatedByClient = clientApp;
            attendanceCheckEntity.UpdatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await cache.DeletePatternAsync($"*{attendanceCheckEntity.Id.ToString()}*");
                var result = await attendanceCheckRepository.UpdateAsync(attendanceCheckEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var workplaces = await workplaceRepository.GetAllAsync(1, 100);
            ViewData["WorkplaceId"] = new SelectList(workplaces, "Id", "ClassRoom", attendanceCheckEntity.WorkplaceIdentifier);
            return View(attendanceCheckEntity);
        }

        // GET: AttendanceCheck/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceCheckEntity = await attendanceCheckRepository.GetByIdAsync(id.Value, true);
            if (attendanceCheckEntity == null)
            {
                return NotFound();
            }

            return View(attendanceCheckEntity);
        }

        // POST: AttendanceCheck/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var attendanceCheckEntity = await attendanceCheckRepository.GetByIdAsync(id);
            if (attendanceCheckEntity != null)
            {
                await attendanceCheckRepository.RemoveAsync(attendanceCheckEntity);
                await cache.DeletePatternAsync($"*{attendanceCheckEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
