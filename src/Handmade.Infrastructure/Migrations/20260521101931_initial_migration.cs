using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    NormalizedPhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PhoneNumberVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TokenVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PermissionVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AccessFailedCount = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AccessLevel = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserExternalLogins",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<short>(type: "smallint", nullable: false),
                    ProviderUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProviderEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ProviderDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExternalLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserExternalLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Purpose = table.Column<short>(type: "smallint", nullable: false),
                    Destination = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAttempts = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_UserSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Deleted",
                table: "RefreshTokens",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_RevokedAt",
                table: "RefreshTokens",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_SessionId",
                table: "RefreshTokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_Deleted",
                table: "UserExternalLogins",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_Provider_ProviderUserId",
                table: "UserExternalLogins",
                columns: new[] { "Provider", "ProviderUserId" },
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_UserId",
                table: "UserExternalLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccessLevel",
                table: "Users",
                column: "AccessLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Deleted",
                table: "Users",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsBlocked",
                table: "Users",
                column: "IsBlocked");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedPhoneNumber",
                table: "Users",
                column: "NormalizedPhoneNumber",
                unique: true,
                filter: "\"Deleted\" = false AND \"NormalizedPhoneNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_Deleted",
                table: "UserSessions",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_LastUsedAt",
                table: "UserSessions",
                column: "LastUsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RevokedAt",
                table: "UserSessions",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_Deleted",
                table: "VerificationCodes",
                column: "Deleted",
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_ExpiresAt",
                table: "VerificationCodes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_UserId_Purpose_UsedAt",
                table: "VerificationCodes",
                columns: new[] { "UserId", "Purpose", "UsedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserExternalLogins");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
