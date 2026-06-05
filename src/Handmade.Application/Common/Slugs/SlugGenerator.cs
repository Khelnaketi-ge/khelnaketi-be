using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Common.Slugs;

public static class SlugGenerator
{
    private static readonly IReadOnlyDictionary<char, string> TransliterationMap = new Dictionary<char, string>
    {
        ['ა'] = "a", ['ბ'] = "b", ['გ'] = "g", ['დ'] = "d", ['ე'] = "e", ['ვ'] = "v", ['ზ'] = "z",
        ['თ'] = "t", ['ი'] = "i", ['კ'] = "k", ['ლ'] = "l", ['მ'] = "m", ['ნ'] = "n", ['ო'] = "o",
        ['პ'] = "p", ['ჟ'] = "zh", ['რ'] = "r", ['ს'] = "s", ['ტ'] = "t", ['უ'] = "u", ['ფ'] = "p",
        ['ქ'] = "k", ['ღ'] = "gh", ['ყ'] = "q", ['შ'] = "sh", ['ჩ'] = "ch", ['ც'] = "ts", ['ძ'] = "dz",
        ['წ'] = "ts", ['ჭ'] = "ch", ['ხ'] = "kh", ['ჯ'] = "j", ['ჰ'] = "h",
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ё'] = "e",
        ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m",
        ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
        ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "shch",
        ['ъ'] = "", ['ы'] = "y", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya"
    };

    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "item";
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            if (TransliterationMap.TryGetValue(character, out var replacement))
            {
                if (replacement.Length > 0)
                {
                    builder.Append(replacement);
                    previousWasSeparator = false;
                }

                continue;
            }

            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (!previousWasSeparator)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        var slug = builder.ToString().Trim('-').Normalize(NormalizationForm.FormC);
        return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
    }

    public static string GenerateForEntity(string value, string ownershipSuffix, int entityId, int maxLength)
    {
        var suffix = $"-{ownershipSuffix}{entityId}";
        return $"{Truncate(Generate(value), maxLength - suffix.Length)}{suffix}";
    }

    public static string WithSuffix(string slug, int suffix)
    {
        return suffix <= 1 ? slug : $"{slug}-{suffix}";
    }

    public static async Task<string> GenerateUniqueAsync(
        IQueryable<string> existingSlugs,
        string value,
        int maxLength,
        CancellationToken cancellationToken)
    {
        var baseSlug = Truncate(Generate(value), maxLength);
        var conflictingSlugs = await existingSlugs
            .Where(x => x == baseSlug || EF.Functions.Like(x, $"{baseSlug}-%"))
            .ToListAsync(cancellationToken);
        var usedSlugs = conflictingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var suffix = 1;
        var slug = baseSlug;

        while (usedSlugs.Contains(slug))
        {
            suffix++;
            var suffixText = $"-{suffix}";
            slug = $"{Truncate(baseSlug, maxLength - suffixText.Length)}{suffixText}";
        }

        return slug;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (maxLength <= 0)
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength].Trim('-');
    }
}
