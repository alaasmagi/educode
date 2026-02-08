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
    public class AttendanceTypeController(
        IAttendanceTypeRepository attendanceTypeRepository,
        ICacheRepository cache) : Controller
    {
        // GET: AttendanceType
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await attendanceTypeRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await attendanceTypeRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<AttendanceTypeEntity>(
                items ?? new List<AttendanceTypeEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: AttendanceType/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceTypeEntity = await attendanceTypeRepository.GetByIdAsync(id.Value, true);
            if (attendanceTypeEntity == null)
            {
                return NotFound();
            }

            return View(attendanceTypeEntity);
        }

        // GET: AttendanceType/Create
        public IActionResult Create()
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            var attendanceTypeEntity = new AttendanceTypeEntity
            {
                CreatedBy = email,
                CreatedByClient = clientApp,
                UpdatedBy = email,
                UpdatedByClient = clientApp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return View(attendanceTypeEntity);
        }

        // POST: AttendanceType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("TypeName,Deleted,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt")] AttendanceTypeEntity attendanceTypeEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            var now = DateTime.UtcNow;
            
            attendanceTypeEntity.CreatedBy = email;
            attendanceTypeEntity.CreatedByClient = clientApp;
            attendanceTypeEntity.UpdatedBy = email;
            attendanceTypeEntity.UpdatedByClient = clientApp;
            attendanceTypeEntity.CreatedAt = now;
            attendanceTypeEntity.UpdatedAt = now;
            
            if (ModelState.IsValid)
            {
                await attendanceTypeRepository.CreateAsync(attendanceTypeEntity);
                return RedirectToAction(nameof(Index));
            }
            return View(attendanceTypeEntity);
        }

        // GET: AttendanceType/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceTypeEntity = await attendanceTypeRepository.GetByIdAsync(id.Value, true);
            if (attendanceTypeEntity == null)
            {
                return NotFound();
            }
            return View(attendanceTypeEntity);
        }

        // POST: AttendanceType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("TypeName,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt,Deleted")] AttendanceTypeEntity attendanceTypeEntity)
        {
            if (id != attendanceTypeEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            attendanceTypeEntity.UpdatedBy = email;
            attendanceTypeEntity.UpdatedByClient = clientApp;
            attendanceTypeEntity.UpdatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await cache.DeletePatternAsync($"*{attendanceTypeEntity.Id.ToString()}*");
                var result = await attendanceTypeRepository.UpdateAsync(attendanceTypeEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(attendanceTypeEntity);
        }

        // GET: AttendanceType/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendanceTypeEntity = await attendanceTypeRepository.GetByIdAsync(id.Value, true);
            if (attendanceTypeEntity == null)
            {
                return NotFound();
            }

            return View(attendanceTypeEntity);
        }

        // POST: AttendanceType/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var attendanceTypeEntity = await attendanceTypeRepository.GetByIdAsync(id);
            if (attendanceTypeEntity != null)
            {
                await attendanceTypeRepository.RemoveAsync(attendanceTypeEntity);
                await cache.DeletePatternAsync($"*{attendanceTypeEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
