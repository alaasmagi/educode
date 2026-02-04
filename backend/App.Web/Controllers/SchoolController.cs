using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class SchoolController(
        ISchoolRepository schoolRepository,
        ICacheRepository cache) : Controller
    {
        // GET: School
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await schoolRepository.GetAllAsync(pageNumber, pageSize, true);
            var totalCount = await schoolRepository.CountAsync(true);
            
            var paginatedList = new PaginatedList<SchoolEntity>(
                items ?? new List<SchoolEntity>(),
                totalCount,
                pageNumber,
                pageSize
            );
            
            return View(paginatedList);
        }

        // GET: School/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEntity = await schoolRepository.GetByIdAsync(id.Value, true);
            if (schoolEntity == null)
            {
                return NotFound();
            }

            return View(schoolEntity);
        }

        // GET: School/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: School/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Name,ShortName,Domain,PhotoPath,StudentCodePattern,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")]
            SchoolEntity schoolEntity)
        {
            if (ModelState.IsValid)
            {
                await schoolRepository.CreateAsync(schoolEntity);
                return RedirectToAction(nameof(Index));
            }

            return View(schoolEntity);
        }

        // GET: School/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEntity = await schoolRepository.GetByIdAsync(id.Value, true);
            if (schoolEntity == null)
            {
                return NotFound();
            }

            return View(schoolEntity);
        }

        // POST: School/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("Name,ShortName,Domain,PhotoPath,StudentCodePattern,Id,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt,Deleted")]
            SchoolEntity schoolEntity)
        {
            if (id != schoolEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                await cache.DeletePatternAsync($"*{schoolEntity.Id.ToString()}*");
                var result = await schoolRepository.UpdateAsync(schoolEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(schoolEntity);
        }

        // GET: School/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEntity = await schoolRepository.GetByIdAsync(id.Value, true);
            if (schoolEntity == null)
            {
                return NotFound();
            }

            return View(schoolEntity);
        }

        // POST: School/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var schoolEntity = await schoolRepository.GetByIdAsync(id);
            if (schoolEntity != null)
            {
                await schoolRepository.RemoveAsync(schoolEntity);
                await cache.DeletePatternAsync($"*{schoolEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

