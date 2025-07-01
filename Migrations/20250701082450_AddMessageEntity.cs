using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dorbit.Framework.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "framework");

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "framework",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Receivers = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TemplateCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Args = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreationTime",
                schema: "framework",
                table: "Messages",
                column: "CreationTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages",
                schema: "framework");

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "framework",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpireTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Icon = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Image = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsArchive = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserIds = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreationTime",
                schema: "framework",
                table: "Notifications",
                column: "CreationTime");
        }
    }
}
