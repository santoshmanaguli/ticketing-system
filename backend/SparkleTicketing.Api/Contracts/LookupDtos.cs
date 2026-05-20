namespace SparkleTicketing.Api.Contracts;

public sealed record LookupDto(int Id, string Name);

public sealed record EnumLookupDto(string Value, string Label);

public sealed record UserLookupDto(int Id, string FullName, string Email, string Role);

public sealed record BranchLookupDto(int Id, string Name, string City);

public sealed record CustomerLookupDto(
    int Id,
    string CompanyName,
    string ContactPerson,
    string City,
    IReadOnlyList<BranchLookupDto> Branches);
