using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedSeoTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Brands",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Brands",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "Brands",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "Brands",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Brands"
                SET
                    "Slug" = left(coalesce(nullif(trim(both '-' from regexp_replace(lower("Name"), '[^[:alnum:]]+', '-', 'g')), ''), 'brand'), 200) || '-' || "Id",
                    "SeoTitle" = "Name"
                WHERE "Slug" = '';
                """);

            migrationBuilder.CreateTable(
                name: "CategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeoTitle = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryTranslations_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductAttributeId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAttributeTranslations_ProductAttributes_ProductAttri~",
                        column: x => x.ProductAttributeId,
                        principalTable: "ProductAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeOptionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttributeOptionId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Value = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeOptionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeOptionTranslations_AttributeOptions_AttributeOptio~",
                        column: x => x.AttributeOptionId,
                        principalTable: "AttributeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SeoTitle = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTranslations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "CategoryTranslations" ("CategoryId", "LanguageCode", "Name", "Description", "Slug", "SeoTitle", "SeoDescription", "Created", "CreatedBy", "Updated", "UpdatedBy", "Deleted")
                SELECT
                    "Id",
                    'ka',
                    "Name",
                    "Description",
                    left(coalesce(nullif(trim(both '-' from regexp_replace(lower("Name"), '[^[:alnum:]]+', '-', 'g')), ''), 'category'), 180) || '-' || "Id",
                    "Name",
                    "Description",
                    "Created",
                    "CreatedBy",
                    "Updated",
                    "UpdatedBy",
                    "Deleted"
                FROM "Categories";

                INSERT INTO "ProductTranslations" ("ProductId", "LanguageCode", "Title", "Slug", "ShortDescription", "Description", "SeoTitle", "SeoDescription", "Created", "CreatedBy", "Updated", "UpdatedBy", "Deleted")
                SELECT
                    "Id",
                    'ka',
                    "Name",
                    left(coalesce(nullif(trim(both '-' from regexp_replace(lower("Name"), '[^[:alnum:]]+', '-', 'g')), ''), 'product'), 200) || '-' || "Id",
                    null,
                    "Description",
                    "Name",
                    null,
                    "Created",
                    "CreatedBy",
                    "Updated",
                    "UpdatedBy",
                    "Deleted"
                FROM "Products";

                INSERT INTO "ProductAttributeTranslations" ("ProductAttributeId", "LanguageCode", "Name", "Created", "CreatedBy", "Updated", "UpdatedBy", "Deleted")
                SELECT
                    "Id",
                    'ka',
                    "Name",
                    "Created",
                    "CreatedBy",
                    "Updated",
                    "UpdatedBy",
                    "Deleted"
                FROM "ProductAttributes";

                INSERT INTO "AttributeOptionTranslations" ("AttributeOptionId", "LanguageCode", "Value", "Created", "CreatedBy", "Updated", "UpdatedBy", "Deleted")
                SELECT
                    "Id",
                    'ka',
                    "Value",
                    "Created",
                    "CreatedBy",
                    "Updated",
                    "UpdatedBy",
                    "Deleted"
                FROM "AttributeOptions";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Slug",
                table: "Brands",
                column: "Slug",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptionTranslations_AttributeOptionId_LanguageCode",
                table: "AttributeOptionTranslations",
                columns: new[] { "AttributeOptionId", "LanguageCode" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptionTranslations_Deleted",
                table: "AttributeOptionTranslations",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_CategoryId_LanguageCode",
                table: "CategoryTranslations",
                columns: new[] { "CategoryId", "LanguageCode" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_Deleted",
                table: "CategoryTranslations",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_LanguageCode_Slug",
                table: "CategoryTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeTranslations_Deleted",
                table: "ProductAttributeTranslations",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeTranslations_ProductAttributeId_LanguageCode",
                table: "ProductAttributeTranslations",
                columns: new[] { "ProductAttributeId", "LanguageCode" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_Deleted",
                table: "ProductTranslations",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_LanguageCode_Slug",
                table: "ProductTranslations",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_ProductId_LanguageCode",
                table: "ProductTranslations",
                columns: new[] { "ProductId", "LanguageCode" },
                unique: true,
                filter: "\"Deleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AttributeOptionTranslations");
            migrationBuilder.DropTable(name: "CategoryTranslations");
            migrationBuilder.DropTable(name: "ProductAttributeTranslations");
            migrationBuilder.DropTable(name: "ProductTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Slug",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "Brands");
        }
    }
}
