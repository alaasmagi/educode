using App.BLL;
using App.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Policy = nameof(EAccessLevel.QuinaryLevel))]
    public class UserController(AppDbContext context, RedisRepository redis, EnvInitializer envInitializer)
        : Controller
    {
        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = context.Users.Include(u => u.UserType).Include(u=>u.School);
            return View(await users.IgnoreQueryFilters().ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await context.Users
                .IgnoreQueryFilters()
                .Include(u => u.UserType)
                .Include(u => u.School)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userEntity == null)
            {
                return NotFound();
            }

            ViewData["PhotoLink"] = envInitializer.OciPublicUrl + userEntity.PhotoPath;
            return View(userEntity);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewData["UserType"] = new SelectList(context.UserTypes, "Id", "UserType");
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
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
                userEntity.UpdatedAt = DateTime.UtcNow;
                userEntity.CreatedAt = DateTime.UtcNow;
                context.Add(userEntity);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["UserType"] = new SelectList(context.UserTypes, "Id", "UserType", userEntity.UserTypeId);
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View(userEntity);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userEntity == null)
            {
                return NotFound();
            }
            ViewData["UserType"] = new SelectList(context.UserTypes, "Id", "UserType", userEntity.UserTypeId);
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
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
                try
                {
                    userEntity.CreatedAt = DateTime.SpecifyKind(userEntity.CreatedAt, DateTimeKind.Utc);
                    userEntity.UpdatedAt = DateTime.UtcNow;
                    await redis.DeleteKeysByPatternAsync($"*{userEntity.Id}*");
                    await redis.DeleteKeysByPatternAsync($"*{userEntity.Email}*");
                    context.Update(userEntity);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserEntityExists(userEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserType"] = new SelectList(context.UserTypes, "Id", "UserType", userEntity.UserType);
            ViewData["School"] = new SelectList(context.Schools, "Id", "Name");
            return View(userEntity);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await context.Users
                .IgnoreQueryFilters()
                .Include(u => u.UserType)
                .Include(u => u.School)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var userEntity = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userEntity != null)
            {
                await redis.DeleteKeysByPatternAsync($"*{userEntity.Id}*");
                await redis.DeleteKeysByPatternAsync($"*{userEntity.Email}*");
                context.Users.Remove(userEntity);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserEntityExists(Guid id)
        {
            return context.Users.IgnoreQueryFilters().Any(e => e.Id == id);
        }
    }
}
