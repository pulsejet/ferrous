using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferrous.Migrations
{
    public partial class onspotdemand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FemaleOnSpotDemand",
                table: "ContingentArrival",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaleOnSpotDemand",
                table: "ContingentArrival",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FemaleOnSpotDemand",
                table: "ContingentArrival");

            migrationBuilder.DropColumn(
                name: "MaleOnSpotDemand",
                table: "ContingentArrival");
        }
    }
}
