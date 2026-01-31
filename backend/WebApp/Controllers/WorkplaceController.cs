using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using App.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels;

namespace WebApp.Controllers
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
            var classrooms = await classroomRepository.GetAllAsync(1, 100);
            ViewData["Classroom"] = new SelectList(classrooms, "Id", "Classroom");
            return View();
        }

        // POST: Workplace/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Identifier,ClassroomId,ClassRoom,ComputerCode,CreatedBy,UpdatedBy,Deleted")] WorkplaceEntity workplaceEntity)
        {
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
        public async Task<IActionResult> Edit(Guid id, [Bind("Identifier,ClassroomId,ComputerCode,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] WorkplaceEntity workplaceEntity)
        {
            if (id != workplaceEntity.Id)
            {
                return NotFound();
            }

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

