using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitcoinClientApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressToWalletsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Wallets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Wallets");
        }
    }
}
