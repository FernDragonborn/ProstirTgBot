using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProstirTgBot.Migrations
{
    /// <inheritdoc />
    public partial class ChangedIdTypeToIntInGameEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InGameEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<int>(type: "int", nullable: false),
                    Apartment = table.Column<int>(type: "int", nullable: false),
                    ActivitiesFound = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InGameEvents", x => x.Id);
                });

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
                    InGameEventId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.DropTable(
                name: "InGameEvents");
        }
    }
}
