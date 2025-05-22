using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dorbit.Framework.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                schema: "framework",
                table: "Logs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreationTime",
                schema: "framework",
                table: "Notifications",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_CreationTime",
                schema: "framework",
                table: "Logs",
                column: "CreationTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreationTime",
                schema: "framework",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Logs_CreationTime",
                schema: "framework",
                table: "Logs");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                schema: "framework",
                table: "Logs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
