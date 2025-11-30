using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelNet9_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RaceRadioBites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    RaceWeekendName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    ClipUrl = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    IsFeatured = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceRadioBites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TechDrops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    TeamName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RaceWeekendName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ImpactTags = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LinkUrl = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    IsFeatured = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechDrops", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceRadioBites");

            migrationBuilder.DropTable(
                name: "TechDrops");
        }
    }
}
