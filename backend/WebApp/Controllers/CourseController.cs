using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseController(
        ICourseRepository courseRepository,
        ICourseStatusRepository courseStatusRepository,
        ISchoolRepository schoolRepository,
        ICacheRepository cache) : Controller
    {
        // GET: Course
        public async Task<IActionResult> Index()
        {
            var result = await courseRepository.GetAllAsync(1, 100, true);
            return View(result);
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseEntity = await courseRepository.GetByIdAsync(id.Value, true);
            if (courseEntity == null)
            {
                return NotFound();
            }

            return View(courseEntity);
        }

        // GET: Course/Create
        public async Task<IActionResult> Create()
        {
            var courseStatuses = await courseStatusRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["CourseStatus"] = new SelectList(courseStatuses, "Id", "CourseStatus");
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View();
        }

        // POST: Course/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseCode,CourseName,SchoolId,CrossUniRegistration,CourseStatusId,CreatedBy,UpdatedBy,Deleted")] CourseEntity courseEntity)
        {
            if (ModelState.IsValid)
            {
                await courseRepository.UpdateAsync(courseEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var courseStatuses = await courseStatusRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["CourseStatus"] = new SelectList(courseStatuses, "Id", "CourseStatus", courseEntity.CourseStatusId);
            ViewData["School"] = new SelectList(schools, "Id", "Name", courseEntity.SchoolId);
            return View(courseEntity);
        }

        // GET: Course/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseEntity = await courseRepository.GetByIdAsync(id.Value, true);
            if (courseEntity == null)
            {
                return NotFound();
            }
            
            var courseStatuses = await courseStatusRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["CourseStatus"] = new SelectList(courseStatuses, "Id", "CourseStatus");
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View(courseEntity);
        }

        // POST: Course/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseCode,CourseName,SchoolId,CrossUniRegistration,CourseStatusId,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] CourseEntity courseEntity)
        {
            if (id != courseEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                await cache.DeletePatternAsync($"*{courseEntity.Id.ToString()}*");
                var result = await courseRepository.UpdateAsync(courseEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var courseStatuses = await courseStatusRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["CourseStatus"] = new SelectList(courseStatuses, "Id", "CourseStatus", courseEntity.CourseStatusId);
            ViewData["School"] = new SelectList(schools, "Id", "Name", courseEntity.SchoolId);
            return View(courseEntity);
        }

        // GET: Course/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseEntity = await courseRepository.GetByIdAsync(id.Value, true);
            if (courseEntity == null)
            {
                return NotFound();
            }

            return View(courseEntity);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseEntity = await courseRepository.GetByIdAsync(id);
            if (courseEntity != null)
            {
                await courseRepository.RemoveAsync(courseEntity);
                await cache.DeletePatternAsync($"*{courseEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

