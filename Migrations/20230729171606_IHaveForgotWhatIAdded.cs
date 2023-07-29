using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class IHaveForgotWhatIAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Apartment",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFormFilled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Money",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apartment",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFormFilled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Money",
                table: "Users");
        }
    }
}
