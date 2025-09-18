using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCBA.Migrations
{
    /// <inheritdoc />
    public partial class dbupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Login_CustomerId",
                table: "Login");

            migrationBuilder.CreateIndex(
                name: "IX_Login_CustomerId",
                table: "Login",
                column: "CustomerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Login_CustomerId",
                table: "Login");

            migrationBuilder.CreateIndex(
                name: "IX_Login_CustomerId",
                table: "Login",
                column: "CustomerId");
        }
    }
}
