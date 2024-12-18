using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthConnect.Migrations
{
    /// <inheritdoc />
    public partial class slotduration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotDuration",
                table: "DoctorAvailability",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ClinicImagePath",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotDuration",
                table: "DoctorAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "ClinicImagePath",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
