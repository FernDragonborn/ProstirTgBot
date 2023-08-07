using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class AddChosenChoicesInPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChosenChoicesId",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EFIntCollection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFIntCollection", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_ChosenChoicesId",
                table: "Players",
                column: "ChosenChoicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_EFIntCollection_ChosenChoicesId",
                table: "Players",
                column: "ChosenChoicesId",
                principalTable: "EFIntCollection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_EFIntCollection_ChosenChoicesId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "EFIntCollection");

            migrationBuilder.DropIndex(
                name: "IX_Players_ChosenChoicesId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ChosenChoicesId",
                table: "Players");
        }
    }
}
