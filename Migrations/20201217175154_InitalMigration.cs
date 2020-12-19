using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CESystem.Migrations
{
    public partial class InitalMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    commission = table.Column<string>(type: "text", nullable: true),
                    absolute_commission_status = table.Column<bool>(type: "boolean", nullable: true),
                    upper_commission_limit = table.Column<double>(type: "double precision", nullable: true),
                    lower_commission_limit = table.Column<double>(type: "double precision", nullable: true),
                    confirm_commission_limit = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "client")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_date = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<string>(type: "text", nullable: true),
                    id_user = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                    table.ForeignKey(
                        name: "FK_id_user",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purse",
                columns: table => new
                {
                    id_account = table.Column<int>(type: "integer", nullable: false),
                    id_currency = table.Column<int>(type: "integer", nullable: false),
                    cash_value = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    commission = table.Column<double>(type: "double precision", nullable: true),
                    absolute_commission_status = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purse", x => new { x.id_account, x.id_currency });
                    table.ForeignKey(
                        name: "FK_purse_account_id_account",
                        column: x => x.id_account,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purse_currency_id_currency",
                        column: x => x.id_currency,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_id_user",
                table: "account",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "IX_currency_name",
                table: "currency",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purse_id_currency",
                table: "purse",
                column: "id_currency");

            migrationBuilder.CreateIndex(
                name: "IX_users_name",
                table: "users",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purse");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
