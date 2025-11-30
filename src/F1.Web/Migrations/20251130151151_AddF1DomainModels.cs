using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddF1DomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    AccentColor = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaceWeekends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CircuitName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    TrackMapUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    RaceDateUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Laps = table.Column<int>(type: "INTEGER", nullable: false),
                    DistanceKm = table.Column<double>(type: "REAL", nullable: false),
                    DRSZones = table.Column<int>(type: "INTEGER", nullable: false),
                    TyreCompounds = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    TrackTimeZone = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    WeatherSummary = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    AveragePitTimeSeconds = table.Column<double>(type: "REAL", nullable: false),
                    Fp1StartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Fp2StartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Fp3StartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    QualifyingStartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SprintStartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RaceStartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PostRaceSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceWeekends", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BaseCountry = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Championships = table.Column<int>(type: "INTEGER", nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    SecondaryColor = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    ShortHistory = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    BadgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => new { x.BadgeId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserBadges_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PostId = table.Column<int>(type: "INTEGER", nullable: true),
                    RaceWeekendId = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentCommentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_RaceWeekends_RaceWeekendId",
                        column: x => x.RaceWeekendId,
                        principalTable: "RaceWeekends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Nationality = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    DebutYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Championships = table.Column<int>(type: "INTEGER", nullable: false),
                    Podiums = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    HelmetImageUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Upgrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    RaceWeekendId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Impact = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Upgrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Upgrades_RaceWeekends_RaceWeekendId",
                        column: x => x.RaceWeekendId,
                        principalTable: "RaceWeekends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Upgrades_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RaceWeekendId = table.Column<int>(type: "INTEGER", nullable: false),
                    PredictedP1DriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    PredictedP2DriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    PredictedP3DriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    PredictedFastestLapDriverId = table.Column<int>(type: "INTEGER", nullable: true),
                    PredictedSafetyCar = table.Column<bool>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predictions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_PredictedFastestLapDriverId",
                        column: x => x.PredictedFastestLapDriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_PredictedP1DriverId",
                        column: x => x.PredictedP1DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_PredictedP2DriverId",
                        column: x => x.PredictedP2DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Drivers_PredictedP3DriverId",
                        column: x => x.PredictedP3DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_RaceWeekends_RaceWeekendId",
                        column: x => x.RaceWeekendId,
                        principalTable: "RaceWeekends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceWeekendId = table.Column<int>(type: "INTEGER", nullable: false),
                    WinnerDriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondDriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThirdDriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    FastestLapDriverId = table.Column<int>(type: "INTEGER", nullable: true),
                    SafetyCarDeployed = table.Column<bool>(type: "INTEGER", nullable: false),
                    VirtualSafetyCarDeployed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceResults_Drivers_FastestLapDriverId",
                        column: x => x.FastestLapDriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResults_Drivers_SecondDriverId",
                        column: x => x.SecondDriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResults_Drivers_ThirdDriverId",
                        column: x => x.ThirdDriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResults_Drivers_WinnerDriverId",
                        column: x => x.WinnerDriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceResults_RaceWeekends_RaceWeekendId",
                        column: x => x.RaceWeekendId,
                        principalTable: "RaceWeekends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId",
                table: "Comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_RaceWeekendId",
                table: "Comments",
                column: "RaceWeekendId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TeamId",
                table: "Drivers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_PredictedFastestLapDriverId",
                table: "Predictions",
                column: "PredictedFastestLapDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_PredictedP1DriverId",
                table: "Predictions",
                column: "PredictedP1DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_PredictedP2DriverId",
                table: "Predictions",
                column: "PredictedP2DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_PredictedP3DriverId",
                table: "Predictions",
                column: "PredictedP3DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_RaceWeekendId",
                table: "Predictions",
                column: "RaceWeekendId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId",
                table: "Predictions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResults_FastestLapDriverId",
                table: "RaceResults",
                column: "FastestLapDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResults_RaceWeekendId",
                table: "RaceResults",
                column: "RaceWeekendId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceResults_SecondDriverId",
                table: "RaceResults",
                column: "SecondDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResults_ThirdDriverId",
                table: "RaceResults",
                column: "ThirdDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResults_WinnerDriverId",
                table: "RaceResults",
                column: "WinnerDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Upgrades_RaceWeekendId",
                table: "Upgrades",
                column: "RaceWeekendId");

            migrationBuilder.CreateIndex(
                name: "IX_Upgrades_TeamId",
                table: "Upgrades",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserId",
                table: "UserBadges",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.DropTable(
                name: "RaceResults");

            migrationBuilder.DropTable(
                name: "Upgrades");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "RaceWeekends");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
