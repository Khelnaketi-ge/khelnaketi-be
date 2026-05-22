using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Brands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageAssets_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LogoImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerUserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_ImageAssets_LogoImageId",
                        column: x => x.LogoImageId,
                        principalTable: "ImageAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Brands_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BrandAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AddressLine2 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandAddresses_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BrandEmailAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandEmailAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandEmailAddresses_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BrandPhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedPhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandPhoneNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandPhoneNumbers_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BrandRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Permissions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandRoles", x => x.Id);
                    table.UniqueConstraint("AK_BrandRoles_BrandId_Id", x => new { x.BrandId, x.Id });
                    table.ForeignKey(
                        name: "FK_BrandRoles_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BrandInvitations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    InvitedByUserId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandInvitations_BrandRoles_BrandId_RoleId",
                        columns: x => new { x.BrandId, x.RoleId },
                        principalTable: "BrandRoles",
                        principalColumns: new[] { "BrandId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandInvitations_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrandInvitations_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BrandMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandMembers_BrandRoles_BrandId_RoleId",
                        columns: x => new { x.BrandId, x.RoleId },
                        principalTable: "BrandRoles",
                        principalColumns: new[] { "BrandId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandMembers_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrandMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandAddresses_BrandId",
                table: "BrandAddresses",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandAddresses_BrandId_IsPrimary",
                table: "BrandAddresses",
                columns: new[] { "BrandId", "IsPrimary" },
                unique: true,
                filter: "\"Deleted\" = false AND \"IsActive\" = true AND \"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_BrandAddresses_Deleted",
                table: "BrandAddresses",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandEmailAddresses_BrandId",
                table: "BrandEmailAddresses",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandEmailAddresses_BrandId_IsPrimary",
                table: "BrandEmailAddresses",
                columns: new[] { "BrandId", "IsPrimary" },
                unique: true,
                filter: "\"Deleted\" = false AND \"IsActive\" = true AND \"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_BrandEmailAddresses_BrandId_NormalizedEmail",
                table: "BrandEmailAddresses",
                columns: new[] { "BrandId", "NormalizedEmail" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandEmailAddresses_Deleted",
                table: "BrandEmailAddresses",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_BrandId",
                table: "BrandInvitations",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_BrandId_NormalizedEmail",
                table: "BrandInvitations",
                columns: new[] { "BrandId", "NormalizedEmail" },
                unique: true,
                filter: "\"Deleted\" = false AND \"AcceptedAt\" IS NULL AND \"RevokedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_BrandId_RoleId",
                table: "BrandInvitations",
                columns: new[] { "BrandId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_Deleted",
                table: "BrandInvitations",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_ExpiresAt",
                table: "BrandInvitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_InvitedByUserId",
                table: "BrandInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_RoleId",
                table: "BrandInvitations",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandInvitations_TokenHash",
                table: "BrandInvitations",
                column: "TokenHash",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandMembers_BrandId",
                table: "BrandMembers",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandMembers_BrandId_RoleId",
                table: "BrandMembers",
                columns: new[] { "BrandId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_BrandMembers_BrandId_UserId",
                table: "BrandMembers",
                columns: new[] { "BrandId", "UserId" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandMembers_Deleted",
                table: "BrandMembers",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandMembers_RoleId",
                table: "BrandMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandMembers_UserId",
                table: "BrandMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandPhoneNumbers_BrandId",
                table: "BrandPhoneNumbers",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandPhoneNumbers_BrandId_IsPrimary",
                table: "BrandPhoneNumbers",
                columns: new[] { "BrandId", "IsPrimary" },
                unique: true,
                filter: "\"Deleted\" = false AND \"IsActive\" = true AND \"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_BrandPhoneNumbers_BrandId_NormalizedPhoneNumber",
                table: "BrandPhoneNumbers",
                columns: new[] { "BrandId", "NormalizedPhoneNumber" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandPhoneNumbers_Deleted",
                table: "BrandPhoneNumbers",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandRoles_BrandId",
                table: "BrandRoles",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandRoles_BrandId_NormalizedName",
                table: "BrandRoles",
                columns: new[] { "BrandId", "NormalizedName" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BrandRoles_Deleted",
                table: "BrandRoles",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Deleted",
                table: "Brands",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_LogoImageId",
                table: "Brands",
                column: "LogoImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_NormalizedName",
                table: "Brands",
                column: "NormalizedName",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_OwnerUserId",
                table: "Brands",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Status",
                table: "Brands",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_BucketName_ObjectKey",
                table: "ImageAssets",
                columns: new[] { "BucketName", "ObjectKey" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_ContentType",
                table: "ImageAssets",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_Deleted",
                table: "ImageAssets",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_UploadedByUserId",
                table: "ImageAssets",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandAddresses");

            migrationBuilder.DropTable(
                name: "BrandEmailAddresses");

            migrationBuilder.DropTable(
                name: "BrandInvitations");

            migrationBuilder.DropTable(
                name: "BrandMembers");

            migrationBuilder.DropTable(
                name: "BrandPhoneNumbers");

            migrationBuilder.DropTable(
                name: "BrandRoles");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "ImageAssets");
        }
    }
}
