﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Tangy.Data.Migrations
{
    public partial class AddLockoutReasonToTangyUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LockoutReason",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockoutReason",
                table: "AspNetUsers");
        }
    }
}
