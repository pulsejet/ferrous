using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ferrous.Migrations
{
    public partial class caperson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "ContingentArrival",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "ContingentArrival",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CAPerson",
                columns: table => new
                {
                    Sno = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ContingentArrivalNavigationContingentArrivalNo = table.Column<int>(nullable: true),
                    Mino = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPerson", x => x.Sno);
                    table.ForeignKey(
                        name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                        column: x => x.ContingentArrivalNavigationContingentArrivalNo,
                        principalTable: "ContingentArrival",
                        principalColumn: "ContingentArrivalNo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CAPerson_ContingentArrivalNavigationContingentArrivalNo",
                table: "CAPerson",
                column: "ContingentArrivalNavigationContingentArrivalNo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAPerson");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "ContingentArrival");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "ContingentArrival");
        }
    }
}
