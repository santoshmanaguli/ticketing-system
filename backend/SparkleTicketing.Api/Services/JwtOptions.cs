namespace SparkleTicketing.Api.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SparkleTicketing";
    public string Audience { get; set; } = "SparkleTicketing.Client";
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 480;
}
