using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsSearchedActivitioesToday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSearchedForActivitiesToday",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSearchedForActivitiesToday",
                table: "Users");
        }
    }
}
