using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreRewards.Migrations
{
    /// <inheritdoc />
    public partial class AddingMarketers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Marketers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SWIFTCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductQuery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GainedWatchers = table.Column<int>(type: "int", nullable: false),
                    GainedBuyers = table.Column<int>(type: "int", nullable: false),
                    AvailableCommissionBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CyclicCommissionBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marketers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Marketers");
        }
    }
}
