using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dorbit.Framework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "framework");

            migrationBuilder.CreateTable(
                name: "Logs",
                schema: "framework",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<byte>(type: "smallint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "framework",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Body = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Image = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExpireTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchive = table.Column<bool>(type: "boolean", nullable: false),
                    UserIds = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "framework",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "character varying(10240)", maxLength: 10240, nullable: true),
                    Access = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_EntityType",
                schema: "framework",
                table: "Logs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_EntityType_ReferenceId",
                schema: "framework",
                table: "Logs",
                columns: new[] { "EntityType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Key",
                schema: "framework",
                table: "Settings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs",
                schema: "framework");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "framework");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "framework");
        }
    }
}
