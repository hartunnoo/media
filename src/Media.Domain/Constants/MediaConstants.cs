namespace Media.Domain.Constants;

public static class MediaConstants
{
    public const int MaxFileNameLength = 500;
    public const int MaxContentTypeLength = 100;
    public const int MaxTagNameLength = 100;
    public const int MaxVersionRetention = 3;
    public const long DefaultMaxFileSize = 100 * 1024 * 1024; // 100 MB
    public const long AdminMaxFileSize = 1024 * 1024 * 1024; // 1 GB
    public const int DefaultRetentionDays = 30;
    public const int MaxRetries = 3;
    public const int QuarantineAutoRejectDays = 14;

    public static class ThumbnailSizes
    {
        public const int Tiny = 64;
        public const int Small = 128;
        public const int Medium = 256;
        public const int Large = 1024;
    }

    public static class AllowedContentTypes
    {
        public static readonly HashSet<string> Images = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml" };
        public static readonly HashSet<string> Documents = new(StringComparer.OrdinalIgnoreCase) { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml", "application/pdf", "text/plain", "video/mp4", "audio/mpeg" };
    }
}
