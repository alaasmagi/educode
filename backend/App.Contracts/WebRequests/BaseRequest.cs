namespace App.Contracts.WebRequests;

public abstract class BaseRequest
{
    public required string Client { get; set; }
}