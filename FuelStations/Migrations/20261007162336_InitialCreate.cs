using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FuelStations.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FuelTypes",
            columns: table => new
            {
                Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FuelTypes", x => x.Code);
                table.CheckConstraint("CK_FuelTypes_Code_Format", "\"Code\" ~ '^[A-Z0-9_]{2,20}$'");
            });

        migrationBuilder.CreateTable(
            name: "Stations",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Stations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "StationFuels",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StationId = table.Column<int>(type: "integer", nullable: false),
                FuelTypeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                PriceUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StationFuels", x => x.Id);
                table.CheckConstraint("CK_StationFuels_Price_Positive", "\"Price\" > 0");
                table.ForeignKey(
                    name: "FK_StationFuels_FuelTypes_FuelTypeCode",
                    column: x => x.FuelTypeCode,
                    principalTable: "FuelTypes",
                    principalColumn: "Code",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StationFuels_Stations_StationId",
                    column: x => x.StationId,
                    principalTable: "Stations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StationTerminals",
            columns: table => new
            {
                Number = table.Column<int>(type: "integer", nullable: false),
                StationId = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StationTerminals", x => x.Number);
                table.CheckConstraint("CK_StationTerminals_Number_Range", "\"Number\" BETWEEN 1000 AND 9999");
                table.ForeignKey(
                    name: "FK_StationTerminals_Stations_StationId",
                    column: x => x.StationId,
                    principalTable: "Stations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FuelTypes_Name",
            table: "FuelTypes",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StationFuels_FuelTypeCode",
            table: "StationFuels",
            column: "FuelTypeCode");

        migrationBuilder.CreateIndex(
            name: "IX_StationFuels_StationId_FuelTypeCode",
            table: "StationFuels",
            columns: new[] { "StationId", "FuelTypeCode" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Stations_City",
            table: "Stations",
            column: "City");

        migrationBuilder.CreateIndex(
            name: "IX_Stations_Name",
            table: "Stations",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StationTerminals_StationId",
            table: "StationTerminals",
            column: "StationId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StationFuels");

        migrationBuilder.DropTable(
            name: "StationTerminals");

        migrationBuilder.DropTable(
            name: "FuelTypes");

        migrationBuilder.DropTable(
            name: "Stations");
    }
}
