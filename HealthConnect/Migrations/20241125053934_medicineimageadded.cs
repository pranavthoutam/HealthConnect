using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthConnect.Migrations
{
    /// <inheritdoc />
    public partial class medicineimageadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Medicines",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Medicines");
        }
    }
}
