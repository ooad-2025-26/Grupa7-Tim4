using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zamger.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrvaMigracija : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administrator",
                columns: table => new
                {
                    AdministratorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrator", x => x.AdministratorId);
                });

            migrationBuilder.CreateTable(
                name: "Ispit",
                columns: table => new
                {
                    IspitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ispit", x => x.IspitId);
                });

            migrationBuilder.CreateTable(
                name: "Korisnik",
                columns: table => new
                {
                    KorisnikId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.KorisnikId);
                });

            migrationBuilder.CreateTable(
                name: "Profesor",
                columns: table => new
                {
                    ProfesorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesor", x => x.ProfesorId);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    BrojIndeksa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vrsta = table.Column<int>(type: "int", nullable: false),
                    GodinaStudija = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.BrojIndeksa);
                });

            migrationBuilder.CreateTable(
                name: "StudentService",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentService", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Predmet",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ECTS = table.Column<double>(type: "float", nullable: false),
                    ProfesorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmet", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Predmet_Profesor_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesor",
                        principalColumn: "ProfesorId");
                });

            migrationBuilder.CreateTable(
                name: "PredajaZadace",
                columns: table => new
                {
                    ZadacaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumPredaje = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredajaZadace", x => x.ZadacaId);
                    table.ForeignKey(
                        name: "FK_PredajaZadace_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrijavaIspit",
                columns: table => new
                {
                    PrijavaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    IspitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrijavaIspit", x => x.PrijavaId);
                    table.ForeignKey(
                        name: "FK_PrijavaIspit_Ispit_IspitId",
                        column: x => x.IspitId,
                        principalTable: "Ispit",
                        principalColumn: "IspitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrijavaIspit_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZahtjevZaDokument",
                columns: table => new
                {
                    ZahtjevId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipDokumenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumZahtjeva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZahtjevZaDokument", x => x.ZahtjevId);
                    table.ForeignKey(
                        name: "FK_ZahtjevZaDokument_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UpisNaPredmet",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpisNaPredmet", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UpisNaPredmet_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UpisNaPredmet_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PredajaZadace_StudentId",
                table: "PredajaZadace",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_ProfesorId",
                table: "Predmet",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijavaIspit_IspitId",
                table: "PrijavaIspit",
                column: "IspitId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijavaIspit_StudentId",
                table: "PrijavaIspit",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_UpisNaPredmet_PredmetId",
                table: "UpisNaPredmet",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_UpisNaPredmet_StudentId",
                table: "UpisNaPredmet",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZahtjevZaDokument_StudentId",
                table: "ZahtjevZaDokument",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrator");

            migrationBuilder.DropTable(
                name: "Korisnik");

            migrationBuilder.DropTable(
                name: "PredajaZadace");

            migrationBuilder.DropTable(
                name: "PrijavaIspit");

            migrationBuilder.DropTable(
                name: "StudentService");

            migrationBuilder.DropTable(
                name: "UpisNaPredmet");

            migrationBuilder.DropTable(
                name: "ZahtjevZaDokument");

            migrationBuilder.DropTable(
                name: "Ispit");

            migrationBuilder.DropTable(
                name: "Predmet");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Profesor");
        }
    }
}
