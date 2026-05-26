using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductStockStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInQuantity",
                table: "Products",
                newName: "IsInStock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInStock",
                table: "Products",
                newName: "IsInQuantity");
        }
    }
}
