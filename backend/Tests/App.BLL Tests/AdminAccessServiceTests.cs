using System.Text;
using App.BLL;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace App.BLL_Tests;

public class Tests
{
    [TestFixture]
    public class AdminAccessServiceTests
    {
        private ILogger<AdminAccessService> _logger = null!;
        private AdminAccessService _adminAccessService = null!;
        private string _storedUserNameBackup = null!;
        private string _storedPasswordBackup = null!;
        
        [SetUp]
        public void Setup()
        {
            _logger = Substitute.For<ILogger<AdminAccessService>>();
            _adminAccessService = new AdminAccessService(_logger);
            _storedUserNameBackup = Environment.GetEnvironmentVariable("ADMINUSER") ?? "";
            _storedPasswordBackup = Environment.GetEnvironmentVariable("ADMINKEY") ?? "";
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("ADMINUSER", _storedUserNameBackup);
            Environment.SetEnvironmentVariable("ADMINKEY", _storedPasswordBackup);
        }

        [Test]
        public void AdminAccessGrant_ValidCredentials_ReturnsTrue()
        {
            var storedUsername = "testuser";
            var storedPassword = "testpassword";
            var enteredUsername = "testuser";
            var enteredPassword = "testpassword";

            string hashedStoredUsername = BCrypt.Net.BCrypt.HashPassword(storedUsername);
            string hashedStoredPassword = BCrypt.Net.BCrypt.HashPassword(storedPassword);

            Environment.SetEnvironmentVariable("ADMINUSER", Convert.ToBase64String(Encoding.UTF8.GetBytes(hashedStoredUsername)));
            Environment.SetEnvironmentVariable("ADMINKEY", Convert.ToBase64String(Encoding.UTF8.GetBytes(hashedStoredPassword)));

            bool result = _adminAccessService.AdminAccessGrant(enteredUsername, enteredPassword);

            Assert.That(result, Is.True);
        }

        [Test]
        public void AdminAccessGrant_InvalidCredentials_ReturnsFalse()
        {
            string storedUsername = "testuser";
            string storedPassword = "testpassword";
            string enteredUsername = "wronguser";
            string enteredPassword = "wrongpassword";
            
            string hashedStoredUsername = BCrypt.Net.BCrypt.HashPassword(storedUsername);
            string hashedStoredPassword = BCrypt.Net.BCrypt.HashPassword(storedPassword);

            Environment.SetEnvironmentVariable("ADMINUSER", Convert.ToBase64String(Encoding.UTF8.GetBytes(hashedStoredUsername)));
            Environment.SetEnvironmentVariable("ADMINKEY", Convert.ToBase64String(Encoding.UTF8.GetBytes(hashedStoredPassword)));
            
            bool result = _adminAccessService.AdminAccessGrant(enteredUsername, enteredPassword);
            
            Assert.That(result, Is.False);
        }

        [Test]
        public void AdminAccessGrant_EnvironmentVariablesNotSet_ReturnsFalse()
        {
            Environment.SetEnvironmentVariable("ADMINUSER", null);
            Environment.SetEnvironmentVariable("ADMINKEY", null);

            bool result = _adminAccessService.AdminAccessGrant("testuser", "testpassword");

            Assert.That(result, Is.False);
        }

        [Test]
        public void GenerateAdminAccessToken_ReturnsValidToken()
        {
            string token = _adminAccessService.GenerateAdminAccessToken();

            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);
            Assert.That(64 == token.Length); 
        }

        [Test]
        public async Task GetHashedAdminAccessTokenAsync_ValidInput_ReturnsHashedToken()
        {
            string input = "testinput";
            string salt = "testsalt";
            Environment.SetEnvironmentVariable("ADMINTOKENSALT", salt);

            string hashedToken = await _adminAccessService.GetHashedAdminAccessTokenAsync(input);

            Assert.That(hashedToken, Is.Not.Null);
            Assert.That(hashedToken, Is.Not.Empty);
            Assert.That(64 == hashedToken.Length); 
        }

        [Test]
        public async Task GetHashedAdminAccessTokenAsync_SaltNotSet_ReturnsEmptyString()
        {
            Environment.SetEnvironmentVariable("ADMINTOKENSALT", null);

            string hashedToken = await _adminAccessService.GetHashedAdminAccessTokenAsync("testinput");

            Assert.That(hashedToken, Is.Empty);
           
        }

        [Test]
        public async Task CompareHashedTokensAsync_MatchingTokens_ReturnsTrue()
        {
            string inputToken = "testinput";
            string salt = "testsalt";
            Environment.SetEnvironmentVariable("ADMINTOKENSALT", salt);
            string hashedInputToken = await _adminAccessService.GetHashedAdminAccessTokenAsync(inputToken);

            bool result = await _adminAccessService.CompareHashedTokensAsync(inputToken, hashedInputToken);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CompareHashedTokensAsync_NonMatchingTokens_ReturnsFalse()
        {
            string inputToken = "testinput";
            string salt = "testsalt";
            Environment.SetEnvironmentVariable("ADMINTOKENSALT", salt);
            string hashedInputToken = "wronghashedtoken";

            bool result = await _adminAccessService.CompareHashedTokensAsync(inputToken, hashedInputToken);

            Assert.That(result, Is.False);
        }
    }
}