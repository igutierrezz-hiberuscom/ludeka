namespace Ludeka.Application.Options;

public class InstagramOptions
{
    public const string SectionName = "Instagram";

    public bool Simulate { get; set; } = true;
    public string? AccessToken { get; set; }
    public string? InstagramAccountId { get; set; }
    public string BaseUrl { get; set; } = "https://graph.facebook.com/v19.0/";
    public string PublicBaseUrl { get; set; } = "https://ludeka.es";

    public bool IsConfigured => !Simulate && !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(InstagramAccountId);
}
