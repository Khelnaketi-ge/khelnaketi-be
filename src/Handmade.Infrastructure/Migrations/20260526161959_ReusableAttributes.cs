using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReusableAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeOptions_CategoryAttributes_CategoryAttributeId",
                table: "AttributeOptions");

            migrationBuilder.DropIndex(
                name: "IX_CategoryAttributes_CategoryId_NormalizedName",
                table: "CategoryAttributes");

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO "ProductAttributes" (
                    "Id",
                    "Name",
                    "NormalizedName",
                    "Type",
                    "Unit",
                    "Created",
                    "CreatedBy",
                    "Updated",
                    "UpdatedBy",
                    "Deleted")
                SELECT
                    "Id",
                    "Name",
                    "NormalizedName",
                    "Type",
                    "Unit",
                    "Created",
                    "CreatedBy",
                    "Updated",
                    "UpdatedBy",
                    "Deleted"
                FROM "CategoryAttributes";
                """);

            migrationBuilder.Sql("""
                SELECT setval(
                    pg_get_serial_sequence('"ProductAttributes"', 'Id'),
                    GREATEST(COALESCE((SELECT MAX("Id") FROM "ProductAttributes"), 1), 1),
                    true);
                """);

            migrationBuilder.RenameColumn(
                name: "CategoryAttributeId",
                table: "AttributeOptions",
                newName: "ProductAttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeOptions_CategoryAttributeId_NormalizedValue",
                table: "AttributeOptions",
                newName: "IX_AttributeOptions_ProductAttributeId_NormalizedValue");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeOptions_CategoryAttributeId",
                table: "AttributeOptions",
                newName: "IX_AttributeOptions_ProductAttributeId");

            migrationBuilder.AddColumn<int>(
                name: "ProductAttributeId",
                table: "CategoryAttributes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "CategoryAttributes"
                SET "ProductAttributeId" = "Id";
                """);

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "CategoryAttributes");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributes_CategoryId_ProductAttributeId",
                table: "CategoryAttributes",
                columns: new[] { "CategoryId", "ProductAttributeId" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributes_ProductAttributeId",
                table: "CategoryAttributes",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Deleted",
                table: "ProductAttributes",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_NormalizedName",
                table: "ProductAttributes",
                column: "NormalizedName",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Type",
                table: "ProductAttributes",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeOptions_ProductAttributes_ProductAttributeId",
                table: "AttributeOptions",
                column: "ProductAttributeId",
                principalTable: "ProductAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryAttributes_ProductAttributes_ProductAttributeId",
                table: "CategoryAttributes",
                column: "ProductAttributeId",
                principalTable: "ProductAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeOptions_ProductAttributes_ProductAttributeId",
                table: "AttributeOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryAttributes_ProductAttributes_ProductAttributeId",
                table: "CategoryAttributes");

            migrationBuilder.DropIndex(
                name: "IX_CategoryAttributes_CategoryId_ProductAttributeId",
                table: "CategoryAttributes");

            migrationBuilder.DropIndex(
                name: "IX_CategoryAttributes_ProductAttributeId",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "ProductAttributeId",
                table: "CategoryAttributes");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CategoryAttributes",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "CategoryAttributes",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "Type",
                table: "CategoryAttributes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "CategoryAttributes",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "CategoryAttributes" AS ca
                SET
                    "Name" = pa."Name",
                    "NormalizedName" = pa."NormalizedName",
                    "Type" = pa."Type",
                    "Unit" = pa."Unit"
                FROM "ProductAttributes" AS pa
                WHERE ca."Id" = pa."Id";
                """);

            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.RenameColumn(
                name: "ProductAttributeId",
                table: "AttributeOptions",
                newName: "CategoryAttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeOptions_ProductAttributeId_NormalizedValue",
                table: "AttributeOptions",
                newName: "IX_AttributeOptions_CategoryAttributeId_NormalizedValue");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeOptions_ProductAttributeId",
                table: "AttributeOptions",
                newName: "IX_AttributeOptions_CategoryAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributes_CategoryId_NormalizedName",
                table: "CategoryAttributes",
                columns: new[] { "CategoryId", "NormalizedName" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeOptions_CategoryAttributes_CategoryAttributeId",
                table: "AttributeOptions",
                column: "CategoryAttributeId",
                principalTable: "CategoryAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
