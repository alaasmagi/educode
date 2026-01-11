using System.IdentityModel.Tokens.Jwt;
using App.BLL;
using App.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace App.BLL_Tests;

public class AuthServiceTests
{
    private ILogger<AuthService> _logger = null!;
    private AuthService _authService = null!;
    private string _storedJwtKeyBackup = null!;
    private string _storedJwtIssBackup = null!;
    private string _storedJwtAudBackup = null!;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<AuthService>>();
        _authService = new AuthService(_logger);
        _storedJwtKeyBackup = Environment.GetEnvironmentVariable("JWTKEY") ?? "";
        _storedJwtIssBackup = Environment.GetEnvironmentVariable("JWTISS") ?? "";
        _storedJwtAudBackup = Environment.GetEnvironmentVariable("JWTAUD") ?? "";
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("JWTKEY", _storedJwtKeyBackup);
        Environment.SetEnvironmentVariable("JWTISS", _storedJwtIssBackup);
        Environment.SetEnvironmentVariable("JWTAUD", _storedJwtAudBackup);
    }

    [Test]
    public void GenerateJwtToken_ValidEnvVars_ReturnsToken()
    {
        Environment.SetEnvironmentVariable("JWTKEY", "thisisaveryseasdasdasdasdasdasdcretkey123456");
        Environment.SetEnvironmentVariable("JWTISS", "TestIssuer");
        Environment.SetEnvironmentVariable("JWTAUD", "TestAudience");
        
        var user = new UserEntity
        {
            Id = 1,
            UniId = "U123",
            FullName = "John Doe",
            UserType = new UserTypeEntity { UserType = "Admin" }
        };

        var token = _authService.GenerateJwtToken(user);

        Assert.That(string.IsNullOrEmpty(token), Is.False);

        var handler = new JwtSecurityTokenHandler();
        var readToken = handler.ReadJwtToken(token);
        Assert.That("TestIssuer" == readToken.Issuer);
        Assert.That("TestAudience" == readToken.Audiences.First());

        var nameClaim = readToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;

        Assert.That("John Doe" == nameClaim);
    }

    [Test]
    public void GenerateJwtToken_MissingJWTKey_ReturnsEmptyStringAndLogsError()
    {
        Environment.SetEnvironmentVariable("JWTKEY", null);
        Environment.SetEnvironmentVariable("JWTISS", "TestIssuer");
        Environment.SetEnvironmentVariable("JWTAUD", "TestAudience");
        var user = new UserEntity
        {
            Id = 1,
            UniId = "U123",
            FullName = "John Doe",
            UserType = new UserTypeEntity { UserType = "Admin" }
        };

        var token = _authService.GenerateJwtToken(user);

        Assert.That(string.Empty == token);
    }

    [Test]
    public void GenerateJwtToken_MissingIssuerOrAudience_ReturnsEmptyStringAndLogsError()
    {
        Environment.SetEnvironmentVariable("JWTKEY", "thisisaveryseasdasdasdasdasdasdcretkey123456");
        Environment.SetEnvironmentVariable("JWTISS", null);  
        Environment.SetEnvironmentVariable("JWTAUD", null); 
        var user = new UserEntity
        {
            Id = 1,
            UniId = "U123",
            FullName = "John Doe",
            UserType = new UserTypeEntity { UserType = "Admin" }
        };

        var token = _authService.GenerateJwtToken(user);

        Assert.That(string.Empty ==  token);
    }
}