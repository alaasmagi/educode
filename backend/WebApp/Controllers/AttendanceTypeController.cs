using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class AttendanceTypeController(
        IAttendanceTypeRepository attendanceTypeRepository,
        ICacheRepository cache) : Controller
    {
        // GET: AttendanceType
        public async Task<IActionResult> Index()
        {
            var result = await attendanceTypeRepository.GetAllAsync(1, 100, true);
            return View(result);
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
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: AttendanceType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("TypeName,CreatedBy,UpdatedBy,Deleted")] AttendanceTypeEntity attendanceTypeEntity)
        {
            if (ModelState.IsValid)
            {
                await attendanceTypeRepository.UpdateAsync(attendanceTypeEntity);
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
        public async Task<IActionResult> Edit(Guid id, [Bind("TypeName,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] AttendanceTypeEntity attendanceTypeEntity)
        {
            if (id != attendanceTypeEntity.Id)
            {
                return NotFound();
            }

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
