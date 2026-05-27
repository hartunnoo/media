namespace Media.Domain.Entities;

public class ApiClient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AppName { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string HashedSecret { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
    public string? AllowedMediaTypes { get; set; }
    public int RateLimit { get; set; } = 1000;
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
