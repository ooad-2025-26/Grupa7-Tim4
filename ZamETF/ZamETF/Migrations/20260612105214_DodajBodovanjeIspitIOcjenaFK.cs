using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZamETF.Migrations
{
    /// <inheritdoc />
    public partial class DodajBodovanjeIspitIOcjenaFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatumUnosa",
                table: "Ocjene",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "JeFinalna",
                table: "Ocjene",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BodovanjaIspit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Bodovi = table.Column<int>(type: "int", nullable: false),
                    DatumUnosa = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodovanjaIspit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BodovanjaIspit_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BodovanjaIspit_Predmeti_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmeti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BodovanjaIspit_PredmetId",
                table: "BodovanjaIspit",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_BodovanjaIspit_StudentId",
                table: "BodovanjaIspit",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodovanjaIspit");

            migrationBuilder.DropColumn(
                name: "DatumUnosa",
                table: "Ocjene");

            migrationBuilder.DropColumn(
                name: "JeFinalna",
                table: "Ocjene");
        }
    }
}
