using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipesRepacoSayco.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Setpoint1 = table.Column<float>(type: "real", nullable: true),
                    ActualValue1 = table.Column<float>(type: "real", nullable: true),
                    Setpoint2 = table.Column<float>(type: "real", nullable: true),
                    ActualValue2 = table.Column<float>(type: "real", nullable: true),
                    Setpoint3 = table.Column<float>(type: "real", nullable: true),
                    ActualValue3 = table.Column<float>(type: "real", nullable: true),
                    Setpoint4 = table.Column<float>(type: "real", nullable: true),
                    ActualValue4 = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchProcesses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchProcesses");
        }
    }
}
