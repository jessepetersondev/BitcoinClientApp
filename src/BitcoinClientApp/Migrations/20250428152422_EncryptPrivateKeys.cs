using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitcoinClientApp.Migrations
{
    /// <inheritdoc />
    public partial class EncryptPrivateKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrivateKey",
                table: "Wallets",
                newName: "Salt");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedPrivateKey",
                table: "Wallets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedPrivateKey",
                table: "Wallets");

            migrationBuilder.RenameColumn(
                name: "Salt",
                table: "Wallets",
                newName: "PrivateKey");
        }
    }
}
