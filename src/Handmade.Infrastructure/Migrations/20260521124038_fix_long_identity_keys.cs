using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handmade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix_long_identity_keys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "VerificationCodes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                SELECT setval(
                    pg_get_serial_sequence('"VerificationCodes"', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM "VerificationCodes"), 0) + 1,
                    false);
                """);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "UserExternalLogins",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                SELECT setval(
                    pg_get_serial_sequence('"UserExternalLogins"', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM "UserExternalLogins"), 0) + 1,
                    false);
                """);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "RefreshTokens",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                SELECT setval(
                    pg_get_serial_sequence('"RefreshTokens"', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM "RefreshTokens"), 0) + 1,
                    false);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "VerificationCodes",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "UserExternalLogins",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "RefreshTokens",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
