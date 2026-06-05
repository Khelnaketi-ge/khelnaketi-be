using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveManualSeoAndProductSlugUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductTranslations_LanguageCode_Slug",
                table: "ProductTranslations");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "ProductTranslations");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "ProductTranslations");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "CategoryTranslations");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "CategoryTranslations");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "Brands");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "ProductTranslations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "ProductTranslations",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "CategoryTranslations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "CategoryTranslations",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "Brands",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "Brands",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_LanguageCode_Slug",
                table: "ProductTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "\"Deleted\" = false");
        }
    }
}
