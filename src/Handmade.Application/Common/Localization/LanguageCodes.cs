namespace Handmade.Application.Common.Localization;

public static class LanguageCodes
{
    public const string Georgian = "ka";
    public const string English = "en";
    public const string Russian = "ru";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>
    {
        Georgian,
        English,
        Russian
    };

    public static string Normalize(string languageCode) =>
        languageCode.Trim().ToLowerInvariant();

    public static bool IsSupported(string languageCode) =>
        Supported.Contains(Normalize(languageCode));
}
