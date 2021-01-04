using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CESystem.Migrations
{
    public partial class InitalMigrations : Migration
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
                    upper_commission_limit = table.Column<float>(type: "real", nullable: true),
                    lower_commission_limit = table.Column<float>(type: "real", nullable: true),
                    confirm_limit = table.Column<float>(type: "real", nullable: true)
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
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "client"),
                    current_account = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<string>(type: "text", nullable: false)
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
                    id_user = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                    table.ForeignKey(
                        name: "FK_id_user_account",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commission",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_user = table.Column<int>(type: "integer", nullable: true),
                    id_currency = table.Column<int>(type: "integer", nullable: true),
                    transfer = table.Column<float>(type: "real", nullable: true),
                    withdraw = table.Column<float>(type: "real", nullable: true),
                    deposit = table.Column<float>(type: "real", nullable: true),
                    is_absolute_type = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commission", x => x.id);
                    table.CheckConstraint("correct_commission", "'id_user' != null or 'id_currency' != null");
                    table.ForeignKey(
                        name: "FK_id_currency_commission",
                        column: x => x.id_currency,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_id_user_commission",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "operations_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_user = table.Column<int>(type: "integer", nullable: false),
                    id_account = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<float>(type: "real", nullable: false),
                    commission = table.Column<float>(type: "real", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_id_user_history",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "confirm_request",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_sender = table.Column<int>(type: "integer", nullable: false),
                    id_recipient = table.Column<int>(type: "integer", nullable: true),
                    operation_type = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<float>(type: "real", nullable: false),
                    commission = table.Column<float>(type: "real", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    formation_date = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_confirm_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_id_account_confirm_request",
                        column: x => x.id_sender,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                columns: table => new
                {
                    id_account = table.Column<int>(type: "integer", nullable: false),
                    id_currency = table.Column<int>(type: "integer", nullable: false),
                    cash_value = table.Column<float>(type: "real", nullable: false, defaultValue: 0f)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet", x => new { x.id_account, x.id_currency });
                    table.ForeignKey(
                        name: "FK_wallet_account_id_account",
                        column: x => x.id_account,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wallet_currency_id_currency",
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
                name: "IX_commission_id_currency",
                table: "commission",
                column: "id_currency",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commission_id_user",
                table: "commission",
                column: "id_user",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_confirm_request_id_sender",
                table: "confirm_request",
                column: "id_sender");

            migrationBuilder.CreateIndex(
                name: "IX_currency_name",
                table: "currency",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operations_history_id_user",
                table: "operations_history",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "IX_users_name",
                table: "users",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_id_currency",
                table: "wallet",
                column: "id_currency");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commission");

            migrationBuilder.DropTable(
                name: "confirm_request");

            migrationBuilder.DropTable(
                name: "operations_history");

            migrationBuilder.DropTable(
                name: "wallet");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
