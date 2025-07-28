using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipesRepacoSayco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSP6andPV6ToBatchProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "BatchProcesses");

            migrationBuilder.AddColumn<float>(
                name: "ActualValue5",
                table: "BatchProcesses",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ActualValue6",
                table: "BatchProcesses",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Batch",
                table: "BatchProcesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RecipeName",
                table: "BatchProcesses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "Setpoint5",
                table: "BatchProcesses",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Setpoint6",
                table: "BatchProcesses",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualValue5",
                table: "BatchProcesses");

            migrationBuilder.DropColumn(
                name: "ActualValue6",
                table: "BatchProcesses");

            migrationBuilder.DropColumn(
                name: "Batch",
                table: "BatchProcesses");

            migrationBuilder.DropColumn(
                name: "RecipeName",
                table: "BatchProcesses");

            migrationBuilder.DropColumn(
                name: "Setpoint5",
                table: "BatchProcesses");

            migrationBuilder.DropColumn(
                name: "Setpoint6",
                table: "BatchProcesses");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "BatchProcesses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
