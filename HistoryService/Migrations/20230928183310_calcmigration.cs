using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoryService.Migrations
{
    /// <inheritdoc />
    public partial class calcmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Operand1",
                table: "Calculations");

            migrationBuilder.DropColumn(
                name: "Operand2",
                table: "Calculations");

            migrationBuilder.AddColumn<string>(
                name: "Expression",
                table: "Calculations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expression",
                table: "Calculations");

            migrationBuilder.AddColumn<double>(
                name: "Operand1",
                table: "Calculations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Operand2",
                table: "Calculations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
