using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZamETF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminZahtjevi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministratorId = table.Column<int>(type: "int", nullable: false),
                    ZahtjevId = table.Column<int>(type: "int", nullable: false),
                    VrstaZahtjeva = table.Column<int>(type: "int", nullable: false),
                    Komentar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Obradjen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminZahtjevi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bodovanja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    Bodovi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bodovanja", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ispiti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RokZaPrijavu = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ispiti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Korisnici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lozinka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uloga = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Indeks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GodinaStudija = table.Column<int>(type: "int", nullable: true),
                    PredmetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnici", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Predmeti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SifraPredmeta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfesorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmeti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predmeti_Korisnici_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrijaveIspita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    IspitId = table.Column<int>(type: "int", nullable: false),
                    DatumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrijaveIspita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrijaveIspita_Ispiti_IspitId",
                        column: x => x.IspitId,
                        principalTable: "Ispiti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrijaveIspita_Korisnici_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZahtjeviDokumenata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TipDokumenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    StudentskaSluzbaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZahtjeviDokumenata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZahtjeviDokumenata_Korisnici_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZahtjeviDokumenata_Korisnici_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "Korisnici",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ocjene",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    Vrijednost = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocjene", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ocjene_Korisnici_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ocjene_Predmeti_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmeti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prisustva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    Prisutan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prisustva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prisustva_Korisnici_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prisustva_Predmeti_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmeti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UpisaNaPredmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    DatumUpisa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GodinaStudija = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpisaNaPredmet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpisaNaPredmet_Korisnici_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UpisaNaPredmet_Predmeti_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmeti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Zadace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NazivID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rok = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zadace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zadace_Predmeti_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmeti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PredajeZadace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ZadacaId = table.Column<int>(type: "int", nullable: false),
                    DatumPredaje = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fajl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Komentar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bodovi = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredajeZadace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredajeZadace_Korisnici_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PredajeZadace_Zadace_ZadacaId",
                        column: x => x.ZadacaId,
                        principalTable: "Zadace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminZahtjevi_AdministratorId",
                table: "AdminZahtjevi",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminZahtjevi_ZahtjevId",
                table: "AdminZahtjevi",
                column: "ZahtjevId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodovanja_PredmetId",
                table: "Bodovanja",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodovanja_StudentId",
                table: "Bodovanja",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispiti_PredmetId",
                table: "Ispiti",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_PredmetId",
                table: "Korisnici",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjene_PredmetId",
                table: "Ocjene",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjene_StudentId",
                table: "Ocjene",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PredajeZadace_StudentId",
                table: "PredajeZadace",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PredajeZadace_ZadacaId",
                table: "PredajeZadace",
                column: "ZadacaId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmeti_ProfesorId",
                table: "Predmeti",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveIspita_IspitId",
                table: "PrijaveIspita",
                column: "IspitId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveIspita_StudentId",
                table: "PrijaveIspita",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prisustva_PredmetId",
                table: "Prisustva",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Prisustva_StudentId",
                table: "Prisustva",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_UpisaNaPredmet_PredmetId",
                table: "UpisaNaPredmet",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_UpisaNaPredmet_StudentId",
                table: "UpisaNaPredmet",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Zadace_PredmetId",
                table: "Zadace",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_ZahtjeviDokumenata_StudentId",
                table: "ZahtjeviDokumenata",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZahtjeviDokumenata_StudentskaSluzbaId",
                table: "ZahtjeviDokumenata",
                column: "StudentskaSluzbaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminZahtjevi_Korisnici_AdministratorId",
                table: "AdminZahtjevi",
                column: "AdministratorId",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminZahtjevi_ZahtjeviDokumenata_ZahtjevId",
                table: "AdminZahtjevi",
                column: "ZahtjevId",
                principalTable: "ZahtjeviDokumenata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bodovanja_Korisnici_StudentId",
                table: "Bodovanja",
                column: "StudentId",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bodovanja_Predmeti_PredmetId",
                table: "Bodovanja",
                column: "PredmetId",
                principalTable: "Predmeti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ispiti_Predmeti_PredmetId",
                table: "Ispiti",
                column: "PredmetId",
                principalTable: "Predmeti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnici_Predmeti_PredmetId",
                table: "Korisnici",
                column: "PredmetId",
                principalTable: "Predmeti",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmeti_Korisnici_ProfesorId",
                table: "Predmeti");

            migrationBuilder.DropTable(
                name: "AdminZahtjevi");

            migrationBuilder.DropTable(
                name: "Bodovanja");

            migrationBuilder.DropTable(
                name: "Ocjene");

            migrationBuilder.DropTable(
                name: "PredajeZadace");

            migrationBuilder.DropTable(
                name: "PrijaveIspita");

            migrationBuilder.DropTable(
                name: "Prisustva");

            migrationBuilder.DropTable(
                name: "UpisaNaPredmet");

            migrationBuilder.DropTable(
                name: "ZahtjeviDokumenata");

            migrationBuilder.DropTable(
                name: "Zadace");

            migrationBuilder.DropTable(
                name: "Ispiti");

            migrationBuilder.DropTable(
                name: "Korisnici");

            migrationBuilder.DropTable(
                name: "Predmeti");
        }
    }
}
