using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZamETF.Migrations
{
    /// <inheritdoc />
    public partial class DodajPoljaStudenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Datum",
                table: "Prisustva",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "Bodovi",
                table: "PredajeZadace",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Ciklus",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumRodjenja",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImeMajke",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImeOca",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JMBG",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MjestoPrebivalisca",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Odsjek",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semestar",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusStudenta",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipStudija",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Datum",
                table: "Prisustva");

            migrationBuilder.DropColumn(
                name: "Ciklus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DatumRodjenja",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImeMajke",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImeOca",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "JMBG",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MjestoPrebivalisca",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Odsjek",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Semestar",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StatusStudenta",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TipStudija",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "Bodovi",
                table: "PredajeZadace",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
