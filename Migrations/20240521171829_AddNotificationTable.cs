using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dorbit.Framework.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "frm");

            migrationBuilder.RenameTable(
                name: "Logs",
                schema: "log",
                newName: "Logs",
                newSchema: "frm");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                schema: "frm",
                table: "Logs",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                schema: "frm",
                table: "Logs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "frm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Body = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Image = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExpireTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserIds = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "frm");

            migrationBuilder.EnsureSchema(
                name: "log");

            migrationBuilder.RenameTable(
                name: "Logs",
                schema: "frm",
                newName: "Logs",
                newSchema: "log");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatorId",
                schema: "log",
                table: "Logs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                schema: "log",
                table: "Logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
