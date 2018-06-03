using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferrous.Migrations
{
    public partial class casex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationContingentArrivalNo",
                table: "CAPerson");

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "CAPerson",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson",
                column: "ContingentArrivalNavigationContingentArrivalNo",
                principalTable: "ContingentArrival",
                principalColumn: "ContingentArrivalNo",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "CAPerson");

            migrationBuilder.AddForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationContingentArrivalNo",
                table: "CAPerson",
                column: "ContingentArrivalNavigationContingentArrivalNo",
                principalTable: "ContingentArrival",
                principalColumn: "ContingentArrivalNo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
