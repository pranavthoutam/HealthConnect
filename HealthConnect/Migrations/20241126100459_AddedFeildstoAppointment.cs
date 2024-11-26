using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddedFeildstoAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");
        }
    }
}
