namespace ScottNeidig.Web.Utilities;

/// <summary>
/// The fixed set of service landing pages a category can feed. The pages themselves stay Razor
/// views with their own copy and routes; this is only the key each page looks a category up by,
/// so which category feeds which page is data rather than code.
/// </summary>
public static class ServicePages
{
    public const string NopCommerce = "nopcommerce";
    public const string DotNet = "dotnet";
    public const string SmallBusiness = "small-business";

    /// <summary>Key to display name, used for the admin dropdown.</summary>
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        [NopCommerce] = "nopCommerce development",
        [DotNet] = ".NET application development",
        [SmallBusiness] = "Small business websites",
    };

    public static string? DisplayName(string? key) =>
        key is not null && All.TryGetValue(key, out var name) ? name : null;
}
