using Microsoft.EntityFrameworkCore.Migrations;

namespace CESystem.Migrations
{
    public partial class ChangeMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "account");

            migrationBuilder.AlterColumn<double>(
                name: "commission",
                table: "currency",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "commission",
                table: "currency",
                type: "text",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified",
                table: "account",
                type: "text",
                nullable: true);
        }
    }
}
