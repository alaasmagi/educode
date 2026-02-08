using App.Contracts.WebRequests;

namespace App.Web.Clients;

public class EmailClient(HttpClient httpClient)
{
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var request = new EmailApiRequest();

        var response = await httpClient.PostAsJsonAsync(
            "/emails/send",
            request,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();
    }
}