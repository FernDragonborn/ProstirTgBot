using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class AddDEpendsOnChoiceInInGameEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InGameEventChoice_InGameEvents_InGameEventId",
                table: "InGameEventChoice");

            migrationBuilder.RenameColumn(
                name: "ActivitiesFound",
                table: "InGameEvents",
                newName: "DependsOnChoice");

            migrationBuilder.AlterColumn<int>(
                name: "InGameEventId",
                table: "InGameEventChoice",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InGameEventChoice_InGameEvents_InGameEventId",
                table: "InGameEventChoice",
                column: "InGameEventId",
                principalTable: "InGameEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InGameEventChoice_InGameEvents_InGameEventId",
                table: "InGameEventChoice");

            migrationBuilder.RenameColumn(
                name: "DependsOnChoice",
                table: "InGameEvents",
                newName: "ActivitiesFound");

            migrationBuilder.AlterColumn<int>(
                name: "InGameEventId",
                table: "InGameEventChoice",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_InGameEventChoice_InGameEvents_InGameEventId",
                table: "InGameEventChoice",
                column: "InGameEventId",
                principalTable: "InGameEvents",
                principalColumn: "Id");
        }
    }
}
