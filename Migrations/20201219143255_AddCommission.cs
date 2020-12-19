using Microsoft.EntityFrameworkCore.Migrations;

namespace CESystem.Migrations
{
    public partial class AddCommission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "commission",
                table: "purse",
                newName: "withdraw_commission");

            migrationBuilder.RenameColumn(
                name: "commission",
                table: "currency",
                newName: "withdraw_commission");

            migrationBuilder.AddColumn<double>(
                name: "deposit_commission",
                table: "purse",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "transfer_commission",
                table: "purse",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "deposit_commission",
                table: "currency",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "transfer_commission",
                table: "currency",
                type: "double precision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deposit_commission",
                table: "purse");

            migrationBuilder.DropColumn(
                name: "transfer_commission",
                table: "purse");

            migrationBuilder.DropColumn(
                name: "deposit_commission",
                table: "currency");

            migrationBuilder.DropColumn(
                name: "transfer_commission",
                table: "currency");

            migrationBuilder.RenameColumn(
                name: "withdraw_commission",
                table: "purse",
                newName: "commission");

            migrationBuilder.RenameColumn(
                name: "withdraw_commission",
                table: "currency",
                newName: "commission");
        }
    }
}
