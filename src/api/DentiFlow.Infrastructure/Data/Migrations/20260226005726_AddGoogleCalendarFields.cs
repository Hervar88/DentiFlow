using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentiFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleCalendarFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleCalendarToken",
                table: "Dentistas");

            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarAccessToken",
                table: "Dentistas",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GoogleCalendarConnected",
                table: "Dentistas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarEmail",
                table: "Dentistas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarRefreshToken",
                table: "Dentistas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GoogleCalendarTokenExpiry",
                table: "Dentistas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleCalendarAccessToken",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "GoogleCalendarConnected",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "GoogleCalendarEmail",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "GoogleCalendarRefreshToken",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "GoogleCalendarTokenExpiry",
                table: "Dentistas");

            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarToken",
                table: "Dentistas",
                type: "text",
                nullable: true);
        }
    }
}
