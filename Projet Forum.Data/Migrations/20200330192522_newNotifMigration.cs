using Microsoft.EntityFrameworkCore.Migrations;

namespace Projet_Forum.Data.Migrations
{
    public partial class newNotifMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Context",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContextId",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Context",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ContextId",
                table: "Notifications");
        }
    }
}
