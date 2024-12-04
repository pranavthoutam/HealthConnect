using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthConnect.Migrations
{
    /// <inheritdoc />
    public partial class consultationlink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultationLink",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationLink",
                table: "Appointments");
        }
    }
}
