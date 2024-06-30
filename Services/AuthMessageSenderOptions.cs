namespace WebPWrecover.Services;

public class AuthMessageSenderOptions
{
    public string? SendGridKey { get; set; }
    public string? SendGridEmail {get; set; }
    public string? AppOwnerEmail { get; set; }
}