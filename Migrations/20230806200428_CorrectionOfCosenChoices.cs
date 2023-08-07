using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionOfCosenChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_EFIntCollection_ChosenChoicesId",
                table: "Players");

            migrationBuilder.AlterColumn<int>(
                name: "ChosenChoicesId",
                table: "Players",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_EFIntCollection_ChosenChoicesId",
                table: "Players",
                column: "ChosenChoicesId",
                principalTable: "EFIntCollection",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_EFIntCollection_ChosenChoicesId",
                table: "Players");

            migrationBuilder.AlterColumn<int>(
                name: "ChosenChoicesId",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_EFIntCollection_ChosenChoicesId",
                table: "Players",
                column: "ChosenChoicesId",
                principalTable: "EFIntCollection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
