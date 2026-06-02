using System;
using Handmade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260602090000_HomeCategoryImages")]
    public partial class HomeCategoryImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "HomeCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeCategories_ImageId",
                table: "HomeCategories",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeCategories_ImageAssets_ImageId",
                table: "HomeCategories",
                column: "ImageId",
                principalTable: "ImageAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeCategories_ImageAssets_ImageId",
                table: "HomeCategories");

            migrationBuilder.DropIndex(
                name: "IX_HomeCategories_ImageId",
                table: "HomeCategories");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "HomeCategories");
        }
    }
}
