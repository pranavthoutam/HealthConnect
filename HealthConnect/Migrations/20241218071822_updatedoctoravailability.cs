using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthConnect.Migrations
{
    /// <inheritdoc />
    public partial class updatedoctoravailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "DoctorAvailability",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_Doctors_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_Doctors_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailability_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "DoctorAvailability");
        }
    }
}
