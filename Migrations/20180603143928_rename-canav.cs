using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ferrous.Migrations
{
    public partial class renamecanav : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~",
                table: "CAPerson");

            migrationBuilder.RenameColumn(
                name: "ContingentArrivalNavigationContingentArrivalNo",
                table: "CAPerson",
                newName: "CANavContingentArrivalNo");

            migrationBuilder.RenameIndex(
                name: "IX_CAPerson_ContingentArrivalNavigationContingentArrivalNo",
                table: "CAPerson",
                newName: "IX_CAPerson_CANavContingentArrivalNo");

            migrationBuilder.AddForeignKey(
                name: "FK_CAPerson_ContingentArrival_CANavContingentArrivalNo",
                table: "CAPerson",
                column: "CANavContingentArrivalNo",
                principalTable: "ContingentArrival",
                principalColumn: "ContingentArrivalNo",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPerson_ContingentArrival_CANavContingentArrivalNo",
                table: "CAPerson");

            migrationBuilder.RenameColumn(
                name: "CANavContingentArrivalNo",
                table: "CAPerson",
                newName: "ContingentArrivalNavigationContingentArrivalNo");

            migrationBuilder.RenameIndex(
                name: "IX_CAPerson_CANavContingentArrivalNo",
                table: "CAPerson",
                newName: "IX_CAPerson_ContingentArrivalNavigationContingentArrivalNo");

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
