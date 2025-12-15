using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixUserBestResultRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBestResults_AspNetUsers_UserId1",
                table: "UserBestResults");

            migrationBuilder.DropIndex(
                name: "IX_UserBestResults_UserId1",
                table: "UserBestResults");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserBestResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserBestResults",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBestResults_UserId1",
                table: "UserBestResults",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBestResults_AspNetUsers_UserId1",
                table: "UserBestResults",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
