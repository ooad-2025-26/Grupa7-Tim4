using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZamETF.Migrations
{
    /// <inheritdoc />
    public partial class DodajFajlBytesNaPredaju : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Fajl",
                table: "PredajeZadace",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);

            migrationBuilder.AddColumn<byte[]>(
                name: "FajlBytes",
                table: "PredajeZadace",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "FajlIme",
                table: "PredajeZadace",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FajlBytes",
                table: "PredajeZadace");

            migrationBuilder.DropColumn(
                name: "FajlIme",
                table: "PredajeZadace");

            migrationBuilder.AlterColumn<string>(
                name: "Fajl",
                table: "PredajeZadace",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
