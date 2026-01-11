using App.BLL;

namespace App.BLL_Tests;

[TestFixture]
public class OtpServiceTests
{
    private OtpService _otpService = null!;

    [SetUp]
    public void SetUp()
    {
        _otpService = new OtpService();
    }

    [Test]
    public void GenerateTotp_ShouldReturnValidOtp_ForGivenUniId()
    {
        var uniId = "user123"; 

        var otp = _otpService.GenerateTotp(uniId);

        Assert.That(otp, Is.Not.Null.Or.Empty);
        Assert.That(otp.Length, Is.EqualTo(6));
    }

    [Test]
    public void VerifyTotp_ShouldReturnTrue_ForValidOtp()
    {
        var uniId = "user123"; 
        var otp = _otpService.GenerateTotp(uniId); 

        var isValid = _otpService.VerifyTotp(uniId, otp);

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void VerifyTotp_ShouldReturnFalse_ForInvalidOtp()
    {
        var uniId = "user123"; 
        var invalidOtp = "123456"; 

        var isValid = _otpService.VerifyTotp(uniId, invalidOtp);

        Assert.That(isValid, Is.False);
    }
    
    [Test]
    // This test runs for over 5 minutes to ensure that the OTP is invalid
    public void VerifyTotp_ShouldReturnFalse_ForExpiredOtp()
    {
        var uniId = "user123";
        var otp = _otpService.GenerateTotp(uniId); 

        System.Threading.Thread.Sleep(310000); 
        var isValid = _otpService.VerifyTotp(uniId, otp);

        Assert.That(isValid, Is.False); 
    }
}