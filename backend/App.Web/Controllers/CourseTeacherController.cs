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
    public class CourseTeacherController(
        ICourseTeacherRepository courseTeacherRepository,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        ICacheRepository cache) : Controller
    {
        // GET: CourseTeacher
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await courseTeacherRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await courseTeacherRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<CourseTeacherEntity>(
                items ?? new List<CourseTeacherEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: CourseTeacher/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseTeacherEntity = await courseTeacherRepository.GetByIdAsync(id.Value, true);
            if (courseTeacherEntity == null)
            {
                return NotFound();
            }

            return View(courseTeacherEntity);
        }

        // GET: CourseTeacher/Create
        public async Task<IActionResult> Create()
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            
            var courseTeacherEntity = new CourseTeacherEntity
            {
                CreatedBy = email,
                CreatedByClient = clientApp,
                UpdatedBy = email,
                UpdatedByClient = clientApp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var courses = await courseRepository.GetAllAsync(1, 100);
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code");
            ViewData["TeacherId"] = new SelectList(users, "Id", "Email");
            return View(courseTeacherEntity);
        }

        // POST: CourseTeacher/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseId,TeacherId,Deleted,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt")] CourseTeacherEntity courseTeacherEntity)
        {
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
            var now = DateTime.UtcNow;
            
            courseTeacherEntity.CreatedBy = email;
            courseTeacherEntity.CreatedByClient = clientApp;
            courseTeacherEntity.UpdatedBy = email;
            courseTeacherEntity.UpdatedByClient = clientApp;
            courseTeacherEntity.CreatedAt = now;
            courseTeacherEntity.UpdatedAt = now;
            
            if (ModelState.IsValid)
            {
                await courseTeacherRepository.CreateAsync(courseTeacherEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var courses = await courseRepository.GetAllAsync(1, 100);
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // GET: CourseTeacher/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseTeacherEntity = await courseTeacherRepository.GetByIdAsync(id.Value, true);
            if (courseTeacherEntity == null)
            {
                return NotFound();
            }
            
            var courses = await courseRepository.GetAllAsync(1, 100);
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // POST: CourseTeacher/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseId,TeacherId,Id,CreatedBy,CreatedByClient,CreatedAt,UpdatedBy,UpdatedByClient,UpdatedAt,Deleted")] CourseTeacherEntity courseTeacherEntity)
        {
            if (id != courseTeacherEntity.Id)
            {
                return NotFound();
            }

            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            courseTeacherEntity.UpdatedBy = email;
            courseTeacherEntity.UpdatedByClient = clientApp;
            courseTeacherEntity.UpdatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await cache.DeletePatternAsync($"*{courseTeacherEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{courseTeacherEntity.CourseId.ToString()}*");
                await cache.DeletePatternAsync($"*{courseTeacherEntity.TeacherId.ToString()}*");
                var result = await courseTeacherRepository.UpdateAsync(courseTeacherEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var courses = await courseRepository.GetAllAsync(1, 100);
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // GET: CourseTeacher/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseTeacherEntity = await courseTeacherRepository.GetByIdAsync(id.Value, true);
            if (courseTeacherEntity == null)
            {
                return NotFound();
            }

            return View(courseTeacherEntity);
        }

        // POST: CourseTeacher/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseTeacherEntity = await courseTeacherRepository.GetByIdAsync(id);
            if (courseTeacherEntity != null)
            {
                await courseTeacherRepository.RemoveAsync(courseTeacherEntity);
                await cache.DeletePatternAsync($"*{courseTeacherEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{courseTeacherEntity.CourseId.ToString()}*");
                await cache.DeletePatternAsync($"*{courseTeacherEntity.TeacherId.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

