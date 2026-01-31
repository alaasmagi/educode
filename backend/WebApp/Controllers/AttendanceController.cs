using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain;
using Microsoft.AspNetCore.Authorization;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class AttendanceController(
        IAttendanceRepository attendanceRepository,
        IAttendanceTypeRepository attendanceTypeRepository,
        ICourseRepository courseRepository,
        ICacheRepository cache) : Controller
    {

        // GET: CourseAttendance
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await attendanceRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await attendanceRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<AttendanceEntity>(
                items ?? new List<AttendanceEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: CourseAttendance/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAttendanceEntity = await attendanceRepository.GetByIdAsync(id.Value, true);
            if (courseAttendanceEntity == null)
            {
                return NotFound();
            }

            return View(courseAttendanceEntity);
        }

        // GET: CourseAttendance/Create
        public async Task<IActionResult> Create()
        {
            var attendanceTypes = await attendanceTypeRepository.GetAllAsync(1, 100);
            var courses = await courseRepository.GetAllAsync(1, 100);
            ViewData["AttendanceTypeId"] = new SelectList(attendanceTypes, "Id", "TypeName");
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code");
            return View();
        }

        // POST: CourseAttendance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseId,Identifier,TypeId,StartTime,EndTime,CreatedBy,UpdatedBy,Delete")] AttendanceEntity attendanceEntity)
        {
            if (ModelState.IsValid)
            {
                await attendanceRepository.CreateAsync(attendanceEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var attendanceTypes = await attendanceTypeRepository.GetAllAsync(1, 100);
            var courses = await courseRepository.GetAllAsync(1, 100);
            ViewData["AttendanceTypeId"] = new SelectList(attendanceTypes, "Id", "TypeName", attendanceEntity.TypeId);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code", attendanceEntity.CourseId);
            return View(attendanceEntity);
        }

        // GET: CourseAttendance/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAttendanceEntity = await attendanceRepository.GetByIdAsync(id.Value, true);
            if (courseAttendanceEntity == null)
            {
                return NotFound();
            }
            
            var attendanceTypes = await attendanceTypeRepository.GetAllAsync(1, 100);
            var courses = await courseRepository.GetAllAsync(1, 100);
            ViewData["AttendanceTypeId"] = new SelectList(attendanceTypes, "Id", "TypeName", courseAttendanceEntity.TypeId);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code", courseAttendanceEntity.CourseId);
            return View(courseAttendanceEntity);
        }

        // POST: CourseAttendance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("CourseId,Identifier,TypeId,StartTime,EndTime,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] AttendanceEntity attendanceEntity)
        {
            if (id != attendanceEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                await cache.DeletePatternAsync($"*{attendanceEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{attendanceEntity.Identifier}*");
                var result = await attendanceRepository.UpdateAsync(attendanceEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var attendanceTypes = await attendanceTypeRepository.GetAllAsync(1, 100);
            var courses = await courseRepository.GetAllAsync(1, 100);
            ViewData["AttendanceTypeId"] = new SelectList(attendanceTypes, "Id", "TypeName", attendanceEntity.TypeId);
            ViewData["CourseId"] = new SelectList(courses, "Id", "Code", attendanceEntity.CourseId);
            return View(attendanceEntity);
        }

        // GET: CourseAttendance/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAttendanceEntity = await attendanceRepository.GetByIdAsync(id.Value, true);
            if (courseAttendanceEntity == null)
            {
                return NotFound();
            }

            return View(courseAttendanceEntity);
        }

        // POST: CourseAttendance/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var courseAttendanceEntity = await attendanceRepository.GetByIdAsync(id);
            if (courseAttendanceEntity != null)
            {
                await attendanceRepository.RemoveAsync(courseAttendanceEntity);
                await cache.DeletePatternAsync($"*{courseAttendanceEntity.Id.ToString()}*");
                await cache.DeletePatternAsync($"*{courseAttendanceEntity.Identifier}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

