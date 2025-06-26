using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignalRDrawingApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrawingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrawingStrokes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrokeData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DrawingSessionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawingStrokes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawingStrokes_DrawingSessions_DrawingSessionId",
                        column: x => x.DrawingSessionId,
                        principalTable: "DrawingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DrawingSessions",
                columns: new[] { "Id", "BackgroundColor", "CreatedAt", "LastModifiedAt", "Name" },
                values: new object[] { 1, "#FFFFFF", new DateTime(2025, 6, 26, 18, 59, 22, 92, DateTimeKind.Utc).AddTicks(9062), null, "Default Session" });

            migrationBuilder.CreateIndex(
                name: "IX_DrawingStrokes_DrawingSessionId",
                table: "DrawingStrokes",
                column: "DrawingSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrawingStrokes");

            migrationBuilder.DropTable(
                name: "DrawingSessions");
        }
    }
}
