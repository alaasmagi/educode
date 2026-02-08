using App.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class ClassroomController(
        IClassroomRepository classroomRepository,
        ISchoolRepository schoolRepository,
        ICacheRepository cache) : Controller
    {
        // GET: Classroom
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await classroomRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await classroomRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<ClassroomEntity>(
                items ?? new List<ClassroomEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: Classroom/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroomEntity = await classroomRepository.GetByIdAsync(id.Value, true);
            if (classroomEntity == null)
            {
                return NotFound();
            }

            return View(classroomEntity);
        }

        // GET: Classroom/Create
        public async Task<IActionResult> Create()
        {
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View();
        }

        // POST: Classroom/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Classroom,SchoolId,Deleted")] ClassroomEntity classroomEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            if (ModelState.IsValid)
            {
                classroomEntity.CreatedBy = email;
                classroomEntity.CreatedByClient = clientApp;
                classroomEntity.UpdatedBy = email;
                classroomEntity.UpdatedByClient = clientApp;
                await classroomRepository.CreateAsync(classroomEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View(classroomEntity);
        }

        // GET: Classroom/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroomEntity = await classroomRepository.GetByIdAsync(id.Value, true);
            if (classroomEntity == null)
            {
                return NotFound();
            }
            
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View(classroomEntity);
        }

        // POST: Classroom/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Classroom,SchoolId,Id,CreatedBy,CreatedByClient,CreatedAt,Deleted")] ClassroomEntity classroomEntity)
        {
            if (id != classroomEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            if (ModelState.IsValid)
            {
                classroomEntity.UpdatedBy = email;
                classroomEntity.UpdatedByClient = clientApp;

                await cache.DeletePatternAsync($"*{classroomEntity.Id.ToString()}*");
                var result = await classroomRepository.UpdateAsync(classroomEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["School"] = new SelectList(schools, "Id", "Name", classroomEntity.SchoolId);
            return View(classroomEntity);
        }

        // GET: Classroom/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroomEntity = await classroomRepository.GetByIdAsync(id.Value, true);
            if (classroomEntity == null)
            {
                return NotFound();
            }

            return View(classroomEntity);
        }

        // POST: Classroom/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var classroomEntity = await classroomRepository.GetByIdAsync(id);
            if (classroomEntity != null)
            {
                await classroomRepository.RemoveAsync(classroomEntity);
                await cache.DeletePatternAsync($"*{classroomEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
