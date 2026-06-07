using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateSavedProductsToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Carts" ("UserId", "Status", "Created", "CreatedBy", "Deleted")
                SELECT
                    saved."UserId",
                    1,
                    MIN(saved."Created"),
                    MIN(saved."CreatedBy"),
                    false
                FROM "UserFavoriteProducts" saved
                WHERE saved."Deleted" = false
                    AND NOT EXISTS (
                        SELECT 1
                        FROM "Carts" cart
                        WHERE cart."UserId" = saved."UserId"
                            AND cart."Status" = 1
                            AND cart."Deleted" = false
                    )
                GROUP BY saved."UserId";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "CartItems" ("CartId", "ProductId", "Quantity", "Created", "CreatedBy", "Deleted")
                SELECT
                    cart."Id",
                    saved."ProductId",
                    1,
                    saved."Created",
                    saved."CreatedBy",
                    false
                FROM "UserFavoriteProducts" saved
                INNER JOIN "Carts" cart
                    ON cart."UserId" = saved."UserId"
                    AND cart."Status" = 1
                    AND cart."Deleted" = false
                WHERE saved."Deleted" = false
                    AND NOT EXISTS (
                        SELECT 1
                        FROM "CartItems" item
                        WHERE item."CartId" = cart."Id"
                            AND item."ProductId" = saved."ProductId"
                            AND item."Deleted" = false
                    );
                """);

            migrationBuilder.DropTable(
                name: "UserFavoriteProducts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoriteProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteProducts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "UserFavoriteProducts" (
                    "ProductId",
                    "UserId",
                    "Created",
                    "CreatedBy",
                    "Deleted",
                    "Updated",
                    "UpdatedBy")
                SELECT
                    item."ProductId",
                    cart."UserId",
                    item."Created",
                    item."CreatedBy",
                    false,
                    item."Updated",
                    item."UpdatedBy"
                FROM "CartItems" item
                INNER JOIN "Carts" cart
                    ON cart."Id" = item."CartId"
                    AND cart."Status" = 1
                    AND cart."Deleted" = false
                WHERE item."Deleted" = false
                    AND NOT EXISTS (
                        SELECT 1
                        FROM "UserFavoriteProducts" saved
                        WHERE saved."UserId" = cart."UserId"
                            AND saved."ProductId" = item."ProductId"
                            AND saved."Deleted" = false
                    );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteProducts_Deleted",
                table: "UserFavoriteProducts",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteProducts_ProductId",
                table: "UserFavoriteProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteProducts_UserId",
                table: "UserFavoriteProducts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteProducts_UserId_ProductId",
                table: "UserFavoriteProducts",
                columns: new[] { "UserId", "ProductId" },
                unique: true,
                filter: "\"Deleted\" = false");
        }
    }
}
