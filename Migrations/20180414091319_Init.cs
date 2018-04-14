using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ferrous.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Building",
                columns: table => new
                {
                    Location = table.Column<string>(nullable: false),
                    DefaultCapacity = table.Column<int>(nullable: false),
                    LocationFullName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Building", x => x.Location);
                });

            migrationBuilder.CreateTable(
                name: "Contingents",
                columns: table => new
                {
                    ContingentLeaderNo = table.Column<string>(nullable: false),
                    Remark = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contingents", x => x.ContingentLeaderNo);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Capacity = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    LocationExtra = table.Column<string>(nullable: true),
                    LockNo = table.Column<string>(nullable: true),
                    Remark = table.Column<string>(nullable: true),
                    RoomName = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Room_Building_Location",
                        column: x => x.Location,
                        principalTable: "Building",
                        principalColumn: "Location",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContingentArrival",
                columns: table => new
                {
                    ContingentArrivalNo = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ContingentLeaderNo = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Female = table.Column<int>(nullable: true),
                    FemaleOnSpot = table.Column<int>(nullable: true),
                    Male = table.Column<int>(nullable: true),
                    MaleOnSpot = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContingentArrival", x => x.ContingentArrivalNo);
                    table.ForeignKey(
                        name: "FK_ContingentArrival_Contingents_ContingentLeaderNo",
                        column: x => x.ContingentLeaderNo,
                        principalTable: "Contingents",
                        principalColumn: "ContingentLeaderNo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Mino = table.Column<string>(nullable: false),
                    College = table.Column<string>(nullable: true),
                    ContingentLeaderNo = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Sex = table.Column<string>(maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Mino);
                    table.ForeignKey(
                        name: "FK_Person_Contingents_ContingentLeaderNo",
                        column: x => x.ContingentLeaderNo,
                        principalTable: "Contingents",
                        principalColumn: "ContingentLeaderNo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomAllocation",
                columns: table => new
                {
                    Sno = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ContingentArrivalNo = table.Column<int>(nullable: true),
                    ContingentLeaderNo = table.Column<string>(nullable: true),
                    Partial = table.Column<int>(nullable: false),
                    RoomId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAllocation", x => x.Sno);
                    table.ForeignKey(
                        name: "FK_RoomAllocation_ContingentArrival_ContingentArrivalNo",
                        column: x => x.ContingentArrivalNo,
                        principalTable: "ContingentArrival",
                        principalColumn: "ContingentArrivalNo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomAllocation_Contingents_ContingentLeaderNo",
                        column: x => x.ContingentLeaderNo,
                        principalTable: "Contingents",
                        principalColumn: "ContingentLeaderNo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomAllocation_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContingentArrival_ContingentLeaderNo",
                table: "ContingentArrival",
                column: "ContingentLeaderNo");

            migrationBuilder.CreateIndex(
                name: "IX_Person_ContingentLeaderNo",
                table: "Person",
                column: "ContingentLeaderNo");

            migrationBuilder.CreateIndex(
                name: "IX_Room_Location",
                table: "Room",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAllocation_ContingentArrivalNo",
                table: "RoomAllocation",
                column: "ContingentArrivalNo");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAllocation_ContingentLeaderNo",
                table: "RoomAllocation",
                column: "ContingentLeaderNo");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAllocation_RoomId",
                table: "RoomAllocation",
                column: "RoomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "RoomAllocation");

            migrationBuilder.DropTable(
                name: "ContingentArrival");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Contingents");

            migrationBuilder.DropTable(
                name: "Building");
        }
    }
}
