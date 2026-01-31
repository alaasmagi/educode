using App.DAL.Contracts;
using App.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class UserController(
        IUserRepository userRepository,
        IUserTypeRepository userTypeRepository,
        ISchoolRepository schoolRepository,
        ICacheRepository cache,
        EnvInitializer envInitializer) : Controller
    {
        // GET: User
        public async Task<IActionResult> Index()
        {
            var result = await userRepository.GetAllAsync(1, 100, true);
            return View(result);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await userRepository.GetByIdAsync(id.Value, true);
            if (userEntity == null)
            {
                return NotFound();
            }

            ViewData["PhotoLink"] = envInitializer.OciPublicUrl + userEntity.PhotoPath;
            return View(userEntity);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            var userTypes = await userTypeRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["UserType"] = new SelectList(userTypes, "Id", "UserType");
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("UserTypeId,SchoolId,Email,StudentCode,FullName,PhotoPath,CreatedBy,UpdatedBy,Deleted")] UserEntity userEntity)
        {
            if (ModelState.IsValid)
            {
                await userRepository.UpdateAsync(userEntity);
                return RedirectToAction(nameof(Index));
            }
            
            var userTypes = await userTypeRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["UserType"] = new SelectList(userTypes, "Id", "UserType", userEntity.UserTypeId);
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View(userEntity);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await userRepository.GetByIdAsync(id.Value, true);
            if (userEntity == null)
            {
                return NotFound();
            }
            
            var userTypes = await userTypeRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["UserType"] = new SelectList(userTypes, "Id", "UserType", userEntity.UserTypeId);
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            ViewData["CreatedAt"] = userEntity.CreatedAt;
            ViewData["CreatedBy"] = userEntity.CreatedBy;
            return View(userEntity);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("UserTypeId,SchoolId,Email,StudentCode,FullName,Id,PhotoPath,CreatedBy,CreatedAt,UpdatedBy,Deleted")] UserEntity userEntity)
        {
            if (id != userEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                await cache.DeletePatternAsync($"*{userEntity.Id}*");
                await cache.DeletePatternAsync($"*{userEntity.Email}*");
                var result = await userRepository.UpdateAsync(userEntity);

                if (result == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            
            var userTypes = await userTypeRepository.GetAllAsync(1, 100);
            var schools = await schoolRepository.GetAllAsync(1, 100);
            ViewData["UserType"] = new SelectList(userTypes, "Id", "UserType", userEntity.UserType);
            ViewData["School"] = new SelectList(schools, "Id", "Name");
            return View(userEntity);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await userRepository.GetByIdAsync(id.Value, true);
            if (userEntity == null)
            {
                return NotFound();
            }

            return View(userEntity);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userEntity = await userRepository.GetByIdAsync(id);
            if (userEntity != null)
            {
                await userRepository.RemoveAsync(userEntity);
                await cache.DeletePatternAsync($"*{userEntity.Id}*");
                await cache.DeletePatternAsync($"*{userEntity.Email}*");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

