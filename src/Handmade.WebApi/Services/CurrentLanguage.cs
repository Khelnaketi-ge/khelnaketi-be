using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Microsoft.Net.Http.Headers;

namespace Handmade.WebApi.Services;

public class CurrentLanguage(IHttpContextAccessor httpContextAccessor) : ICurrentLanguage
{
    public string Code => ResolveLanguageCode();

    private string ResolveLanguageCode()
    {
        var acceptLanguage = httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();

        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return LanguageCodes.Default;
        }

        var requestedLanguages = acceptLanguage
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => StringWithQualityHeaderValue.TryParse(value, out var header) ? header : null)
            .Where(header => header is not null)
            .OrderByDescending(header => header!.Quality ?? 1)
            .Select(header => GetBaseLanguageCode(header!.Value.ToString()));

        foreach (var requestedLanguage in requestedLanguages)
        {
            if (LanguageCodes.IsSupported(requestedLanguage))
            {
                return LanguageCodes.Normalize(requestedLanguage);
            }
        }

        return LanguageCodes.Default;
    }

    private static string GetBaseLanguageCode(string languageCode)
    {
        var separatorIndex = languageCode.IndexOf('-');
        return separatorIndex < 0 ? languageCode : languageCode[..separatorIndex];
    }
}
