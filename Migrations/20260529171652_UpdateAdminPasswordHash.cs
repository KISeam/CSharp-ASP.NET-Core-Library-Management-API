using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix admin password hash for "Admin@123"
            // Generated with: BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12)
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$12$sKs27fLhLVCLZMOWfj4hAev3ydIDKb8R2E8FRGVCJp8PbXFI8tpM6"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore old (incorrect) hash if rolling back
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$12$v7tOlMPlMv0gxC8Y/pN6EuY6YhXmCmOeB0B1vYWxP.GkgKkSgGq9y"
            );
        }
    }
}
