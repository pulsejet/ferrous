using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferrous.Migrations
{
    public partial class allottedca : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson");

            migrationBuilder.AddColumn<int>(
                name: "allottedCAContingentArrivalNo",
                table: "Person",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_allottedCAContingentArrivalNo",
                table: "Person",
                column: "allottedCAContingentArrivalNo");

            migrationBuilder.AddForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson",
                column: "ContingentArrivalNavigationContingentArrivalNo",
                principalTable: "ContingentArrival",
                principalColumn: "ContingentArrivalNo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_ContingentArrival_allottedCAContingentArrivalNo",
                table: "Person",
                column: "allottedCAContingentArrivalNo",
                principalTable: "ContingentArrival",
                principalColumn: "ContingentArrivalNo",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_ContingentArrival_allottedCAContingentArrivalNo",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_allottedCAContingentArrivalNo",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "allottedCAContingentArrivalNo",
                table: "Person");

            migrationBuilder.AddForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson",
                column: "ContingentArrivalNavigationContingentArrivalNo",
                principalTable: "ContingentArrival",
                principalColumn: "ContingentArrivalNo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
