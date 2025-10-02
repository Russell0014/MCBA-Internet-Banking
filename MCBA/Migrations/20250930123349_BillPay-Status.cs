using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCBA.Migrations
{
    /// <inheritdoc />
    public partial class BillPayStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BillPays",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BillPays");
        }
    }
}
