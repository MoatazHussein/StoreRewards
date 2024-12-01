using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreRewards.Migrations
{
    /// <inheritdoc />
    public partial class AddingUserRegionAndPicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);


            migrationBuilder.AddColumn<string>(
               name: "ProfileImagePath",
               table: "Users",
               type: "nvarchar(max)",
               nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marketers_UserId",
                table: "Marketers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marketers_Users_UserId",
                table: "Marketers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marketers_Users_UserId",
                table: "Marketers");

            migrationBuilder.DropIndex(
                name: "IX_Marketers_UserId",
                table: "Marketers");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Users");
        }
    }
}
