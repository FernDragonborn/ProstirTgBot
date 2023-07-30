using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class RemadeInGameEventAddListOfChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Energy",
                table: "InGameEvents");

            migrationBuilder.DropColumn(
                name: "Happiness",
                table: "InGameEvents");

            migrationBuilder.DropColumn(
                name: "Health",
                table: "InGameEvents");

            migrationBuilder.DropColumn(
                name: "Money",
                table: "InGameEvents");

            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "InGameEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "EventDescription",
                table: "InGameEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InGameEventChoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChoiceDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Energy = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Happiness = table.Column<int>(type: "int", nullable: false),
                    Money = table.Column<int>(type: "int", nullable: false),
                    InGameEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InGameEventChoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InGameEventChoice_InGameEvents_InGameEventId",
                        column: x => x.InGameEventId,
                        principalTable: "InGameEvents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InGameEventChoice_InGameEventId",
                table: "InGameEventChoice",
                column: "InGameEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InGameEventChoice");

            migrationBuilder.DropColumn(
                name: "EventDescription",
                table: "InGameEvents");

            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "InGameEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "Energy",
                table: "InGameEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Happiness",
                table: "InGameEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Health",
                table: "InGameEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Money",
                table: "InGameEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
