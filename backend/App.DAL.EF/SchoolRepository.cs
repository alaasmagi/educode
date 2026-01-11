using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class SchoolRepository(AppDbContext context)
{
    public async Task<SchoolEntity?> GetSchoolById(Guid schoolId)
    {
        return await context.Schools
            .FirstOrDefaultAsync(ua => ua.Id == schoolId);
    }
    
    public async Task<List<SchoolEntity>> GetAllSchools(int pageNr, int pageSize)
    {
        return await context.Schools
            .OrderBy(c => c.Id)
            .Skip((pageNr - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<bool> AddSchoolEntityToDb(SchoolEntity newSchool)
    {
        newSchool.CreatedAt = DateTime.UtcNow;
        newSchool.UpdatedAt = DateTime.UtcNow;
        
        await context.Schools.AddAsync(newSchool);
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateSchoolEntity(Guid schoolId, SchoolEntity updatedSchool)
    {
        var existingSchool = await context.Schools.FindAsync(schoolId);
        if (existingSchool == null)
            return false;
        
        existingSchool.Name = updatedSchool.Name;
        existingSchool.ShortName = updatedSchool.ShortName;
        existingSchool.Domain = updatedSchool.Domain;
        existingSchool.PhotoPath = updatedSchool.PhotoPath;
        existingSchool.StudentCodePattern = updatedSchool.PhotoPath;
        existingSchool.UpdatedAt = DateTime.UtcNow;
        existingSchool.UpdatedBy = updatedSchool.UpdatedBy;

        context.Schools.Update(existingSchool);

        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteSchoolEntity(SchoolEntity school)
    {
        context.Schools.Remove(school);
        return await context.SaveChangesAsync() > 0;
    }
}