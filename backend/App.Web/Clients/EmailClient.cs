using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Infrastructure.Initializers;

namespace App.Web.Clients;

public class EmailClient(EnvInitializer envInitializer) : IEmailService
{
    public async Task<bool> SendOtpAsync(OtpEmailApiRequest request)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-API-Key", envInitializer.EmailApiKey);
        var response = await httpClient.PostAsJsonAsync(
            envInitializer.EmailApiUrl,
            request
        );
        
        return response.IsSuccessStatusCode;
    }
}