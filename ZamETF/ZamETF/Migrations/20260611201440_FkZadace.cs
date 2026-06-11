using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZamETF.Migrations
{
    /// <inheritdoc />
    public partial class FkZadace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ispiti_Predmeti_PredmetId",
                table: "Ispiti");

            migrationBuilder.DropForeignKey(
                name: "FK_PredajeZadace_AspNetUsers_StudentId",
                table: "PredajeZadace");

            migrationBuilder.DropForeignKey(
                name: "FK_Zadace_Predmeti_PredmetId",
                table: "Zadace");

            migrationBuilder.RenameColumn(
                name: "PredmetId",
                table: "Zadace",
                newName: "PredmetID");

            migrationBuilder.RenameIndex(
                name: "IX_Zadace_PredmetId",
                table: "Zadace",
                newName: "IX_Zadace_PredmetID");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "PredajeZadace",
                newName: "StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_PredajeZadace_StudentId",
                table: "PredajeZadace",
                newName: "IX_PredajeZadace_StudentID");

            migrationBuilder.AlterColumn<int>(
                name: "ProfesorId",
                table: "Predmeti",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispiti_Predmeti_PredmetId",
                table: "Ispiti",
                column: "PredmetId",
                principalTable: "Predmeti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PredajeZadace_AspNetUsers_StudentID",
                table: "PredajeZadace",
                column: "StudentID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zadace_Predmeti_PredmetID",
                table: "Zadace",
                column: "PredmetID",
                principalTable: "Predmeti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ispiti_Predmeti_PredmetId",
                table: "Ispiti");

            migrationBuilder.DropForeignKey(
                name: "FK_PredajeZadace_AspNetUsers_StudentID",
                table: "PredajeZadace");

            migrationBuilder.DropForeignKey(
                name: "FK_Zadace_Predmeti_PredmetID",
                table: "Zadace");

            migrationBuilder.RenameColumn(
                name: "PredmetID",
                table: "Zadace",
                newName: "PredmetId");

            migrationBuilder.RenameIndex(
                name: "IX_Zadace_PredmetID",
                table: "Zadace",
                newName: "IX_Zadace_PredmetId");

            migrationBuilder.RenameColumn(
                name: "StudentID",
                table: "PredajeZadace",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_PredajeZadace_StudentID",
                table: "PredajeZadace",
                newName: "IX_PredajeZadace_StudentId");

            migrationBuilder.AlterColumn<int>(
                name: "ProfesorId",
                table: "Predmeti",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ispiti_Predmeti_PredmetId",
                table: "Ispiti",
                column: "PredmetId",
                principalTable: "Predmeti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PredajeZadace_AspNetUsers_StudentId",
                table: "PredajeZadace",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zadace_Predmeti_PredmetId",
                table: "Zadace",
                column: "PredmetId",
                principalTable: "Predmeti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
