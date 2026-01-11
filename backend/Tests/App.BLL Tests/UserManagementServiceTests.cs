using App.BLL;
using App.DAL.EF;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.BLL_Tests;

[TestFixture]
public class UserManagementServiceTests
{
    private AppDbContext _context = null!;
    private UserManagementService _service = null!;
    private ILogger<UserManagementService> _logger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new AppDbContext(options);
        _logger = new LoggerFactory().CreateLogger<UserManagementService>();
        _service = new UserManagementService(_context, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateAccountAsync_ShouldCreateUserAndAuth()
    {
        var user = new UserEntity 
        {  
            Id = 12, 
            UniId = "authTest" , 
            CreatedBy = "authTest", 
            UpdatedBy = "authTest", 
            FullName = "authTest", 
            UserTypeId = 0
        };
        var password = "testpassword123";
        var passwordHash = _service.GetPasswordHash(password);
        var userAuth = new UserAuthEntity { PasswordHash = passwordHash, CreatedBy = "authTest", UpdatedBy = "authTest" };

        var result = await _service.CreateAccountAsync(user, userAuth);

        Assert.That(result, Is.True);
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.UniId == "authTest");
        Assert.That(savedUser, Is.Not.Null);
    }

    [Test]
    public async Task AuthenticateUserAsync_WithCorrectPassword_ShouldReturnUser()
    {
        var password = "testpassword123";
        var hashed = _service.GetPasswordHash(password);

        var userType = new UserTypeEntity() { 
            Id = 1, 
            UserType = "authTest",
            CreatedBy = "authTest",
            UpdatedBy = "authTest"
        };
        
        await _context.UserTypes.AddAsync(userType);
        await _context.SaveChangesAsync();
        
        var user = new UserEntity { 
            Id = 12, 
            UniId = "authTest" ,
            CreatedBy = "authTest",
            UpdatedBy = "authTest", 
            FullName = "authTest", 
            UserTypeId = 1
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var userAuth = new UserAuthEntity
        {
            PasswordHash = hashed,
            UserId = user.Id,
            User = user,
            CreatedBy = "authTest",
            UpdatedBy = "authTest"
        };
        
        await _context.UserAuthData.AddAsync(userAuth);
        await _context.SaveChangesAsync();
        
        var authenticated = await _service.AuthenticateUserAsync(user.Id, password);      

        Assert.That(authenticated, Is.Not.Null);
        Assert.That(user.UniId == authenticated!.UniId);
    }

    [Test]
    public async Task DoesUserExistAsync_ShouldReturnTrue_IfUserExists()
    {
        var user = new UserEntity 
        { 
            UniId = "exist123", 
            FullName = "authTest", 
            CreatedBy = "authTest",
            UpdatedBy = "authTest", 
            UserTypeId = 1
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var exists = await _service.DoesUserExistAsync("exist123");
        
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task DoesUserExistAsync_ShouldReturnFalse_IfUserDoesNotExist()
    {
        var exists = await _service.DoesUserExistAsync("nonexistent");

        Assert.That(exists, Is.False);
    }
    
    [Test]
    public async Task ChangeUserPasswordAsync_ShouldChangePassword_WhenUserExists()
    {
        var user = new UserEntity 
        { 
            Id = 100, 
            UniId = "changepw", 
            FullName = "authTest", 
            CreatedBy = "authTest", 
            UpdatedBy = "authTest",
            UserTypeId = 1 
        };
        await _context.Users.AddAsync(user);
    
        var hash = _service.GetPasswordHash("oldPassword");
        var userAuth = new UserAuthEntity { UserId = user.Id, PasswordHash = hash, CreatedBy = "authTest", UpdatedBy = "authTest" };
        await _context.UserAuthData.AddAsync(userAuth);
        await _context.SaveChangesAsync();

        var newHash = _service.GetPasswordHash("newPassword");
        var result = await _service.ChangeUserPasswordAsync(user, newHash);

        Assert.That(result, Is.True);
        var updatedAuth = await _context.UserAuthData.FirstOrDefaultAsync(u => u.UserId == user.Id);
        Assert.That(updatedAuth!.PasswordHash, Is.EqualTo(newHash));
    }

    [Test]
    public async Task ChangeUserPasswordAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var fakeUser = new UserEntity
        {
            Id = 999, 
            UniId = "nope"
        };
        var result = await _service.ChangeUserPasswordAsync(fakeUser, _service.GetPasswordHash("irrelevant"));

        Assert.That(result, Is.False);
    }
    
    [Test]
    public async Task GetUserTypeAsync_ShouldReturnUserType_IfExists()
    {
        var type = new UserTypeEntity
        {
            UserType = "Tester",
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        await _context.UserTypes.AddAsync(type);
        await _context.SaveChangesAsync();

        var result = await _service.GetUserTypeAsync("Tester");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserType, Is.EqualTo("Tester"));
    }

    [Test]
    public async Task GetUserTypeAsync_ShouldReturnNull_IfNotExists()
    {
        var result = await _service.GetUserTypeAsync("GhostType");
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetAllUsersAsync_ShouldReturnUsers_IfAnyExist()
    {
        await _context.Users.AddAsync(new UserEntity { 
            UniId = "alltest",  
            FullName = "Test User",
            CreatedBy = "test", 
            UpdatedBy = "test", 
            UserTypeId = 1 });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllUsersAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldReturnNull_IfNoUsersExist()
    {
        var result = await _service.GetAllUsersAsync();
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetUserByUniIdAsync_ShouldReturnUser_IfExists()
    {
        var userType = new UserTypeEntity() { 
            Id = 1, 
            UserType = "authTest",
            CreatedBy = "authTest",
            UpdatedBy = "authTest"
        };
        
        await _context.UserTypes.AddAsync(userType);
        await _context.SaveChangesAsync();
        
        var user = new UserEntity 
        { 
            UniId = "uni123",
            FullName = "Test User", 
            CreatedBy = "test", 
            UpdatedBy = "test", 
            UserTypeId = 1 
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _service.GetUserByUniIdAsync("uni123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UniId, Is.EqualTo("uni123"));
    }

    [Test]
    public async Task GetUserByUniIdAsync_ShouldReturnNull_IfNotExists()
    {
        var result = await _service.GetUserByUniIdAsync("missing");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnUser_IfExists()
    {
        var user = new UserEntity 
        { 
            Id = 222, 
            UniId = "test", 
            FullName = "Test User",
            CreatedBy = "test", 
            UpdatedBy = "test", 
            UserTypeId = 1
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _service.GetUserByIdAsync(222);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UniId, Is.EqualTo("test"));
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnNull_IfNotExists()
    {
        var result = await _service.GetUserByIdAsync(9999);
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task DeleteUserAsync_ShouldDeleteUser_IfExists()
    {
        var user = new UserEntity 
        { 
            Id = 333, 
            UniId = "deleteMe",  
            FullName = "Test User",
            CreatedBy = "test", 
            UpdatedBy = "test", 
            UserTypeId = 1
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteUserAsync(user);

        Assert.That(result, Is.True);
        var deleted = await _context.Users.FindAsync(333);
        Assert.That(deleted, Is.Null);
    }
}