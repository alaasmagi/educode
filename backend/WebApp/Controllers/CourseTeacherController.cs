using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class CourseTeacherController(
        ICourseTeacherRepository courseTeacherRepository,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        ICacheRepository cache) : Controller
    {
        // GET: CourseTeacher
        public async Task<IActionResult> Index()
        {
            var result = await courseTeacherRepository.GetAllAsync(1, 100, true);
            return View(result);
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
            var courses = await courseRepository.GetAllAsync(1, 100);
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseCode");
            ViewData["TeacherId"] = new SelectList(users, "Id", "Email");
            return View();
        }

        // POST: CourseTeacher/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseId,TeacherId,CreatedBy,UpdatedBy,Deleted")] CourseTeacherEntity courseTeacherEntity)
        {
            if (ModelState.IsValid)
            {
                await courseTeacherRepository.UpdateAsync(courseTeacherEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var courses = await courseRepository.GetAllAsync(1, 100);
            var users = await userRepository.GetAllAsync(1, 100);
            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseCode", courseTeacherEntity.CourseId);
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
            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseCode", courseTeacherEntity.CourseId);
            ViewData["TeacherId"] = new SelectList(users, "Id", "Email", courseTeacherEntity.TeacherId);
            return View(courseTeacherEntity);
        }

        // POST: CourseTeacher/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseId,TeacherId,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] CourseTeacherEntity courseTeacherEntity)
        {
            if (id != courseTeacherEntity.Id)
            {
                return NotFound();
            }

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
            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseCode", courseTeacherEntity.CourseId);
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

