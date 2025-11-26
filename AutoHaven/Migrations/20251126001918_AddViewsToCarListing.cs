using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHaven.Migrations
{
    /// <inheritdoc />
    public partial class AddViewsToCarListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "CarListings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Views",
                table: "CarListings");
        }
    }
}
