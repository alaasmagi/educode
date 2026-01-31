using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseStatusController(
        ICourseStatusRepository courseStatusRepository,
        ICacheRepository cache) : Controller
    {
        // GET: CourseStatus
        public async Task<IActionResult> Index()
        {
            var result = await courseStatusRepository.GetAllAsync(1, 100, true);
            return View(result);
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
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: CourseStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseStatus,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")] CourseStatusEntity courseStatusEntity)
        {
            if (ModelState.IsValid)
            {
                await courseStatusRepository.UpdateAsync(courseStatusEntity);
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
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseStatus,Id,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")] CourseStatusEntity courseStatusEntity)
        {
            if (id != courseStatusEntity.Id)
            {
                return NotFound();
            }

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
            var courseStatusEntity = await courseStatusRepository.GetByIdAsync(id);
            if (courseStatusEntity != null)
            {
                await courseStatusRepository.RemoveAsync(courseStatusEntity);
                await cache.DeletePatternAsync($"*{courseStatusEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

