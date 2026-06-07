using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategorySlugsWithoutOwnershipSuffix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH active_category_slugs AS (
                    SELECT
                        "Id",
                        regexp_replace("Slug", '-c[0-9]+$', '') AS "BaseSlug",
                        row_number() OVER (
                            PARTITION BY "LanguageCode", regexp_replace("Slug", '-c[0-9]+$', '')
                            ORDER BY "CategoryId", "Id"
                        ) AS "Suffix"
                    FROM "CategoryTranslations"
                    WHERE "Deleted" = false
                )
                UPDATE "CategoryTranslations" AS category_translation
                SET "Slug" = CASE
                    WHEN active_category_slugs."Suffix" = 1
                        THEN left(active_category_slugs."BaseSlug", 200)
                    ELSE left(
                        active_category_slugs."BaseSlug",
                        200 - length('-' || active_category_slugs."Suffix")
                    ) || '-' || active_category_slugs."Suffix"
                END
                FROM active_category_slugs
                WHERE category_translation."Id" = active_category_slugs."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "CategoryTranslations"
                SET "Slug" = left("Slug", 200 - length('-c' || "CategoryId")) || '-c' || "CategoryId"
                WHERE "Deleted" = false;
                """);
        }
    }
}
