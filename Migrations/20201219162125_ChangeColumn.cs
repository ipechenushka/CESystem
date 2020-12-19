using Microsoft.EntityFrameworkCore.Migrations;

namespace CESystem.Migrations
{
    public partial class ChangeColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "confirm_commission_limit",
                table: "currency",
                newName: "confirm_limit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "confirm_limit",
                table: "currency",
                newName: "confirm_commission_limit");
        }
    }
}
