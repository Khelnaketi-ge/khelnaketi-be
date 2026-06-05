using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocalizedSlugOwnershipSuffixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_NormalizedName",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductAttributes_NormalizedName",
                table: "ProductAttributes");

            migrationBuilder.DropIndex(
                name: "IX_Categories_NormalizedName",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentId_NormalizedName",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_AttributeOptions_ProductAttributeId_NormalizedValue",
                table: "AttributeOptions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NormalizedValue",
                table: "AttributeOptions");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "AttributeOptions");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "ProductAttributeTranslations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "AttributeOptionTranslations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION handmade_slugify(input text)
                RETURNS text
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    value text := lower(coalesce(input, ''));
                BEGIN
                    value := replace(value, 'შჩ', 'shch');
                    value := replace(value, 'ა', 'a'); value := replace(value, 'ბ', 'b'); value := replace(value, 'გ', 'g');
                    value := replace(value, 'დ', 'd'); value := replace(value, 'ე', 'e'); value := replace(value, 'ვ', 'v');
                    value := replace(value, 'ზ', 'z'); value := replace(value, 'თ', 't'); value := replace(value, 'ი', 'i');
                    value := replace(value, 'კ', 'k'); value := replace(value, 'ლ', 'l'); value := replace(value, 'მ', 'm');
                    value := replace(value, 'ნ', 'n'); value := replace(value, 'ო', 'o'); value := replace(value, 'პ', 'p');
                    value := replace(value, 'ჟ', 'zh'); value := replace(value, 'რ', 'r'); value := replace(value, 'ს', 's');
                    value := replace(value, 'ტ', 't'); value := replace(value, 'უ', 'u'); value := replace(value, 'ფ', 'p');
                    value := replace(value, 'ქ', 'k'); value := replace(value, 'ღ', 'gh'); value := replace(value, 'ყ', 'q');
                    value := replace(value, 'შ', 'sh'); value := replace(value, 'ჩ', 'ch'); value := replace(value, 'ც', 'ts');
                    value := replace(value, 'ძ', 'dz'); value := replace(value, 'წ', 'ts'); value := replace(value, 'ჭ', 'ch');
                    value := replace(value, 'ხ', 'kh'); value := replace(value, 'ჯ', 'j'); value := replace(value, 'ჰ', 'h');
                    value := replace(value, 'щ', 'shch'); value := replace(value, 'ж', 'zh'); value := replace(value, 'х', 'kh');
                    value := replace(value, 'ц', 'ts'); value := replace(value, 'ч', 'ch'); value := replace(value, 'ш', 'sh');
                    value := replace(value, 'ю', 'yu'); value := replace(value, 'я', 'ya'); value := replace(value, 'ё', 'e');
                    value := replace(value, 'а', 'a'); value := replace(value, 'б', 'b'); value := replace(value, 'в', 'v');
                    value := replace(value, 'г', 'g'); value := replace(value, 'д', 'd'); value := replace(value, 'е', 'e');
                    value := replace(value, 'з', 'z'); value := replace(value, 'и', 'i'); value := replace(value, 'й', 'y');
                    value := replace(value, 'к', 'k'); value := replace(value, 'л', 'l'); value := replace(value, 'м', 'm');
                    value := replace(value, 'н', 'n'); value := replace(value, 'о', 'o'); value := replace(value, 'п', 'p');
                    value := replace(value, 'р', 'r'); value := replace(value, 'с', 's'); value := replace(value, 'т', 't');
                    value := replace(value, 'у', 'u'); value := replace(value, 'ф', 'f'); value := replace(value, 'ъ', '');
                    value := replace(value, 'ы', 'y'); value := replace(value, 'ь', ''); value := replace(value, 'э', 'e');
                    value := regexp_replace(value, '[^a-z0-9]+', '-', 'g');
                    value := regexp_replace(value, '-+', '-', 'g');
                    value := trim(both '-' from value);
                    RETURN coalesce(nullif(value, ''), 'item');
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "CategoryTranslations"
                SET "Slug" = left(handmade_slugify("Name"), 200 - length('-c' || "CategoryId")) || '-c' || "CategoryId"
                WHERE "Deleted" = false;

                UPDATE "ProductTranslations"
                SET "Slug" = left(handmade_slugify("Title"), 220 - length('-p' || "ProductId")) || '-p' || "ProductId"
                WHERE "Deleted" = false;

                UPDATE "ProductAttributeTranslations"
                SET "Slug" = left(handmade_slugify("Name"), 200 - length('-a' || "ProductAttributeId")) || '-a' || "ProductAttributeId"
                WHERE "Deleted" = false;

                UPDATE "AttributeOptionTranslations"
                SET "Slug" = left(handmade_slugify("Value"), 200 - length('-ao' || "AttributeOptionId")) || '-ao' || "AttributeOptionId"
                WHERE "Deleted" = false;

                DROP FUNCTION handmade_slugify(text);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_LanguageCode_Slug",
                table: "ProductTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeTranslations_LanguageCode_Slug",
                table: "ProductAttributeTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptionTranslations_LanguageCode_Slug",
                table: "AttributeOptionTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "\"Deleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductTranslations_LanguageCode_Slug",
                table: "ProductTranslations");

            migrationBuilder.DropIndex(
                name: "IX_ProductAttributeTranslations_LanguageCode_Slug",
                table: "ProductAttributeTranslations");

            migrationBuilder.DropIndex(
                name: "IX_AttributeOptionTranslations_LanguageCode_Slug",
                table: "AttributeOptionTranslations");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "ProductAttributeTranslations");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "AttributeOptionTranslations");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Products",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProductAttributes",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "ProductAttributes",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Categories",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedValue",
                table: "AttributeOptions",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "AttributeOptions",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Products_NormalizedName",
                table: "Products",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_NormalizedName",
                table: "ProductAttributes",
                column: "NormalizedName",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NormalizedName",
                table: "Categories",
                column: "NormalizedName",
                unique: true,
                filter: "\"Deleted\" = false AND \"ParentId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId_NormalizedName",
                table: "Categories",
                columns: new[] { "ParentId", "NormalizedName" },
                unique: true,
                filter: "\"Deleted\" = false AND \"ParentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptions_ProductAttributeId_NormalizedValue",
                table: "AttributeOptions",
                columns: new[] { "ProductAttributeId", "NormalizedValue" },
                unique: true,
                filter: "\"Deleted\" = false");
        }
    }
}
