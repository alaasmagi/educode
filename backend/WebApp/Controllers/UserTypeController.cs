using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class UserTypeController(
        IUserTypeRepository userTypeRepository,
        ICacheRepository cache) : Controller
    {
        // GET: UserType
        public async Task<IActionResult> Index()
        {
            var result = await userTypeRepository.GetAllAsync(1, 100, true);
            return View(result);
        }

        // GET: UserType/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTypeEntity = await userTypeRepository.GetByIdAsync(id.Value, true);
            if (userTypeEntity == null)
            {
                return NotFound();
            }

            return View(userTypeEntity);
        }

        // GET: UserType/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: UserType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("TypeName,CreatedBy,UpdatedBy,Deleted")] UserTypeEntity userTypeEntity)
        {
            if (ModelState.IsValid)
            {
                await userTypeRepository.UpdateAsync(userTypeEntity);
                return RedirectToAction(nameof(Index));
            }
            return View(userTypeEntity);
        }

        // GET: UserType/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTypeEntity = await userTypeRepository.GetByIdAsync(id.Value, true);
            if (userTypeEntity == null)
            {
                return NotFound();
            }
            return View(userTypeEntity);
        }

        // POST: UserType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("TypeName,Id,CreatedBy,CreatedAt,UpdatedBy,Deleted")] UserTypeEntity userTypeEntity)
        {
            if (id != userTypeEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                await cache.DeletePatternAsync($"*{userTypeEntity.Id.ToString()}*");
                var result = await userTypeRepository.UpdateAsync(userTypeEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(userTypeEntity);
        }

        // GET: UserType/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTypeEntity = await userTypeRepository.GetByIdAsync(id.Value, true);
            if (userTypeEntity == null)
            {
                return NotFound();
            }

            return View(userTypeEntity);
        }

        // POST: UserType/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userTypeEntity = await userTypeRepository.GetByIdAsync(id);
            if (userTypeEntity != null)
            {
                await userTypeRepository.RemoveAsync(userTypeEntity);
                await cache.DeletePatternAsync($"*{userTypeEntity.Id.ToString()}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

